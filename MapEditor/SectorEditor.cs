﻿using SlimDX;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using FunAndGamesWithSlimDX.Entities;
using System.Collections.Generic;
using System;
using SlimDX.Direct3D11;
using System.Windows.Input;
using FunAndGamesWithSlimDX.DirectX;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.Windows.Markup;
using System.Xml;
using System.Configuration;
using GameData;

namespace MapEditor
{
    public class SectorEditor
    {
        private Canvas _canvas;
        private Line _line;
        private Line _tempLine;
        private Brush _lineBrush;
        private Brush _selectedLineBrush;
        private Brush _sectorBrush;
        private List<Tuple<Line, GameData.LineSegment>> lineList = new List<Tuple<Line, GameData.LineSegment>>();
        private List<Tuple<Line, GameData.LineSegment>> currentSectorLineList = new List<Tuple<Line, GameData.LineSegment>>();
        private List<Tuple<Shape, Mesh>> meshList = new List<Tuple<Shape, Mesh>>();
        private Device _device;
        private IShader _shader;
        private float _midWidth;
        private float _midHeight;
        private float _gridSize;
        private GameData.MapData _mapData;
        private bool _sectorInComplete = true;

        private List<Tuple<int, Line>> Lines = new List<Tuple<int, Line>>();

        private List<Tuple<Sector, Line>> Sectors = new List<Tuple<Sector, Line>>();

        private Line SelectedLine { get; set; }

        private List<Line> SelectedSectorLines { get; set; }

        private Sector SelectedSector { get; set; }

        public string SelectedTexture { get; set; }

        public Action<GameData.LineSegment> SegmentSelectedAction { get; set; }

        public Action<GameData.Sector> SectorSelectedAction { get; set; }

        public bool SelectSector { get; set; }

        public bool SelectSegment { get; set; }

        public MapData MapData
        {
            get
            {
                return _mapData;
            }

            set
            {
                _mapData = value;
            }
        }

        public SectorEditor(Canvas canvas, Device device, IShader shader, float midWidth, float midHeight, float gridSize, GameData.MapData mapData)
        {
            _lineBrush = new SolidColorBrush(Color.FromRgb(0, 128, 128));
            _selectedLineBrush = new SolidColorBrush(Color.FromRgb(128, 0, 0));
            _sectorBrush = new SolidColorBrush(Color.FromRgb(255, 255, 0));
            _canvas = canvas;
            _device = device;
            _shader = shader;
            _midHeight = midHeight;
            _midWidth = midWidth;
            _gridSize = gridSize;
            MapData = mapData;
        }

        public SectorEditor(Canvas canvas, Device device, IShader shader, float midWidth, float midHeight, float gridSize)
        {
            _lineBrush = new SolidColorBrush(Color.FromRgb(0, 128, 128));
            _selectedLineBrush = new SolidColorBrush(Color.FromRgb(128, 0, 0));
            _canvas = canvas;
            _device = device;
            _shader = shader;
            _midHeight = midHeight;
            _midWidth = midWidth;
            _gridSize = gridSize;
            MapData = new MapData();
        }

        public List<Tuple<Shape, Mesh>> GetMeshList()
        {
            return meshList;
        }

        public void MoveAction(Point currentMousePosition, Point startPoint, float currentScale, bool snapToGrid, bool snapToClosestPoint)
        {
            var currentPos = currentMousePosition;

            if (_tempLine != null)
            {
                _canvas.Children.Remove(_tempLine);
                //lineList.Remove(lineList.Find(x => x.Line == _tempLine));
            }

            Line line = new Line();
            line.Stroke = _lineBrush;
            line.X1 = startPoint.X;
            line.Y1 = startPoint.Y;
            line.X2 = currentPos.X;
            line.Y2 = currentPos.Y;

            if (snapToGrid)
            {
                SnapToGrid(line, currentScale);
            }
            else if (snapToClosestPoint)
            {
                SnapToClosestPoint(line, 50.0);
            }

            _tempLine = line;

            _tempLine.PreviewMouseDown += OnLineSelected;

            //lineList.Add(new LineDef(line));
            _canvas.Children.Add(line);
        }

