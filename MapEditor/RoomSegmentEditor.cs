using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Shapes;
using FunAndGamesWithSharpDX.Entities;
using System.Windows.Controls;
using System.Windows.Media;
using SharpDX.Direct3D11;
using FunAndGamesWithSharpDX.DirectX;
using GameData;
using System.Configuration;

namespace MapEditor
{
    public class RoomSegmentEditor : BaseSegmentEditor, ISegmentEditor
    {
        private readonly IList<Tuple<GameData.LineSegment, Line>> _lineSegmentList;
        private readonly IList<Line> _tempLineList;
        private IList<GameData.LineSegment> _roomLines;
        private readonly Brush _lineBrush;
        private readonly Canvas _canvas;
        private readonly float _gridSize;

        public RoomSegmentEditor(Canvas canvas, float midWidth, float midHeight, float gridSize)
            : base(midWidth, midHeight)
        {
            _lineSegmentList = new List<Tuple<GameData.LineSegment, Line>>();
            _tempLineList = new List<Line>();
            _canvas = canvas;
            _lineBrush = new SolidColorBrush(Color.FromRgb(0, 128, 128));
            _gridSize = gridSize;
        }

        public void SetLineSegmentList(List<GameData.LineSegment> roomLineSegments)
        {
            _roomLines = roomLineSegments;
        }

        public void EditAction(Point startPoint, float currentScale, GlobalMapData globalMapData)
        {
            _mapData.UpdateStartPosition(new GameData.Vertex()
            {
                X = (float)startPoint.X,
                Y = (float)startPoint.Y
            });

            globalMapData.AddMap(_mapData);

            _lineSegmentList.Clear();
            _tempLineList.Clear();
        }

       
        public void MoveAction(Point currentMousePosition, Point startPoint, float currentScale, bool snapToGrid, bool snapToClosestPoint)
        {
            var currentPos = currentMousePosition;
            var offsetPosition = currentPos;

            if (snapToGrid)
                offsetPosition = SnapToGrid(currentPos, currentScale);

            foreach (var tempLine in _tempLineList)
            {
                if (tempLine != null)
                {
                    _canvas.Children.Remove(tempLine);

                    if (_lineSegmentList.Any(x => x.Item2 == tempLine)) 
                        _lineSegmentList.Remove(_lineSegmentList.First(x => x.Item2 == tempLine));
                }
            }

            foreach (var roomLine in _roomLines)
            {
                Line line = new Line();
                line.Stroke = _lineBrush;

                var newRoom = new GameData.LineSegment();
                newRoom.Start = new GameData.Vertex()
                {
                    X = roomLine.Start.X + (float) offsetPosition.X,
                    Y = roomLine.Start.Y + (float) offsetPosition.Y - 8,
                    Z = roomLine.Start.Z
                };
                newRoom.End = new GameData.Vertex()
                {
                    X = roomLine.End.X + (float) offsetPosition.X,
                    Y = roomLine.End.Y + (float) offsetPosition.Y - 8,
                    Z = roomLine.End.Z
                };
                newRoom.TextureId = roomLine.TextureId;

                line.X1 = newRoom.Start.X;
                line.Y1 = newRoom.Start.Y;
                line.X2 = newRoom.End.X;
                line.Y2 = newRoom.End.Y;

                _tempLineList.Add(line);
                _lineSegmentList.Add(new Tuple<GameData.LineSegment, Line>(newRoom, line));
                _canvas.Children.Add(line);
            }
        }

        private Point SnapToGrid(Point point, float currentScale)
        {
            Point newPoint = new Point();

            double scaledGridSize = _gridSize / currentScale;
            int remainder;
            Math.DivRem((int)Math.Round(point.X), (int)Math.Round(scaledGridSize), out remainder);

            if (remainder <= scaledGridSize / 2)
                newPoint.X = point.X - (float)remainder;
            else
                newPoint.X = point.X + (float)(scaledGridSize - remainder);

            Math.DivRem((int)Math.Round(point.Y), (int)Math.Round(scaledGridSize), out remainder);

            if (remainder <= scaledGridSize / 2)
                newPoint.Y = point.Y - (float)remainder;
            else
                newPoint.Y = point.Y + (float)(scaledGridSize - remainder);

            return newPoint;

        }

        public void LoadLineSegments(string fileName)
        {
            _mapData = new MapData();

            var mapSaver = new MapSaver(_mapData);
            GameData.LineSegment.Count = 0;

            mapSaver.LoadData(System.IO.Path.Combine(ConfigurationManager.AppSettings["BaseSavePath"], fileName));

            _roomLines = _mapData.LineSegments.Values.Select(x => x).ToList();
        }

        public List<Line> GetPreviewLines(string fileName)
        {
            return LoadLines(fileName);
        }

        public List<Line> GetEditorLines(string fileName)
        {
            return LoadLines(fileName, false);
        }

        
        public List<Line> LoadLines(string fileName, bool applyInternalTransform = true, Transform transform = null)
        {
            var lineList = new List<Line>();

            _mapData = new MapData();

            var mapSaver = new MapSaver(_mapData);
            GameData.LineSegment.Count = 0;

            mapSaver.LoadData(System.IO.Path.Combine(ConfigurationManager.AppSettings["BaseSavePath"], fileName));

            foreach (var lineSeg in _mapData.LineSegments.Values)
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

        public void CreateMesh(Shape shape, Point startPoint, float currentScale)
        {
            throw new NotImplementedException();
        }

        List<Tuple<Shape, FunAndGamesWithSharpDX.Entities.Polygon>> ISegmentEditor.GetMeshList()
        {
            throw new NotImplementedException();
        }
       
       
        public void EditAction(Point startPoint, float currentScale)
        {
            throw new NotImplementedException();
        }
    }
}
