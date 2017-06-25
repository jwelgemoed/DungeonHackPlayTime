using FunAndGamesWithSlimDX.DirectX;
using FunAndGamesWithSlimDX.Entities;
using SlimDX;
using SlimDX.Direct3D11;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.IO;
using System.Text;
using System.Runtime.Serialization.Formatters.Binary;
using GameData;
using DungeonHack.Builders;

namespace MapEditor
{
    public class RectangleSegment
    {
        private List<FunAndGamesWithSlimDX.Entities.Polygon> _meshList = new List<FunAndGamesWithSlimDX.Entities.Polygon>();
        private Device _device;
        private IShader _shader;

        public float CeilingHeight { get; set; }
        public float FloorHeight { get; set; }
        public float MidWidth { get; set; }
        public float MidHeight { get; set; }
        public float StartX { get; set; }
        public float StartY { get; set; }
        public float Scale { get; set; }
        public float TextureRepeat { get; set; }

        private float _x1,_y1,_x2,_y2;

        public RectangleSegment(Device device, IShader shader)
        {
            _device = device;
            _shader = shader;
            TextureRepeat = 4.0f;
        }

        public List<FunAndGamesWithSlimDX.Entities.Polygon> Meshes
        {
            get
            {
                return _meshList;
            }
        }

        public void BuildRoom(float x1, float y1, float x2, float y2, float scale)
        {
            _x1 = x1;
            _y1 = y1;
            _x2 = x2;
            _y2 = y2;
           
            BuildMeshes(x1, y1, x2, y2, scale);
        }