        public void AddLineSegment(Point startPoint, float currentScale)
        {
            var lineSegment = new GameData.LineSegment()
            {
                Start = new GameData.Vertex()
                {
                    X = (float)_tempLine.X1,
                    Y = (float)_tempLine.Y1
                },
                End = new GameData.Vertex()
                {
                    X = (float)_tempLine.X2,
                    Y = (float)_tempLine.Y2
                },
                TextureId = MapData.GetTextureId(SelectedTexture)
            };

            //MapData.AddLineSegment(lineSegment);

            lineList.Add(new Tuple<Line, GameData.LineSegment>(_tempLine, lineSegment));
            currentSectorLineList.Add(new Tuple<Line, GameData.LineSegment>(_tempLine, lineSegment));

            //CreateMesh(_tempLine, startPoint, currentScale);

            if (currentSectorLineList.FirstOrDefault() != null)
            {
                var start = currentSectorLineList.First().Item2.Start;

                if (lineSegment.End.X == start.X && lineSegment.End.Y == start.Y
                   && lineSegment.End.Z == start.Z)
                {
                    foreach (var segmentTuple in currentSectorLineList)
                    {
                        segmentTuple.Item1.Stroke = _sectorBrush;
                        MapData.AddLineSegment(segmentTuple.Item2);
                        Lines.Add(new Tuple<int, Line>(segmentTuple.Item2.Id, segmentTuple.Item1));
                    }

                    var newSector = new Sector()
                    {
                        CeilingTextureId = 0,
                        FloorTextureId = 0,
                        Effect = 0,
                        SideDefinitions = currentSectorLineList.Select(x => x.Item2.Id).ToList()
                    };

                    MapData.AddSector(newSector);

                    foreach (var line in currentSectorLineList.Select(x => x.Item1))
                        Sectors.Add(new Tuple<Sector, Line>(newSector, line));

                    _sectorInComplete = false;

                    currentSectorLineList.Clear();
                }
                else
                {
                    _sectorInComplete = true;
                }
            }

            _tempLine = null;
        }

        public bool IsSectorComplete()
        {
            return !_sectorInComplete;
        }

        private void SnapToClosestPoint(Line line, double smallestDistance)
        {
            Point closestPoint = new Point(line.X2, line.Y2);
            double closestDistance = 10000;
            Point endPoint = new Point(line.X2, line.Y2);

            foreach (Line tempLine in lineList.Select(x => x.Item1))
            {
                Point one = new Point(tempLine.X1, tempLine.Y1);
                Point two = new Point(tempLine.X2, tempLine.Y2);

                double distance = Math.Sqrt((two.X - endPoint.X) * (two.X - endPoint.X) + (two.Y - endPoint.Y) * (two.Y - endPoint.Y));

                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestPoint = two;
                }

                distance = Math.Sqrt((one.X - endPoint.X) * (one.X - endPoint.X) + (one.Y - endPoint.Y) * (one.Y - endPoint.Y));

                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestPoint = one;
                }
            }

