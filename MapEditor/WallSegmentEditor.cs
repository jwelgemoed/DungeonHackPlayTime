using SharpDX;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using FunAndGamesWithSharpDX.Entities;
using System.Collections.Generic;
using System;
using SharpDX.Direct3D11;
using System.Windows.Input;
using FunAndGamesWithSharpDX.DirectX;
using System.Configuration;
using GameData;
using DungeonHack.Builders;
using DungeonHack.DirectX;
using Point = System.Windows.Point;

namespace MapEditor
{
    public class LineDef
    {
        public Line Line { get; set; }

        public string LineTexture { get; set; }

        public LineDef(Line line, string lineTexture)
        {
            Line = line;
            LineTexture = lineTexture;
        }

        public LineDef(Line line)
        {
            Line = line;
        }
    }

    public class WallSegmentEditor : ISegmentEditor
    {
        private Canvas _canvas;
        private Line _line;
        private Line _tempLine;
        private Brush _lineBrush;
        private Brush _selectedLineBrush;
        private List<Tuple<Line, GameData.LineSegment>> lineList = new List<Tuple<Line, GameData.LineSegment>>();
        private List<Tuple<Shape, FunAndGamesWithSharpDX.Entities.Polygon>> meshList = new List<Tuple<Shape, FunAndGamesWithSharpDX.Entities.Polygon>>();
        private Device _device;
        private Shader _shader;
        private float _midWidth;
        private float _midHeight;
        private float _gridSize;
        private GameData.MapData _mapData;
        private Line SelectedLine { get; set; }

        public string SelectedTexture { get; set; }

        public Action<GameData.LineSegment> WallSelectedAction { get; set; }

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

        public WallSegmentEditor(Canvas canvas, Device device, Shader shader, float midWidth, float midHeight, float gridSize, GameData.MapData mapData)
        {
            _lineBrush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(0, 128, 128));
            _selectedLineBrush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(128, 0, 0));
            _canvas = canvas;
            _device = device;
            _shader = shader;
            _midHeight = midHeight;
            _midWidth = midWidth;
            _gridSize = gridSize;
            MapData = mapData;
        }

        public WallSegmentEditor(Canvas canvas, Device device, Shader shader, float midWidth, float midHeight, float gridSize)
        {
            _lineBrush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(0, 128, 128));
            _selectedLineBrush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(128, 0, 0));
            _canvas = canvas;
            _device = device;
            _shader = shader;
            _midHeight = midHeight;
            _midWidth = midWidth;
            _gridSize = gridSize;
            MapData = new MapData();
        }

        public List<Tuple<Shape, FunAndGamesWithSharpDX.Entities.Polygon>> GetMeshList()
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
            line.X1 = startPoint.X-8;
            line.Y1 = startPoint.Y-8;
            line.X2 = currentPos.X-8;
            line.Y2 = currentPos.Y-8;

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

        public void EditAction(Point startPoint, float currentScale)
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

            MapData.AddLineSegment(lineSegment);

            lineList.Add(new Tuple<Line, GameData.LineSegment>(_tempLine, lineSegment));

            CreateMesh(_tempLine, startPoint, currentScale);

            _tempLine = null;
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
                line.X1 = line.X1 - (float)remainder;
            else
                line.X1 = line.X1 + (float)(scaledGridSize - remainder);

            Math.DivRem((int) Math.Round(line.Y1), (int) Math.Round(scaledGridSize), out remainder);

            if (remainder <= scaledGridSize / 2)
                line.Y1 = line.Y1 - (float)remainder;
            else
                line.Y1 = line.Y1 + (float)(scaledGridSize - remainder);

            Math.DivRem((int) Math.Round(line.X2), (int) Math.Round(scaledGridSize), out remainder);

            if (remainder <= scaledGridSize / 2)
                line.X2 = line.X2 - (float)remainder;
            else
                line.X2 = line.X2 + (float)(scaledGridSize - remainder);

            Math.DivRem((int) Math.Round(line.Y2), (int) Math.Round(scaledGridSize), out remainder);

            if (remainder <= scaledGridSize / 2)
                line.Y2 = line.Y2 - (float)remainder;
            else
                line.Y2 = line.Y2 + (float)(scaledGridSize - remainder);
        }

        private void OnLineSelected(object sender, MouseButtonEventArgs args)
        {
            if (SelectedLine != null)
                SelectedLine.Stroke = _lineBrush;

            var line = sender as Line;

            SelectedLine = line;

            line.Stroke = _selectedLineBrush;
            var lineSegment = lineList.Find(x => x.Item1 == line).Item2;

            if (WallSelectedAction != null)
                WallSelectedAction.Invoke(lineSegment);

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

            float scaleFactor = currentScale;

            var scaleTransform = line.LayoutTransform as ScaleTransform;
            var translateTransform = line.RenderTransform as TranslateTransform;

            if (scaleTransform != null)
            {
                scaleFactor = (float)scaleTransform.ScaleX;
            }

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

            PolygonBuilder meshBuilder = new PolygonBuilder(_device, _shader, new BufferFactory(_device));
            var roomMesh = meshBuilder
                            .New()
                            .SetPosition((float)startPoint.X * scaleFactor, 0.0f, (float)startPoint.Y * scaleFactor)
                            .SetModel(model)
                            .SetScaling(1, 1, 1)
                            .WithTransformToWorld()
                            .Build();
            meshList.Add(new Tuple<Shape, FunAndGamesWithSharpDX.Entities.Polygon>(line, roomMesh));

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

        public void SaveLines(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                throw new ArgumentNullException("fileName");

            GameData.MapSaver mapSaver = new GameData.MapSaver(MapData);

            mapSaver.SaveData(fileName);
        }

        public List<Line> LoadLines(string fileName, bool applyInternalTransform = true, Transform transform = null)
        {
            var lineList = new List<Line>();

            MapData = new MapData();

            var mapSaver = new MapSaver(MapData);

            mapSaver.LoadData(System.IO.Path.Combine(ConfigurationManager.AppSettings["BaseSavePath"], fileName));

            foreach (var lineSeg in MapData.LineSegments.Values)
            {
                Point startPoint = new Point(lineSeg.Start.X, lineSeg.Start.Y);
                Point endPoint = new Point(lineSeg.End.X, lineSeg.End.Y);

                if (transform != null)
                {
                    transform.TryTransform(startPoint, out startPoint);
                    transform.TryTransform(endPoint, out endPoint);
                }

                if (applyInternalTransform)
                {
                    //    line.Transform.TryTransform(startPoint, out startPoint);
                    //   line.Transform.TryTransform(endPoint, out endPoint);
                }

                lineList.Add(new Line()
                {
                    X1 = startPoint.X + _midWidth,
                    Y1 = startPoint.Y + _midHeight,
                    X2 = endPoint.X + _midWidth,
                    Y2 = endPoint.Y + _midHeight,
                    Stroke = _lineBrush
                });
            }

            return lineList;
        }

        public void CreateMesh(GameData.LineSegment lineSegment, Point startPoint, float currentScale)
        {
            throw new NotImplementedException();
        }

        public void EditAction(Point startPoint, float currentScale, GlobalMapData mapData)
        {
            throw new NotImplementedException();
        }
    }
}
