using FunAndGamesWithSlimDX.DirectX;
using FunAndGamesWithSlimDX.Entities;
using SlimDX;
using SlimDX.Direct3D11;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using GameData;

namespace MapEditor
{
    public class FloorSegmentEditor : ISegmentEditor
    {
        private readonly Canvas _canvas;
        private readonly Device _device;
        private readonly IShader _shader;
        private readonly float _midWidth;
        private readonly float _midHeight;
        private readonly float _gridSize;
        private readonly Brush _squareBrush;
        private readonly Brush _selectedSquareBrush;
        private Rectangle _tempRectangle;
        private Rectangle _lastRectangle;
        private readonly List<Rectangle> _rectangleList = new List<Rectangle>();

        private readonly List<Tuple<Shape, Mesh>> _meshList = new List<Tuple<Shape, Mesh>>();

        public FloorSegmentEditor(Canvas canvase, Device device, IShader shader, float midWidth, float midHeight, float gridSize)
        {
            _squareBrush = new SolidColorBrush(Color.FromRgb(128, 0, 128));
            _selectedSquareBrush = new SolidColorBrush(Color.FromRgb(128, 128, 0));
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

            Mesh roomMesh = new Mesh(_device, _shader);
            float scaleFactor = currentScale;

            var scaleTransform = rect.LayoutTransform as ScaleTransform;
            
            if (scaleTransform != null)
            {
                scaleFactor = (float)scaleTransform.ScaleX;
            }

            roomMesh.SetPosition((float)startPoint.X * scaleFactor, 0.0f, (float)startPoint.Y * scaleFactor);

            Model[] model = new Model[6];
            Vector3[] vectors = new Vector3[4];

            float left = (float)Canvas.GetLeft(rect);
            float top = (float)Canvas.GetTop(rect);
            float width = (float)rect.Width;
            float height = (float)rect.Height;
            float x1 = left;
            float x2 = left + width;
            float y1 = top;
            float y2 = top + height;

            vectors[0].X = ((x1 * scaleFactor) - _midWidth);
            vectors[0].Y = 0.0f;
            vectors[0].Z = _midHeight - (y1 * scaleFactor);

            vectors[1].X = ((x2 * scaleFactor) - _midWidth);
            vectors[1].Y = 0.0f;
            vectors[1].Z = _midHeight - (y1 * scaleFactor);

            vectors[2].X = ((x2 * scaleFactor) - _midWidth);
            vectors[2].Y = 0.0f;
            vectors[2].Z = _midHeight - (y2 * scaleFactor);

            vectors[3].X = ((x1 * scaleFactor) - _midWidth);
            vectors[3].Y = 0.0f;
            vectors[3].Z = _midHeight - (y2 * scaleFactor);

            //Indexes for the above square
            short[] faceIndex = new short[6] {
                0, 1, 2, 0, 2, 3
            };

            Vector3 normal = Vector3.Cross(vectors[0], vectors[1]);
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
            _meshList.Add(new Tuple<Shape, Mesh>(rect, roomMesh));

        }

        public void EditAction(Point startPoint, float currentScale)
        {
            CreateMesh(_tempRectangle, startPoint, currentScale);
            _tempRectangle = null;
        }

        public List<Tuple<Shape, Mesh>> GetMeshList()
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

        public void EditAction(Point startPoint, float currentScale, MapData mapData)
        {
            throw new NotImplementedException();
        }
    }
}