            if (closestDistance <= smallestDistance)
            {
                line.X2 = closestPoint.X;
                line.Y2 = closestPoint.Y;
            }
        }

        private void SnapToGrid(Line line, float currentScale)
        {
            double scaledGridSize = _gridSize / currentScale;
            int remainder;
            Math.DivRem((int) Math.Round(line.X1), (int) Math.Round(scaledGridSize), out remainder);

            if (remainder <= scaledGridSize / 2)
                line.X1 = Math.Round(line.X1 - (float)remainder);
            else
                line.X1 = Math.Round(line.X1 + (float)(scaledGridSize - remainder));

            Math.DivRem((int) Math.Round(line.Y1), (int) Math.Round(scaledGridSize), out remainder);

            if (remainder <= scaledGridSize / 2)
                line.Y1 = Math.Round(line.Y1 - (float)remainder);
            else
                line.Y1 = Math.Round(line.Y1 + (float)(scaledGridSize - remainder));

            Math.DivRem((int) Math.Round(line.X2), (int) Math.Round(scaledGridSize), out remainder);

            if (remainder <= scaledGridSize / 2)
                line.X2 = Math.Round(line.X2 - (float)remainder);
            else
                line.X2 = Math.Round(line.X2 + (float)(scaledGridSize - remainder));

            Math.DivRem((int) Math.Round(line.Y2), (int) Math.Round(scaledGridSize), out remainder);

            if (remainder <= scaledGridSize / 2)
                line.Y2 = Math.Round(line.Y2 - (float)remainder);
            else
                line.Y2 = Math.Round(line.Y2 + (float)(scaledGridSize - remainder));
        }

        public Point? GetLastPoint()
        {
            if (lineList == null)
                return null;

            var lastSegment = lineList.LastOrDefault();

            if (lastSegment == null)
                return null;

            var point = new Point()
            {
                X = lastSegment.Item2.End.X,
                Y = lastSegment.Item2.End.Y
            };

            return point;
        }

        private void OnLineSelected(object sender, MouseButtonEventArgs args)
        {
            var line = sender as Line;

            if (SelectSegment)
            {
                if (SelectedLine != null)
                    SelectedLine.Stroke = _lineBrush;

                SelectedLine = line;

                line.Stroke = _selectedLineBrush;
                var lineSegment = lineList.Find(x => x.Item1 == line)?.Item2;

                if (lineSegment == null)
                    return;

                if (SegmentSelectedAction != null)
                    SegmentSelectedAction.Invoke(lineSegment);
            }
            else if (SelectSector)
            {
                if (SelectedSectorLines != null)
                {
                    foreach (var sectorLine in SelectedSectorLines)
                        sectorLine.Stroke = _sectorBrush;
                }

                var sector = Sectors.SingleOrDefault(x => x.Item2 == line);

                if (sector == null)
                    return;

                SelectedSector = sector.Item1;
                SelectedSectorLines = Sectors
                                        .Where(x => x.Item1 == sector.Item1)
                                        .Select(x => x.Item2)
                                        .ToList();

                foreach (var selectedSectorLine in SelectedSectorLines)
                    selectedSectorLine.Stroke = _selectedLineBrush;

                if (SectorSelectedAction != null)
                    SectorSelectedAction.Invoke(SelectedSector);

            }
            /*if (!string.IsNullOrEmpty(SelectedTexture))
            {
                var lineSegment = lineList.Find(x => x.Item1 == line).Item2;
                _mapData.AddTextureData(SelectedTexture);
                int textureId = _mapData.GetTextureId(SelectedTexture);
                lineSegment.TextureId = textureId;
                _mapData.UpdateLineSegment(lineSegment);
            }*/

            //_selectedMesh = meshList.Find(x => x.Item1 == line).Item2;
        }

        public void CreateMesh(Shape shape, Point startPoint, float currentScale)
        {
            Line line = shape as Line;

            if (line == null)
                throw new ArgumentException("Parameter shape not a line!");

            Mesh roomMesh = new Mesh(_device, _shader);
            float scaleFactor = currentScale;

            var scaleTransform = line.LayoutTransform as ScaleTransform;
            var translateTransform = line.RenderTransform as TranslateTransform;

            if (scaleTransform != null)
            {
                scaleFactor = (float)scaleTransform.ScaleX;
            }

            roomMesh.SetPosition((float)startPoint.X * scaleFactor, 0.0f, (float)startPoint.Y * scaleFactor);

            Model[] model = new Model[6];
            Vector3[] vectors = new Vector3[4];

            vectors[0].X = ((float)(line.X1 * scaleFactor) - _midWidth);
            vectors[0].Y = 64.0f;
            vectors[0].Z = _midHeight - (float)(line.Y1 * scaleFactor);

            vectors[1].X = ((float)(line.X2 * scaleFactor) - _midWidth);
            vectors[1].Y = 64.0f;
            vectors[1].Z = _midHeight - (float)(line.Y2 * scaleFactor);

            vectors[2].X = ((float)(line.X2 * scaleFactor) - _midWidth);
            vectors[2].Y = 0.0f;
            vectors[2].Z = _midHeight - (float)(line.Y2 * scaleFactor);

            vectors[3].X = ((float)(line.X1 * scaleFactor) - _midWidth);
            vectors[3].Y = 0.0f;
            vectors[3].Z = _midHeight - (float)(line.Y1 * scaleFactor);


            //Indexes for the above square
            short[] faceIndex = new short[6] {
                0, 1, 2, 0, 2,3
            };

            Vector3 normal = Vector3.Cross(vectors[1] - vectors[0], vectors[2] - vectors[1]);
            normal = Vector3.Normalize(normal);

            model[0].x = vectors[0].X;
            model[0].y = vectors[0].Y;
            model[0].z = vectors[0].Z;
            model[0].nx = normal.X;
            model[0].ny = normal.Y;
            model[0].nz = normal.Z;
            model[0].tx = 0.0f;
            model[0].ty = 0.0f;

            model[1].x = vectors[1].X;
            model[1].y = vectors[1].Y;
            model[1].z = vectors[1].Z;
            model[1].nx = normal.X;
            model[1].ny = normal.Y;
            model[1].nz = normal.Z;
            model[1].tx = vectors[1].X / 64;//4.0f; length divided by texture width
            model[1].ty = 0.0f;

            model[2].x = vectors[2].X;
            model[2].y = vectors[2].Y;
            model[2].z = vectors[2].Z;
            model[2].nx = normal.X;
            model[2].ny = normal.Y;
            model[2].nz = normal.Z;
            model[2].tx = vectors[2].X / 64;//4.0f;
            model[2].ty = 1.0f;

            model[3].x = vectors[0].X;
            model[3].y = vectors[0].Y;
            model[3].z = vectors[0].Z;
            model[3].nx = normal.X;
            model[3].ny = normal.Y;
            model[3].nz = normal.Z;
            model[3].tx = 0.0f;
            model[3].ty = 0.0f;

            model[4].x = vectors[2].X;
            model[4].y = vectors[2].Y;
            model[4].z = vectors[2].Z;
            model[4].nx = normal.X;
            model[4].ny = normal.Y;
            model[4].nz = normal.Z;
            model[4].tx = vectors[2].X / 64;//4.0f;
            model[4].ty = 1.0f;

            model[5].x = vectors[3].X;
            model[5].y = vectors[3].Y;
            model[5].z = vectors[3].Z;
            model[5].nx = normal.X;
            model[5].ny = normal.Y;
            model[5].nz = normal.Z;
            model[5].tx = 0.0f;
            model[5].ty = 1.0f;

            roomMesh.LoadVectorsFromModel(model, faceIndex);
            roomMesh.SetScaling(1, 1, 1);
            meshList.Add(new Tuple<Shape, Mesh>(line, roomMesh));

            CreateNormalLine(line, normal);
        }

        private void CreateNormalLine(Line line, Vector3 normal)
        {
            Line normalLine = new Line();

            float midLengthX = (float)Math.Abs(line.X2 - line.X1) / 2;
            float midLengthY = (float)Math.Abs(line.Y2 - line.Y1) / 2;
            float midPointX = 0;
            float midPointY = 0;

            if (line.X2 > line.X1)
                midPointX = (float)line.X1 + midLengthX;
            else
                midPointX = (float)line.X1 - midLengthX;

            if (line.Y2 > line.Y1)
                midPointY = (float)line.Y1 + midLengthY;
            else
                midPointY = (float)line.Y1 - midLengthY;

            normalLine.X1 = midPointX;
            normalLine.Y1 = midPointY;

            normalLine.X2 = midPointX + (10) * normal.X;
            normalLine.Y2 = midPointY - (10) * normal.Z;
            normalLine.Stroke = _lineBrush;

            _canvas.Children.Add(normalLine);
        }

        public Mesh CreateMesh(GameData.LineSegment lineSeg)
        {
            Mesh roomMesh = new Mesh(_device, _shader);

            Model[] model = new Model[6];
            Vector3[] vectors = new Vector3[4];

            GameData.Vertex start = lineSeg.Start;
            GameData.Vertex end = lineSeg.End; 

            vectors[0].X = ((float)(start.X) - _midWidth);
            vectors[0].Y = 64.0f;
            vectors[0].Z = _midHeight - (float)(start.Y);

            vectors[1].X = ((float)(end.X) - _midWidth);
            vectors[1].Y = 64.0f;
            vectors[1].Z = _midHeight - (float)(end.Y);

            vectors[2].X = ((float)(end.X) - _midWidth);
            vectors[2].Y = 0.0f;
            vectors[2].Z = _midHeight - (float)(end.Y);

            vectors[3].X = ((float)(start.X) - _midWidth);
            vectors[3].Y = 0.0f;
            vectors[3].Z = _midHeight - (float)(start.Y);


            //Indexes for the above square
            short[] faceIndex = new short[6] {
                0, 1, 2, 0, 2,3
            };

            Vector3 normal = Vector3.Cross(vectors[1] - vectors[0], vectors[2] - vectors[1]);
            normal = Vector3.Normalize(normal);

            model[0].x = vectors[0].X;
            model[0].y = vectors[0].Y;
            model[0].z = vectors[0].Z;
            model[0].nx = normal.X;
            model[0].ny = normal.Y;
            model[0].nz = normal.Z;
            model[0].tx = 0.0f;
            model[0].ty = 0.0f;

            model[1].x = vectors[1].X;
            model[1].y = vectors[1].Y;
            model[1].z = vectors[1].Z;
            model[1].nx = normal.X;
            model[1].ny = normal.Y;
            model[1].nz = normal.Z;
            model[1].tx = vectors[1].X / 64;//4.0f; length divided by texture width
            model[1].ty = 0.0f;

            model[2].x = vectors[2].X;
            model[2].y = vectors[2].Y;
            model[2].z = vectors[2].Z;
            model[2].nx = normal.X;
            model[2].ny = normal.Y;
            model[2].nz = normal.Z;
            model[2].tx = vectors[2].X / 64;//4.0f;
            model[2].ty = 1.0f;

            model[3].x = vectors[0].X;
            model[3].y = vectors[0].Y;
            model[3].z = vectors[0].Z;
            model[3].nx = normal.X;
            model[3].ny = normal.Y;
            model[3].nz = normal.Z;
            model[3].tx = 0.0f;
            model[3].ty = 0.0f;

            model[4].x = vectors[2].X;
            model[4].y = vectors[2].Y;
            model[4].z = vectors[2].Z;
            model[4].nx = normal.X;
            model[4].ny = normal.Y;
            model[4].nz = normal.Z;
            model[4].tx = vectors[2].X / 64;//4.0f;
            model[4].ty = 1.0f;

            model[5].x = vectors[3].X;
            model[5].y = vectors[3].Y;
            model[5].z = vectors[3].Z;
            model[5].nx = normal.X;
            model[5].ny = normal.Y;
            model[5].nz = normal.Z;
            model[5].tx = 0.0f;
            model[5].ty = 1.0f;

            roomMesh.LoadVectorsFromModel(model, faceIndex);
            roomMesh.SetScaling(1, 1, 1);
            roomMesh.LoadTexture(MapData.TextureData[lineSeg.TextureId]);

            return roomMesh;
        }

        public void SaveLines(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                throw new ArgumentNullException("fileName");

            GameData.MapSaver mapSaver = new GameData.MapSaver(MapData);

            mapSaver.SaveData(fileName);
        }

                
        public void CreateMesh(GameData.LineSegment lineSegment, Point startPoint, float currentScale)
        {
            throw new NotImplementedException();
        }

        public void EditAction(Point startPoint, float currentScale, MapData mapData)
        {
            throw new NotImplementedException();
        }
    }
}