        private void BuildMeshes(float x1, float y1, float x2, float y2, float scaleFactor)
        {
            _meshList.RemoveAll(x => true);

            Model[] model = new Model[6];
            Vector3[] vectors = new Vector3[24];

            float midWidth = MidWidth;
            float midHeight = MidHeight;
            float startX = StartX;
            float startY = StartY;

            //floor
            vectors[0].X = ((x1 * scaleFactor) - midWidth);
            vectors[0].Y = FloorHeight;
            vectors[0].Z = midHeight - (y1 * scaleFactor);

            vectors[1].X = ((x2 * scaleFactor) - midWidth);
            vectors[1].Y = FloorHeight;
            vectors[1].Z = midHeight - (y1 * scaleFactor);

            vectors[2].X = ((x2 * scaleFactor) - midWidth);
            vectors[2].Y = FloorHeight;
            vectors[2].Z = midHeight - (y2 * scaleFactor);

            vectors[3].X = ((x1 * scaleFactor) - midWidth);
            vectors[3].Y = FloorHeight;
            vectors[3].Z = midHeight - (y2 * scaleFactor);

            //ceiling
            vectors[4].X = ((x1 * scaleFactor) - midWidth);
            vectors[4].Y = CeilingHeight;
            vectors[4].Z = midHeight - (y1 * scaleFactor);

            vectors[5].X = ((x2 * scaleFactor) - midWidth);
            vectors[5].Y = CeilingHeight;
            vectors[5].Z = midHeight - (y1 * scaleFactor);

            vectors[6].X = ((x2 * scaleFactor) - midWidth);
            vectors[6].Y = CeilingHeight;
            vectors[6].Z = midHeight - (y2 * scaleFactor);

            vectors[7].X = ((x1 * scaleFactor) - midWidth);
            vectors[7].Y = CeilingHeight;
            vectors[7].Z = midHeight - (y2 * scaleFactor);

            // front wall
            vectors[8].X = ((x1 * scaleFactor) - midWidth);
            vectors[8].Y = CeilingHeight;
            vectors[8].Z = midHeight - (y1 * scaleFactor);

            vectors[9].X = ((x2 * scaleFactor) - midWidth);
            vectors[9].Y = CeilingHeight;
            vectors[9].Z = midHeight - (y1 * scaleFactor);

            vectors[10].X = ((x2 * scaleFactor) - midWidth);
            vectors[10].Y = FloorHeight;
            vectors[10].Z = midHeight - (y1 * scaleFactor);

            vectors[11].X = ((x1 * scaleFactor) - midWidth);
            vectors[11].Y = FloorHeight;
            vectors[11].Z = midHeight - (y1 * scaleFactor);

            // back wall
            vectors[12].X = ((x2 * scaleFactor) - midWidth);
            vectors[12].Y = CeilingHeight;
            vectors[12].Z = midHeight - (y2 * scaleFactor);

            vectors[13].X = ((x1 * scaleFactor) - midWidth);
            vectors[13].Y = CeilingHeight;
            vectors[13].Z = midHeight - (y2 * scaleFactor);

            vectors[14].X = ((x1 * scaleFactor) - midWidth);
            vectors[14].Y = FloorHeight;
            vectors[14].Z = midHeight - (y2 * scaleFactor);

            vectors[15].X = ((x2 * scaleFactor) - midWidth);
            vectors[15].Y = FloorHeight;
            vectors[15].Z = midHeight - (y2 * scaleFactor);

            //left wall
            vectors[16].X = ((x1 * scaleFactor) - midWidth);
            vectors[16].Y = CeilingHeight;
            vectors[16].Z = midHeight - (y1 * scaleFactor);

            vectors[17].X = ((x1 * scaleFactor) - midWidth);
            vectors[17].Y = CeilingHeight;
            vectors[17].Z = midHeight - (y2 * scaleFactor);

            vectors[18].X = ((x1 * scaleFactor) - midWidth);
            vectors[18].Y = FloorHeight;
            vectors[18].Z = midHeight - (y1 * scaleFactor);

            vectors[19].X = ((x1 * scaleFactor) - midWidth);
            vectors[19].Y = FloorHeight;
            vectors[19].Z = midHeight - (y2 * scaleFactor);

            //right wall
            vectors[20].X = ((x2 * scaleFactor) - midWidth);
            vectors[20].Y = CeilingHeight;
            vectors[20].Z = midHeight - (y2 * scaleFactor);

            vectors[21].X = ((x2 * scaleFactor) - midWidth);
            vectors[21].Y = CeilingHeight;
            vectors[21].Z = midHeight - (y1 * scaleFactor);

            vectors[22].X = ((x2 * scaleFactor) - midWidth);
            vectors[22].Y = FloorHeight;
            vectors[22].Z = midHeight - (y2 * scaleFactor);

            vectors[23].X = ((x2 * scaleFactor) - midWidth);
            vectors[23].Y = FloorHeight;
            vectors[23].Z = midHeight - (y1 * scaleFactor);


            //Indexes for the above square
            short[] faceIndex = new short[36] {
                0, 1, 2, 0, 2, 3, //floor
                6, 5, 4, 6, 4, 7, //ceiling
                8, 9, 10, 8, 10, 11, //left wall
                12, 13, 14, 12, 14, 15, //right wall
                17, 16, 18, 17, 18, 19,
                21, 20, 22, 21, 22, 23
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
                    faceIndex2[j] = (short)(faceIndex[j + (i * 6)] % 6);
                    model[j].x = vectors[faceIndex[j + (i * 6)]].X;
                    model[j].y = vectors[faceIndex[j + (i * 6)]].Y;
                    model[j].z = vectors[faceIndex[j + (i * 6)]].Z;
                    model[j].nx = normal.X;
                    model[j].ny = normal.Y;
                    model[j].nz = normal.Z;

                    switch (j)
                    {
                        case 1:
                            model[j].tx = TextureRepeat;
                            break;
                        case 2:
                        case 4:
                            model[j].tx = TextureRepeat;
                            if (j == 0 || j == 1)
                                model[j].ty = TextureRepeat;
                            else
                                model[j].ty = 1.0f;
                            break;
                        case 5:
                            if (j == 0 || j == 1)
                                model[j].ty = TextureRepeat;
                            else
                                model[j].ty = 1.0f;
                            break;
                        default:
                            break;
                    }
                }

                MeshBuilder meshBuilder = new MeshBuilder(_device, _shader);

                var roomMesh = meshBuilder
                                .New()
                                .SetModel(model)
                                .SetPosition(startX * scaleFactor, 0.0f, startY * scaleFactor)
                                .SetScaling(1, 1, 1)
                                .WithTransformToWorld()
                                .Build();

                _meshList.Add(roomMesh);
            }
        }

        /// <summary>
        /// Method for cutting a given mesh. Given two cutting points the mesh segment between these two points are removed and the original mesh is replaced
        /// by two new meshes.
        /// Eg.
        /// |---------------------------------------|     = Original Mesh
        /// |--------------x--------------x---------|     = Mesh with cutting points
        /// |--------------|              |---------|     = Cut mesh - original mesh has been replaced by 2 new meshes
        /// </summary>
        public void CutMesh(FunAndGamesWithSlimDX.Entities.Polygon mesh, float x1, float y1, float x2, float y2)
        {
            if (mesh == null)
                throw new ArgumentNullException("mesh");

            //are the points located on the given mesh? if not then we cannot cut it!
            //mesh.Model
            
            //our model is a rectangle, therefore there are 4 possible lines on which the cutting points can lie, we have to find that line.
            
            //Given points P1 and P2 on a line, then another point P3 is also on the line if the following is true:
            //det|(x-x1)  (y-y1) | = 0
            //   |(x2-x1) (y2-y1)|   


        }

