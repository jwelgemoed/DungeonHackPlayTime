using FunAndGamesWithSharpDX.DirectX;
using FunAndGamesWithSharpDX.Entities;
using SharpDX;
using SharpDX.Direct3D11;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using GameData;
using DungeonHack.Builders;
using DungeonHack.DirectX;
using Rectangle = System.Windows.Shapes.Rectangle;
using Point = System.Windows.Point;
using Polygon = DungeonHack.Entities.Polygon;

namespace MapEditor
{
    public class FloorSegmentEditor : ISegmentEditor
    {
        private readonly Canvas _canvas;
        private readonly Device _device;
        private readonly Shader _shader;
        private readonly float _midWidth;
        private readonly float _midHeight;
        private readonly float _gridSize;
        private readonly Brush _squareBrush;
        private readonly Brush _selectedSquareBrush;
        private Rectangle _tempRectangle;
        private Rectangle _lastRectangle;
        private readonly List<Rectangle> _rectangleList = new List<Rectangle>();

        private readonly List<Tuple<Shape, Polygon>> _meshList = new List<Tuple<Shape, Polygon>>();

        public FloorSegmentEditor(Canvas canvase, Device device, Shader shader, float midWidth, float midHeight, float gridSize)
        {
            _squareBrush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(128, 0, 128));
            _selectedSquareBrush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(128, 128, 0));
            _canvas = canvase;
            _device = device;
            _shader = shader;
            _midHeight = midHeight;
            _midWidth = midWidth;
            _gridSize = gridSize;
        }

        public void CreateMesh(Shape shape, Point startPoint, float currentScale)
        {
            Rectangle rect = shape as Rectangle;

            if (rect == null)
                throw new ArgumentException("Parameter shape not a rectangle!");

            float scaleFactor = currentScale;

            var scaleTransform = rect.LayoutTransform as ScaleTransform;
            
            if (scaleTransform != null)
            {
                scaleFactor = (float)scaleTransform.ScaleX;
            }

            Model[] model = new Model[6];
            FunAndGamesWithSharpDX.Entities.Vertex[] vertexes = new FunAndGamesWithSharpDX.Entities.Vertex[4];

            float left = (float)Canvas.GetLeft(rect);
            float top = (float)Canvas.GetTop(rect);
            float width = (float)rect.Width;
            float height = (float)rect.Height;
            float x1 = left;
            float x2 = left + width;
            float y1 = top;
            float y2 = top + height;

            vertexes[0].Position.X = ((x1 * scaleFactor) - _midWidth);
            vertexes[0].Position.Y = 0.0f;
            vertexes[0].Position.Z = _midHeight - (y1 * scaleFactor);
            vertexes[0].Position.W = 1.0f;

            vertexes[1].Position.X = ((x2 * scaleFactor) - _midWidth);
            vertexes[1].Position.Y = 0.0f;
            vertexes[1].Position.Z = _midHeight - (y1 * scaleFactor);
            vertexes[1].Position.W = 1.0f;

            vertexes[2].Position.X = ((x2 * scaleFactor) - _midWidth);
            vertexes[2].Position.Y = 0.0f;
            vertexes[2].Position.Z = _midHeight - (y2 * scaleFactor);
            vertexes[2].Position.W = 1.0f;

            vertexes[3].Position.X = ((x1 * scaleFactor) - _midWidth);
            vertexes[3].Position.Y = 0.0f;
            vertexes[3].Position.Z = _midHeight - (y2 * scaleFactor);
            vertexes[3].Position.W = 1.0f;

            //Indexes for the above square
            short[] faceIndex = new short[6] {
                0, 1, 2, 0, 2, 3
            };

            Vector3 normal = Vector3.Cross(new Vector3(vertexes[0].Position.X, vertexes[0].Position.Y, vertexes[0].Position.Z)
                , new Vector3(vertexes[1].Position.X, vertexes[1].Position.Y, vertexes[1].Position.Z));

            normal = Vector3.Normalize(normal);

            FunAndGamesWithSharpDX.Entities.Vertex vertex = new FunAndGamesWithSharpDX.Entities.Vertex();

            model[0].x = vertexes[0].Position.X;
            model[0].y = vertexes[0].Position.Y;
            model[0].z = vertexes[0].Position.Z;
            model[0].nx = normal.X;
            model[0].ny = normal.Y;
            model[0].nz = normal.Z;
            model[0].tx = 0.0f;
            model[0].ty = 0.0f;

            model[1].x = vertexes[1].Position.X;
            model[1].y = vertexes[1].Position.Y;
            model[1].z = vertexes[1].Position.Z;
            model[1].nx = normal.X;
            model[1].ny = normal.Y;
            model[1].nz = normal.Z;
            model[1].tx = vertexes[1].Position.X / 64;//4.0f; length divided by texture width
            model[1].ty = 0.0f;

            model[2].x = vertexes[2].Position.X;
            model[2].y = vertexes[2].Position.Y;
            model[2].z = vertexes[2].Position.Z;
            model[2].nx = normal.X;
            model[2].ny = normal.Y;
            model[2].nz = normal.Z;
            model[2].tx = vertexes[2].Position.X / 64;//4.0f;
            model[2].ty = 1.0f;

            model[3].x = vertexes[0].Position.X;
            model[3].y = vertexes[0].Position.Y;
            model[3].z = vertexes[0].Position.Z;

            model[3].nx = normal.X;
            model[3].ny = normal.Y;
            model[3].nz = normal.Z;
            model[3].tx = 0.0f;
            model[3].ty = 0.0f;

            model[4].x = vertexes[2].Position.X;
            model[4].y = vertexes[2].Position.Y;
            model[4].z = vertexes[2].Position.Z;
            model[4].nx = normal.X;
            model[4].ny = normal.Y;
            model[4].nz = normal.Z;
            model[4].tx = vertexes[2].Position.X / 64;//4.0f;
            model[4].ty = 1.0f;

            model[5].x = vertexes[3].Position.X;
            model[5].y = vertexes[3].Position.Y;
            model[5].z = vertexes[3].Position.Z;
            model[5].nx = normal.X;
            model[5].ny = normal.Y;
            model[5].nz = normal.Z;
            model[5].tx = 0.0f;
            model[5].ty = 1.0f;

            PolygonBuilder meshBuilder = new PolygonBuilder(_device, _shader, new BufferFactory((_device)));
            var roomMesh = meshBuilder
                            .New()
                            .SetPosition((float)startPoint.X * scaleFactor, 0.0f, (float)startPoint.Y * scaleFactor)
                            .SetScaling(1, 1, 1)
                            .SetModel(model)
                            .WithTransformToWorld()
                            .Build();

            _meshList.Add(new Tuple<Shape, Polygon>(rect, roomMesh));

        }

        public void EditAction(Point startPoint, float currentScale)
        {
            CreateMesh(_tempRectangle, startPoint, currentScale);
            _tempRectangle = null;
        }

        public List<Tuple<Shape, Polygon>> GetMeshList()
        {
            return _meshList;
        }

        public void MoveAction(Point currentMousePosition, Point startPoint, float currentScale, bool snapToGrid, bool snapToClosestPoint)
        {
            var currentPos = currentMousePosition;

            if (_tempRectangle != null)
            {
                _canvas.Children.Remove(_tempRectangle);
                _rectangleList.Remove(_tempRectangle);
            }

            int absWidth = (int)Math.Abs(startPoint.X - 8 - currentPos.X);
            int absHeight = (int)Math.Abs(startPoint.Y - 8 - currentPos.Y);

            Rectangle rectangle = new Rectangle();
            rectangle.Stroke = SystemColors.WindowFrameBrush;

            rectangle.Width = absWidth;
            rectangle.Height = absHeight;

            _lastRectangle = _tempRectangle = rectangle;

            _canvas.Children.Add(rectangle);

            if ((currentPos.X - startPoint.X - 8) >= 0)
            {
                Canvas.SetLeft(rectangle, startPoint.X - 8);
            }
            else
            {
                Canvas.SetLeft(rectangle, startPoint.X - 8 - absWidth);
            }

            if ((currentPos.Y - startPoint.Y - 8) >= 0)
                Canvas.SetTop(rectangle, startPoint.Y - 8);
            else
                Canvas.SetTop(rectangle, startPoint.Y - 8 - absHeight);
        }

        public void CreateMesh(GameData.LineSegment lineSegment, Point startPoint, float currentScale)
        {
            throw new NotImplementedException();
        }

        public void EditAction(Point startPoint, float currentScale, GlobalMapData mapData)
        {
            CreateMesh(_tempRectangle, startPoint, currentScale);
            _tempRectangle = null;
        }
    }
}