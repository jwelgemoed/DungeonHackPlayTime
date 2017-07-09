using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Shapes;
using FunAndGamesWithSlimDX.Entities;
using System.Windows.Media;
using System.Windows.Controls;
using SlimDX.Direct3D11;
using FunAndGamesWithSlimDX.DirectX;
using SlimDX;
using GameData;
using DungeonHack.Builders;

namespace MapEditor
{
    public class ObstacleSegmentEditor : ISegmentEditor
    {
        private Canvas _canvas;
        private Device _device;
        private IShader _shader;
        private float _midWidth;
        private float _midHeight;
        private float _gridSize;
        private Brush _squareBrush;
        private Brush _selectedSquareBrush;
        private Rectangle _rectangle;
        private Rectangle _tempRectangle;
        private Rectangle _lastRectangle;
        private List<Rectangle> _rectangleList = new List<Rectangle>();

        private List<Tuple<Shape, FunAndGamesWithSlimDX.Entities.Polygon>> _meshList = new List<Tuple<Shape, FunAndGamesWithSlimDX.Entities.Polygon>>();

        public ObstacleSegmentEditor(Canvas canvase, Device device, IShader shader, float midWidth, float midHeight, float gridSize)
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

            //Mesh roomMesh = new Mesh(_device, _shader);
            float scaleFactor = currentScale;

            var scaleTransform = rect.LayoutTransform as ScaleTransform;
            var translateTransform = rect.RenderTransform as TranslateTransform;

            if (scaleTransform != null)
            {
                scaleFactor = (float)scaleTransform.ScaleX;
            }

            Model[] model = new Model[6];
            Vector3[] vectors = new Vector3[24];

            float left = (float)Canvas.GetLeft(rect);
            float top = (float)Canvas.GetTop(rect);
            float width = (float)rect.Width;
            float height = (float)rect.Height;
            float x1 = left;
            float x2 = left + width;
            float y1 = top;
            float y2 = top + height;

            //floor
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

            //ceiling
            vectors[4].X = ((x1 * scaleFactor) - _midWidth);
            vectors[4].Y = 64.0f;
            vectors[4].Z = _midHeight - (y1 * scaleFactor);

            vectors[5].X = ((x2 * scaleFactor) - _midWidth);
            vectors[5].Y = 64.0f;
            vectors[5].Z = _midHeight - (y1 * scaleFactor);

            vectors[6].X = ((x2 * scaleFactor) - _midWidth);
            vectors[6].Y = 64.0f;
            vectors[6].Z = _midHeight - (y2 * scaleFactor);

            vectors[7].X = ((x1 * scaleFactor) - _midWidth);
            vectors[7].Y = 64.0f;
            vectors[7].Z = _midHeight - (y2 * scaleFactor);

            // back wall
            vectors[8].X = ((x1 * scaleFactor) - _midWidth);
            vectors[8].Y = 64.0f;
            vectors[8].Z = _midHeight - (y1 * scaleFactor);

            vectors[9].X = ((x2 * scaleFactor) - _midWidth);
            vectors[9].Y = 64.0f;
            vectors[9].Z = _midHeight - (y1 * scaleFactor);

            vectors[10].X = ((x2 * scaleFactor) - _midWidth);
            vectors[10].Y = 0.0f;
            vectors[10].Z = _midHeight - (y1 * scaleFactor);

            vectors[11].X = ((x1 * scaleFactor) - _midWidth);
            vectors[11].Y = 0.0f;
            vectors[11].Z = _midHeight - (y1 * scaleFactor);

            // front wall
            vectors[12].X = ((x1 * scaleFactor) - _midWidth);
            vectors[12].Y = 64.0f;
            vectors[12].Z = _midHeight - (y2 * scaleFactor);
          
            vectors[13].X = ((x2 * scaleFactor) - _midWidth);
            vectors[13].Y = 64.0f;
            vectors[13].Z = _midHeight - (y2 * scaleFactor);
          
            vectors[14].X = ((x2 * scaleFactor) - _midWidth);
            vectors[14].Y = 0.0f;
            vectors[14].Z = _midHeight - (y2 * scaleFactor);
          