        public void UpdateMeshes()
        {
            BuildMeshes(_x1, _y1, _x2, _y2, Scale);
        }

        public void Save(Stream stream)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Serialize(stream, this);

            formatter = null;
        }

        
    }

    public class RectangleSegmentEditor : ISegmentEditor
    {
        private Canvas _canvas;
        private Device _device;
        private IShader _shader;
        private float _midWidth;
        private float _midHeight;
        private float _gridSize;
        private Brush _squareBrush;
        private Brush _selectedSquareBrush;
        private Rectangle _tempRectangle;
        private Rectangle _lastRectangle;
        private Rectangle _currentSelectedRectangle;
        private List<Rectangle> _rectangleList = new List<Rectangle>();

        private List<Tuple<Shape, RectangleSegment>> _roomList = new List<Tuple<Shape, RectangleSegment>>();

        public float CeilingHeight { get; set;}

        public float FloorHeight { get; set; }

        public List<Tuple<Shape, RectangleSegment>> RoomList { get { return _roomList; } }

        public event MouseButtonEventHandler RectangleClicked; 

        public RectangleSegmentEditor(Canvas canvase, Device device, IShader shader, float midWidth, float midHeight, float gridSize)
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

        public RectangleSegment SelectRoomSegment(Rectangle selectedRectangle)
        {
            var selection = _roomList.Find(x => x.Item1 == selectedRectangle);

            if (selection == null)
                return null;

            if (_currentSelectedRectangle != null)
            {
                _currentSelectedRectangle.Stroke = SystemColors.WindowFrameBrush;
            }

            _currentSelectedRectangle = selectedRectangle;
            selectedRectangle.Stroke = _selectedSquareBrush;

            return selection.Item2;
        }

        public void CreateMesh(Shape shape, Point startPoint, float currentScale)
        {
            Rectangle rect = shape as Rectangle;

            if (rect == null)
                throw new ArgumentException("Parameter shape not a rectangle!");

            //Make this rectangle clickable - now we can edit the heights of the room, etc.
            if (RectangleClicked != null)
            {
                rect.MouseDown += RectangleClicked;
            }

            //Mesh roomMesh = new Mesh(_device, _shader);
            float scaleFactor = currentScale;

            var scaleTransform = rect.LayoutTransform as ScaleTransform;

            if (scaleTransform != null)
            {
                scaleFactor = (float)scaleTransform.ScaleX;
            }

            float left = (float)Canvas.GetLeft(rect);
            float top = (float)Canvas.GetTop(rect);
            float width = (float)rect.Width;
            float height = (float)rect.Height;
            float x1 = left;
            float x2 = left + width;
            float y1 = top;
            float y2 = top + height;

            RectangleSegment roomSegment = new RectangleSegment(_device, _shader);

            roomSegment.MidHeight = _midHeight;
            roomSegment.MidWidth = _midWidth;
            roomSegment.CeilingHeight = 64.0f;
            roomSegment.FloorHeight = 0.0f;
            roomSegment.StartX = (float) startPoint.X;
            roomSegment.StartY = (float)startPoint.Y;
            roomSegment.Scale = scaleFactor;
            roomSegment.BuildRoom(x1, y1, x2, y2, scaleFactor);

            _roomList.Add(new Tuple<Shape, RectangleSegment>(rect, roomSegment));
        }

        public void EditAction(Point startPoint, float currentScale)
        {
            CreateMesh(_tempRectangle, startPoint, currentScale);
            
            _tempRectangle = null;
        }

        public List<Tuple<Shape, FunAndGamesWithSlimDX.Entities.Polygon>> GetMeshList()
        {
            //TODO: Change this
            List<Tuple<Shape, FunAndGamesWithSlimDX.Entities.Polygon>> meshList = new List<Tuple<Shape, FunAndGamesWithSlimDX.Entities.Polygon>>();

            foreach (var room in _roomList)
            {
                //Roogsegment list is projected back onto old meshlist for backwards compatibility.
                meshList.AddRange(room.Item2.Meshes.Select(x => new Tuple<Shape, FunAndGamesWithSlimDX.Entities.Polygon>(room.Item1, x)));
            }

            return meshList;
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

        public void SaveData(FileStream stream)
        {
            SaveData(stream);
        }

               
        public void ReadData(FileStream stream)
        {
            ReadData(stream);
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
