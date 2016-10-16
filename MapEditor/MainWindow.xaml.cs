using DungeonHack.BSP;
using FunAndGamesWithSlimDX;
using FunAndGamesWithSlimDX.Entities;
using Geometry;
using log4net;
using Poly2Tri;
using SlimDX;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MapEditor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Point startPoint;
        private bool _editState = false;
        private List<Mesh> areaList = new List<Mesh>();
        private List<Line> gridLines = new List<Line>();
        private List<System.Windows.Shapes.Polygon> areasList = new List<System.Windows.Shapes.Polygon>();
        private MapDemoRunner demo;
        private int _gridSize = 8;
        private bool _snapToGrid = true;
        private bool _snapToClosestPoint = true;
        private SolidColorBrush _gridBrush;
        private SolidColorBrush _areaBrush;
        private SolidColorBrush _selectedShapeBrush;
        private float _scaleRate = 1.1f;
        private float _currentScale = 1.0f;
        private float _translatedX = 0.0f;
        private float _translatedY = 0.0f;
        private float _midWidth;
        private float _midHeight;
        private Point _currentMousePosition;
        private GridPlayer _playerStart;
        private Mesh _selectedMesh;
        private bool _activeEdit = false;
        private WallSegmentEditor _wallSegmentEditor;
        private FloorSegmentEditor _floorSegmentEditor;
        private RectangleSegmentEditor _rectangleSegmentEditor;
        private ObstacleSegmentEditor _obstacleSegmentEditor;
        private ISegmentEditor _currentEditor;
        private RectangleSegment _selectedRectangleSegment;
        private RoomSegmentEditor _roomSegmentEditor;
        private RotateTransform _rotateTransform;
        private GameData.MapData _globalMapData;

        private ILog _logger = LogManager.GetLogger("application-logger");

        private Shape _selectedShape;

        public List<Line> SelectedRoom { get; set; }

        public RectangleSegment SelectedRectangleSegment {
            get
            {
                return _selectedRectangleSegment;
            }
            set
            {
                if (value != _selectedRectangleSegment)
                {
                    _selectedRectangleSegment = value;
                }
            }
        }

        public MainWindow()
        {
            try
            {
                InitializeComponent();
                LoadRoomList();

                demo = new MapDemoRunner();
                demo.Initialize();

                _gridBrush = new SolidColorBrush(Color.FromArgb(50, 128, 128, 0));
                _areaBrush = new SolidColorBrush(Color.FromArgb(50, 128, 0, 0));
                _selectedShapeBrush = new SolidColorBrush(Color.FromArgb(50, 0, 128, 0));
                _midWidth = (float)canvasXZ.ActualWidth / 2;
                _midHeight = (float)canvasXZ.ActualHeight / 2;

                FocusManager.SetFocusedElement(canvasXZ, Keyboard.Focus(canvasXZ));

                _floorSegmentEditor = new FloorSegmentEditor(canvasXZ, demo.Device, demo.GetShader, _midWidth, _midHeight, _gridSize);
                _rectangleSegmentEditor = new RectangleSegmentEditor(canvasXZ, demo.Device, demo.GetShader, _midWidth, _midHeight, _gridSize);
                _rectangleSegmentEditor.RectangleClicked += rectangle_Clicked;
                _obstacleSegmentEditor = new ObstacleSegmentEditor(canvasXZ, demo.Device, demo.GetShader, _midWidth, _midHeight, _gridSize);
                _roomSegmentEditor = new RoomSegmentEditor(canvasXZ, _midWidth, _midHeight, _gridSize);
                _wallSegmentEditor = new WallSegmentEditor(canvasXZ, demo.Device, demo.GetShader, _midWidth, _midHeight, _gridSize);

                _currentEditor = _roomSegmentEditor;

                _globalMapData = new GameData.MapData();

                this.KeyDown += canvasXZ_PreviewKeyDown;
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
        }

        private void rectangle_Clicked(object sender, MouseButtonEventArgs e)
        {
            Rectangle clickedRectangle = sender as Rectangle;

            if (clickedRectangle == null)
                return;

            SelectedRectangleSegment = _rectangleSegmentEditor.SelectRoomSegment(clickedRectangle);
            FillTextValuesFromSelectedRoomSegment();
        }

        private void UpdateSelectedRoomSegment()
        {
            float value;

            if (float.TryParse(txtCeilingHeight.Text, out value))
                SelectedRectangleSegment.CeilingHeight = value;

            if (float.TryParse(txtFloorHeight.Text, out value))
                SelectedRectangleSegment.FloorHeight = value;

            if (float.TryParse(txtTextureRepeat.Text, out value))
                SelectedRectangleSegment.TextureRepeat = value;
        }

        private void FillTextValuesFromSelectedRoomSegment()
        {
            txtCeilingHeight.Text = SelectedRectangleSegment.CeilingHeight.ToString();
            txtFloorHeight.Text = SelectedRectangleSegment.FloorHeight.ToString();
            txtTextureRepeat.Text = SelectedRectangleSegment.TextureRepeat.ToString();
        }

        private void Canvas_Loaded(object sender, RoutedEventArgs e)
        {
            FocusManager.SetFocusedElement(canvasXZ, Keyboard.Focus(canvasXZ));
            DrawGrid(_gridSize, canvasXZ);
        }

        private void Canvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
        }

        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            _currentMousePosition = e.GetPosition(this);

            if (_editState)
            {
                _currentEditor.MoveAction(_currentMousePosition, startPoint, _currentScale, _snapToGrid, _snapToClosestPoint);
            }
        }

        private void btnDemo_Click(object sender, RoutedEventArgs e)
        {
            TerrainDemo demo = new TerrainDemo();

            demo.Run();

            demo.Dispose();
        }

        private void btnRunMap_Click(object sender, RoutedEventArgs e)
        {
            var meshList = new List<Mesh>();

            foreach (var sector in _globalMapData.Sectors.Values)
            {
                meshList.AddRange(CreateMeshes(_globalMapData, sector, 1.0f));
            }

            demo.Meshes = meshList;

            if (_playerStart != null)
                demo.SetStartingPosition(_playerStart.TranslateToRealSpace(4, (float) canvasXZ.Width / 2, (float) canvasXZ.Height / 2));

            BspTreeBuilder bspTreeBuilder = new BspTreeBuilder(demo.Device, demo.GetShader);

            demo.InitializeScene();
            demo.BspRootNode = bspTreeBuilder.BuildTree(demo.Meshes);
            
            demo.Start();
            demo.Run();
        }

        private void DrawGrid(int gridSize, Canvas canvas)
        {
            //first clear old grid
            foreach (var line in gridLines)
            {
                canvas.Children.Remove(line);
            }

            gridLines.Clear();

            double numGridX = Math.Floor(canvas.Width * _currentScale / gridSize);
            double numGridY = Math.Floor(canvas.Height * _currentScale / gridSize);
            double currentPosX = 0;

            float scaleGridSize = gridSize / _currentScale;

            for (int i = 0; i < numGridX; i++)
            {
                Line gridLine = new Line();
                gridLine.X1 = currentPosX + scaleGridSize;
                gridLine.Y1 = 0;

                gridLine.X2 = currentPosX + scaleGridSize;
                gridLine.Y2 = canvas.Height;
                gridLine.Stroke = _gridBrush;
                canvas.Children.Add(gridLine);

                currentPosX += scaleGridSize;

                gridLines.Add(gridLine);
            }

            double currentPosY = 0;

            for (int j = 0; j < numGridY; j++)
            {
                Line gridLine = new Line();
                gridLine.X1 = 0;
                gridLine.Y1 = currentPosY + scaleGridSize;
                gridLine.X2 = canvas.Width;
                gridLine.Y2 = currentPosY + scaleGridSize;
                gridLine.Stroke = _gridBrush;
                canvas.Children.Add(gridLine);

                currentPosY += scaleGridSize;

                gridLines.Add(gridLine);
            }
        }

        private void canvasXZ_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed && _activeEdit)
            {
                if (!_editState)
                {
                    _editState = true;
                    startPoint = e.GetPosition(this);
                    startPoint.X *= _currentScale;
                    startPoint.Y *= _currentScale;
                }
                else
                {
                    _editState = false;
                    _wallSegmentEditor.EditAction(startPoint, _currentScale);
                }
            }
        }

        private void canvasXZ_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed && _activeEdit)
            {
                if (!_editState)
                {
                    _editState = true;
                    startPoint = e.GetPosition(this);
                }
                else
                {
                    _editState = false;
                    _currentEditor.EditAction(startPoint, _currentScale, _globalMapData);
                }
            }

            _activeEdit = false;
        }

        private void canvasXZ_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            foreach (UIElement element in canvasXZ.Children)
            {
                var shape = element as Shape;

                if ((shape.LayoutTransform as ScaleTransform) == null)
                {
                    shape.LayoutTransform = new ScaleTransform(1, 1, canvasXZ.Width / 2, canvasXZ.Height / 2);
                }

                var scaleTransform = shape.LayoutTransform as ScaleTransform;

                if (e.Delta > 0)
                {
                    scaleTransform.ScaleX *= _scaleRate;
                    scaleTransform.ScaleY *= _scaleRate;
                }
                else
                {
                    scaleTransform.ScaleX /= _scaleRate;
                    scaleTransform.ScaleY /= _scaleRate;
                }
            }

            if (e.Delta > 0)
            {
                _currentScale /= _scaleRate;
            }
            else
            {
                _currentScale *= _scaleRate;
            }

            DrawGrid(_gridSize, canvasXZ);
        }

        private void canvasXZ_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (!e.IsDown)
                return;

            _activeEdit = false;

            if (e.Key == Key.S)
            {
                if (_playerStart == null)
                {
                    _playerStart = new GridPlayer(canvasXZ);
                }

                _playerStart.MovePlayer(_currentMousePosition);
            }
            else if (e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl)
            {
                _activeEdit = true;
            }
            else if (e.Key == Key.W)
            {
                _currentEditor = _wallSegmentEditor;
            }
            else if (e.Key == Key.F)
            {
                _currentEditor = _floorSegmentEditor;
            }
            else if (e.Key == Key.O)
            {
                _currentEditor = _obstacleSegmentEditor;
            }
            else if (e.Key == Key.R)
            {
                _currentEditor = _roomSegmentEditor;
            }
            else
            {
                foreach (UIElement element in canvasXZ.Children)
                {
                    var shape = element as Shape;

                    double offsetX = 0;
                    double offsetY = 0;

                    if ((shape.RenderTransform as TranslateTransform) == null)
                    {
                        shape.RenderTransform = new TranslateTransform(0, 0);
                    }

                    var translateTransform = shape.RenderTransform as TranslateTransform;

                    switch (e.Key)
                    {
                        case Key.Left:
                            offsetX = _scaleRate;
                            break;

                        case Key.Right:
                            offsetX = -_scaleRate;
                            break;

                        case Key.Up:
                            offsetY = _scaleRate;
                            break;

                        case Key.Down:
                            offsetY = -_scaleRate;
                            break;
                    }

                    translateTransform.X += offsetX;
                    translateTransform.Y += offsetY;
                }
            }

            //_translatedX -= offsetX;
            //_translatedY -= offsetY;

            DrawGrid(_gridSize, canvasXZ);
        }

        private void txtCeilingHeight_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateSelectedRoomSegment();
            SelectedRectangleSegment.UpdateMeshes();
        }

        private void txtFloorHeight_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateSelectedRoomSegment();
            SelectedRectangleSegment.UpdateMeshes();
        }

        private void txtTextureRepeat_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateSelectedRoomSegment();
            SelectedRectangleSegment.UpdateMeshes();
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            RoomEditWindow editorWindow = new RoomEditWindow();

            editorWindow.Closing += new CancelEventHandler((x, y) => {
                LoadRoomList();
            });

            editorWindow.Show();
        }

        private void LoadRoomList()
        {
            lstRooms.Items.Clear();

            var inputDir = ConfigurationManager.AppSettings["BaseSavePath"];

            foreach (var file in Directory.EnumerateFiles(inputDir))
            {
                lstRooms.Items.Add(file.Substring(file.LastIndexOf(@"\")+1));
            }
        }

        private void lstRooms_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedFile = (sender as ListBox)?.SelectedItem as string;

            if (!string.IsNullOrEmpty(selectedFile))
            {
                List<Line> lines = _roomSegmentEditor.GetPreviewLines(selectedFile);
                canvasPreview.Children.Clear();

                ScaleTransform scaleTransform = new ScaleTransform(0.5, 0.5);

                foreach (var line in lines)
                {
                    line.LayoutTransform = scaleTransform;
                    
                    canvasPreview.Children.Add(line);
                }

                _roomSegmentEditor.LoadLineSegments(selectedFile);

                _currentEditor = _roomSegmentEditor;
            }
        }

        private void btnRotateLeft_Click(object sender, RoutedEventArgs e)
        {
            RotateWallSegmentEditor(-90);
        }

        private void btnRotateRight_Click(object sender, RoutedEventArgs e)
        {
            RotateWallSegmentEditor(90);
        }

        private void RotateWallSegmentEditor(float angle)
        {
            if (_rotateTransform == null)
            {
                _rotateTransform = new RotateTransform();
            }

            _rotateTransform.Angle = _rotateTransform.Angle + angle;
            _rotateTransform.CenterX = 120.0;
            _rotateTransform.CenterY = 120.0;

            var selectedFile = lstRooms.SelectedItem as string;

            if (!string.IsNullOrEmpty(selectedFile))
            {
                List<Line> lines = _roomSegmentEditor.LoadLines(selectedFile, true, _rotateTransform);
                canvasPreview.Children.Clear();

                ScaleTransform scaleTransform = new ScaleTransform(0.5, 0.5);

                foreach (var line in lines)
                {
                    line.LayoutTransform = scaleTransform;

                    canvasPreview.Children.Add(line);
                }

                //_roomSegmentEditor.SetLineList(lines);

                _currentEditor = _roomSegmentEditor;
            }
        }

        private IEnumerable<Mesh> CreateMeshes(GameData.MapData mapdata, GameData.Sector sector, float currentScale)
        {
            List<Mesh> meshList = new List<Mesh>();

            var lineSegments = mapdata
                .LineSegments
                .Where(x => sector.SideDefinitions.Contains(x.Key))
                .OrderBy(x => x.Key)
                .Select(x => x.Value);

            //Create walls
            foreach (var lineSegment in lineSegments)
            {
            //    if (!lineSegment.IsSolid)
            //        continue;

                meshList.Add(CreateMesh(lineSegment, currentScale));
            }

            //Create floor
            //First triangulate the polygon defined by the sector sidedefs.
            lineSegments = mapdata.LineSegments.Where(x => sector.SideDefinitions.Contains(x.Key)).Select(x => x.Value);
            var vertexes = new List<GameData.Vertex>();

            vertexes.AddRange(lineSegments.Select(x => x.End));

            List<PolygonPoint> polygonPoints = new List<PolygonPoint>();

            Poly2Tri.Polygon polygon = new Poly2Tri.Polygon(vertexes.Select(x => new PolygonPoint(x.X, x.Y)));

            P2T.Triangulate(polygon);

            var lowerBound = new Vector2((float)polygon.Points.Min(x => x.X), (float)polygon.Points.Min(y => y.Y));
            var upperBound = new Vector2((float)polygon.Points.Max(x => x.X), (float)polygon.Points.Max(y => y.Y));

            List<GameData.Vertex> triangles = new List<GameData.Vertex>();

            foreach (var triangle in polygon.Triangles)
            {
                triangles.Add(new GameData.Vertex
                {
                    X = (float)triangle.Points[0].X,
                    Y = (float)triangle.Points[0].Y
                });

                triangles.Add(new GameData.Vertex
                {
                    X = (float)triangle.Points[1].X,
                    Y = (float)triangle.Points[1].Y
                });

                triangles.Add(new GameData.Vertex
                {
                    X = (float)triangle.Points[2].X,
                    Y = (float)triangle.Points[2].Y
                });
            }

            meshList.AddRange(CreateMesh(triangles, 0, 64.0f, sector.FloorTextureId, sector.CeilingTextureId, lowerBound, upperBound));

            return meshList;
        }

        private List<Mesh> CreateMesh(List<GameData.Vertex> triangles, float floorHeight, float ceilingHeight, int floorTextureId, int ceilingTextureId, Vector2 lowerBound, Vector2 upperBound)
        {
            List<Mesh> meshes = new List<Mesh>();

            int numberOfTriangles = triangles.Count / 3;

            for (int i = 0; i < numberOfTriangles; i++)
            {
                Mesh floorMesh = new Mesh(demo.Device, demo.GetShader);
                float scaleFactor = _currentScale;

                Model[] model = new Model[3];
                Vector3[] vectors = new Vector3[3];

                vectors[0].X = (triangles[i * 3].X * scaleFactor) - _midWidth;
                vectors[0].Y = floorHeight;
                vectors[0].Z = _midHeight - (triangles[i * 3].Y * scaleFactor);

                vectors[1].X = (triangles[(i * 3) + 1].X * scaleFactor) - _midWidth;
                vectors[1].Y = floorHeight;
                vectors[1].Z = _midHeight - (triangles[(i * 3) + 1].Y * scaleFactor);

                vectors[2].X = (triangles[(i * 3) + 2].X * scaleFactor) - _midWidth;
                vectors[2].Y = floorHeight;
                vectors[2].Z = _midHeight - (triangles[(i * 3) + 2].Y * scaleFactor);

                Vector2 boundingBoxSize = (upperBound - lowerBound)/16;

                Vector2 relativeP1 = new Vector2();
                relativeP1.X = vectors[0].X - lowerBound.X;
                relativeP1.Y = vectors[0].Z - lowerBound.Y;
                Vector2 relativeP2 = new Vector2();
                relativeP2.X = vectors[1].X - lowerBound.X;
                relativeP2.Y = vectors[1].Z - lowerBound.Y;
                Vector2 relativeP3 = new Vector2();
                relativeP3.X = vectors[2].X - lowerBound.X;
                relativeP3.Y = vectors[2].Z - lowerBound.Y;

                short[] faceIndex = new short[3] {
                    0, 1, 2
                };

                var image = new BitmapImage(new Uri(_globalMapData.TextureData[floorTextureId]));
                float imageWidth = image.PixelWidth / 16.0f; //grid size

                Vector3 normal = Vector3.Cross(vectors[1] - vectors[0], vectors[2] - vectors[1]);
                normal = Vector3.Normalize(normal);

                model[0].x = vectors[0].X;
                model[0].y = vectors[0].Y;
                model[0].z = vectors[0].Z;
                model[0].nx = normal.X;
                model[0].ny = normal.Y;
                model[0].nz = normal.Z;
                model[0].tx = relativeP1.X / boundingBoxSize.X;
                model[0].ty = relativeP1.Y / boundingBoxSize.Y;

                model[1].x = vectors[1].X;
                model[1].y = vectors[1].Y;
                model[1].z = vectors[1].Z;
                model[1].nx = normal.X;
                model[1].ny = normal.Y;
                model[1].nz = normal.Z;
                model[1].tx = relativeP2.X / boundingBoxSize.X;//vectors[1].X;//4.0f; length divided by texture width
                model[1].ty = relativeP2.Y / boundingBoxSize.Y;

                model[2].x = vectors[2].X;
                model[2].y = vectors[2].Y;
                model[2].z = vectors[2].Z;
                model[2].nx = normal.X;
                model[2].ny = normal.Y;
                model[2].nz = normal.Z;
                model[2].tx = relativeP3.X / boundingBoxSize.X;//vectors[2].X;//4.0f;
                model[2].ty = relativeP3.Y / boundingBoxSize.Y;

                floorMesh.LoadVectorsFromModel(model, faceIndex);
                floorMesh.SetScaling(4, 1, 4);
                floorMesh.LoadTextureFullPath(_globalMapData.TextureData[floorTextureId]);

                meshes.Add(floorMesh);

                Mesh ceilingMesh = new Mesh(demo.Device, demo.GetShader);

                vectors[0].Y = ceilingHeight;

                vectors[1].Y = ceilingHeight;

                vectors[2].Y = ceilingHeight;

                faceIndex = new short[3] {
                    2, 1, 0
                };

                normal = Vector3.Cross(vectors[1] - vectors[0], vectors[2] - vectors[1]);
                normal = Vector3.Normalize(normal);

                model[0].x = vectors[faceIndex[0]].X;
                model[0].y = vectors[faceIndex[0]].Y;
                model[0].z = vectors[faceIndex[0]].Z;
                model[0].nx = normal.X;
                model[0].ny = normal.Y;
                model[0].nz = normal.Z;
                model[0].tx = relativeP3.X / boundingBoxSize.X;
                model[0].ty = relativeP3.Y / boundingBoxSize.Y;

                model[1].x = vectors[faceIndex[1]].X;
                model[1].y = vectors[faceIndex[1]].Y;
                model[1].z = vectors[faceIndex[1]].Z;
                model[1].nx = normal.X;
                model[1].ny = normal.Y;
                model[1].nz = normal.Z;
                model[1].tx = relativeP2.X / boundingBoxSize.X;//vectors[1].X;//4.0f; length divided by texture width
                model[1].ty = relativeP2.Y / boundingBoxSize.Y;

                model[2].x = vectors[faceIndex[2]].X;
                model[2].y = vectors[faceIndex[2]].Y;
                model[2].z = vectors[faceIndex[2]].Z;
                model[2].nx = normal.X;
                model[2].ny = normal.Y;
                model[2].nz = normal.Z;
                model[2].tx = relativeP1.X / boundingBoxSize.X;//vectors[2].X;//4.0f;
                model[2].ty = relativeP1.Y / boundingBoxSize.Y;

                ceilingMesh.LoadVectorsFromModel(model, faceIndex);
                ceilingMesh.SetScaling(4, 1, 4);
                ceilingMesh.LoadTextureFullPath(_globalMapData.TextureData[ceilingTextureId]);

                meshes.Add(ceilingMesh);
            }

            return meshes;
        }

        private Mesh CreateMesh(GameData.LineSegment lineSegment, float currentScale)
        {
            if (lineSegment == null)
                throw new ArgumentException(nameof(GameData.LineSegment));

          //  if (!lineSegment.IsSolid)
           //     return null;

            Mesh roomMesh = new Mesh(demo.Device, demo.GetShader);
            float scaleFactor = currentScale;

            var start = lineSegment.Start;
            var end = lineSegment.End;

            //roomMesh.SetPosition((float)startPoint.X * scaleFactor, 0.0f, (float)startPoint.Y * scaleFactor);

            Model[] model = new Model[6];
            Vector3[] vectors = new Vector3[4];

            vectors[0].X = ((float)(start.X * scaleFactor) - _midWidth);
            vectors[0].Y = 64.0f;
            vectors[0].Z = _midHeight - (float)(start.Y * scaleFactor);

            vectors[1].X = ((float)(end.X * scaleFactor) - _midWidth);
            vectors[1].Y = 64.0f;
            vectors[1].Z = _midHeight - (float)(end.Y * scaleFactor);

            vectors[2].X = ((float)(end.X * scaleFactor) - _midWidth);
            vectors[2].Y = 0.0f;
            vectors[2].Z = _midHeight - (float)(end.Y * scaleFactor);

            vectors[3].X = ((float)(start.X * scaleFactor) - _midWidth);
            vectors[3].Y = 0.0f;
            vectors[3].Z = _midHeight - (float)(start.Y * scaleFactor);

            float maxTX;

            var image = new BitmapImage(new Uri(_globalMapData.TextureData[lineSegment.TextureId]));
            float imageWidth = image.PixelWidth / 8.0f; //grid size

            if (start.X == end.X)
            {
                maxTX = (float)Math.Round(Math.Abs(end.Y - start.Y) / imageWidth);
            }
            else if (start.Y == end.Y)
            {
                maxTX = (float)Math.Round(Math.Abs(end.X - start.X) / imageWidth);
            }
            else //use the diagonal of the triangle.
            {
                float sideA = Math.Abs(end.X - start.X);
                float sideB = Math.Abs(end.Y - start.Y);

                maxTX = (float)Math.Round(Math.Sqrt((sideA * sideA) + (sideB * sideB)) / imageWidth);
            }

            if (maxTX == 0)
            {
                maxTX = 1.0f;
            }

            float maxTY = 1.0f;

            //Indexes for the above square
            short[] faceIndex = new short[6] {
                0, 1, 2, 0, 2, 3
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
            model[1].tx = maxTX;//vectors[1].X;//4.0f; length divided by texture width
            model[1].ty = 0.0f;

            model[2].x = vectors[2].X;
            model[2].y = vectors[2].Y;
            model[2].z = vectors[2].Z;
            model[2].nx = normal.X;
            model[2].ny = normal.Y;
            model[2].nz = normal.Z;
            model[2].tx = maxTX;//vectors[2].X;//4.0f;
            model[2].ty = maxTY;

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
            model[4].tx = maxTX; //vectors[2].X;//4.0f;
            model[4].ty = maxTY;

            model[5].x = vectors[3].X;
            model[5].y = vectors[3].Y;
            model[5].z = vectors[3].Z;
            model[5].nx = normal.X;
            model[5].ny = normal.Y;
            model[5].nz = normal.Z;
            model[5].tx = 0.0f;
            model[5].ty = maxTY;

            roomMesh.LoadVectorsFromModel(model, faceIndex);
            roomMesh.SetScaling(4, 1, 4);
            roomMesh.LoadTextureFullPath(_globalMapData.TextureData[lineSegment.TextureId]);

            return roomMesh;
        }

        private void btnCreateRoom_Click(object sender, RoutedEventArgs e)
        {
            RoomEditWindow editorWindow = new RoomEditWindow();

            editorWindow.Closing += new CancelEventHandler((x, y) => {
                LoadRoomList();
            });

            editorWindow.Show();
        }
    }
}