            vectors[15].X = ((x1 * scaleFactor) - _midWidth);
            vectors[15].Y = 0.0f;
            vectors[15].Z = _midHeight - (y2 * scaleFactor);

            //left wall
            vectors[16].X = ((x1 * scaleFactor) - _midWidth);
            vectors[16].Y = 64.0f;
            vectors[16].Z = _midHeight - (y1 * scaleFactor);

            vectors[17].X = ((x1 * scaleFactor) - _midWidth);
            vectors[17].Y = 64.0f;
            vectors[17].Z = _midHeight - (y2 * scaleFactor);

            vectors[18].X = ((x1 * scaleFactor) - _midWidth);
            vectors[18].Y = 0.0f;
            vectors[18].Z = _midHeight - (y1 * scaleFactor);

            vectors[19].X = ((x1 * scaleFactor) - _midWidth);
            vectors[19].Y = 0.0f;
            vectors[19].Z = _midHeight - (y2 * scaleFactor);

            //right wall
            vectors[20].X = ((x2 * scaleFactor) - _midWidth);
            vectors[20].Y = 64.0f;
            vectors[20].Z = _midHeight - (y2 * scaleFactor);

            vectors[21].X = ((x2 * scaleFactor) - _midWidth);
            vectors[21].Y = 64.0f;
            vectors[21].Z = _midHeight - (y1 * scaleFactor);

            vectors[22].X = ((x2 * scaleFactor) - _midWidth);
            vectors[22].Y = 0.0f;
            vectors[22].Z = _midHeight - (y2 * scaleFactor);

            vectors[23].X = ((x2 * scaleFactor) - _midWidth);
            vectors[23].Y = 0.0f;
            vectors[23].Z = _midHeight - (y1 * scaleFactor);

            //Indexes for the above square
            short[] faceIndexInverted = new short[36]
            {
                1, 0, 3, 1, 3, 2,
                4, 5, 6, 4, 6, 7,
                9, 8, 11, 9, 11, 10,
                12, 13, 14, 12, 14, 15,
                16, 17, 19, 16, 19, 18,
                20, 21, 23, 20, 23, 22
            };
            
            short[] faceIndex2 = new short[6];

            Vector3 normal;

            for (int i = 0; i < 6; i++)
            {
                normal = Vector3.Cross(vectors[(i * 4) + 1] - vectors[i * 4], vectors[(i * 4) + 2] - vectors[(i * 4) + 1]);
                normal = Vector3.Normalize(normal);

                model = new Model[6];

                for (int j = 0; j < 6; j++)
                {
                    faceIndex2[j] = (short)(faceIndexInverted[j + (i * 6)] % 6);
                    model[j].x = vectors[faceIndexInverted[j + (i * 6)]].X;
                    model[j].y = vectors[faceIndexInverted[j + (i * 6)]].Y;
                    model[j].z = vectors[faceIndexInverted[j + (i * 6)]].Z;
                    model[j].nx = normal.X;
                    model[j].ny = normal.Y;
                    model[j].nz = normal.Z;

                    switch (j)
                    {
                        case 1:
                            model[j].tx = 1.0f;
                            break;
                        case 2:
                        case 4:
                            model[j].tx = 1.0f;
                            model[j].ty = 1.0f;
                            break;
                        case 5:
                            model[j].ty = 1.0f;
                            break;
                        default:
                            break;
                    }
                }

                PolygonBuilder meshBuilder = new PolygonBuilder(_device, _shader);
                var roomMesh = meshBuilder
                                .New()
                                .SetModel(model)
                                .SetPosition((float)startPoint.X * scaleFactor, 0.0f, (float)startPoint.Y * scaleFactor)
                                .SetScaling(1, 1, 1)
                                .WithTransformToWorld()
                                .Build();

                _meshList.Add(new Tuple<Shape, FunAndGamesWithSlimDX.Entities.Polygon>(rect, roomMesh));
            }
        }

        public void EditAction(Point startPoint, float currentScale)
        {
            CreateMesh(_tempRectangle, startPoint, currentScale);
            
            _tempRectangle = null;
        }

        public List<Tuple<Shape, FunAndGamesWithSlimDX.Entities.Polygon>> GetMeshList()
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
            throw new NotImplementedException();
        }
    }
}
