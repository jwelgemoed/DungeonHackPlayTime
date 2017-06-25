using DungeonHack.BSP;
using DungeonHack.BSP.LeafBsp;
using DungeonHack.Builders;
using FunAndGamesWithSlimDX;
using FunAndGamesWithSlimDX.Entities;
using GameData;
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
using System.Threading.Tasks;
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
        private List<Line> gridLines = new List<Line>();
        private MapDemoRunner demo;
        private int _gridSize = 8;
        private bool _snapToGrid = true;
        private bool _snapToClosestPoint = true;
        private SolidColorBrush _gridBrush;
        private SolidColorBrush _areaBrush;
        private SolidColorBrush _selectedShapeBrush;
        private float _scaleRate = 1.1f;
        private float _currentScale = 1.0f;
        private float _midWidth;
        private float _midHeight;
        private Point _currentMousePosition;
        private GridPlayer _playerStart;
        private bool _activeEdit = false;
        private WallSegmentEditor _wallSegmentEditor;
        private FloorSegmentEditor _floorSegmentEditor;
        private RectangleSegmentEditor _rectangleSegmentEditor;
        private ObstacleSegmentEditor _obstacleSegmentEditor;
        private ISegmentEditor _currentEditor;
        private RoomSegmentEditor _roomSegmentEditor;
        private RotateTransform _rotateTransform;
        private GlobalMapData _globalMapData;

        private ILog _logger = LogManager.GetLogger("application-logger");

        public List<Line> SelectedRoom { get; set; }

        private const float EPSILON = 0.0001f;

        public RectangleSegment SelectedRectangleSegment { get; set; }

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
                _midWidth = (float)960 / 2;
                _midHeight = (float)720 / 2;

                FocusManager.SetFocusedElement(canvasXZ, Keyboard.Focus(canvasXZ));

                _floorSegmentEditor = new FloorSegmentEditor(canvasXZ, demo.Device, demo.GetShader, _midWidth, _midHeight, _gridSize);
                _rectangleSegmentEditor = new RectangleSegmentEditor(canvasXZ, demo.Device, demo.GetShader, _midWidth, _midHeight, _gridSize);
                _rectangleSegmentEditor.RectangleClicked += rectangle_Clicked;
                _obstacleSegmentEditor = new ObstacleSegmentEditor(canvasXZ, demo.Device, demo.GetShader, _midWidth, _midHeight, _gridSize);
                _roomSegmentEditor = new RoomSegmentEditor(canvasXZ, _midWidth, _midHeight, _gridSize);
                _wallSegmentEditor = new WallSegmentEditor(canvasXZ, demo.Device, demo.GetShader, _midWidth, _midHeight, _gridSize);

                _currentEditor = _roomSegmentEditor;

                _globalMapData = new GlobalMapData();

                KeyDown += canvasXZ_PreviewKeyDown;
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
        }

        private void btnRunMap_Click(object sender, RoutedEventArgs e)
        {
            var meshList = new List<FunAndGamesWithSlimDX.Entities.Polygon>();

            foreach (var map in _globalMapData.GetMaps())
            {
                foreach (var sector in map.Sectors.Values)
                {
                    meshList.AddRange(CreateMeshes(map, sector, 1.0f));
                }
            }

            demo.Meshes = meshList;

            if (_playerStart != null)
                demo.SetStartingPosition(_playerStart.TranslateToRealSpace(1, (float) canvasXZ.Width / 2, (float) canvasXZ.Height / 2));

            BspTreeBuilder bspTreeBuilder = new BspTreeBuilder(demo.Device, demo.GetShader);
            BspBoundingVolumeCalculator bspBoudingVolumeCalculator = new BspBoundingVolumeCalculator();
            LeafBspTreeBuilder leafTreeBuilder = new LeafBspTreeBuilder(demo.Device, demo.GetShader);

            demo.InitializeScene();

            leafTreeBuilder.BuildTree(0, demo.Meshes);
            demo.BspRootNode = bspTreeBuilder.BuildTree(demo.Meshes);
            bspBoudingVolumeCalculator.ComputeBoundingVolumes(demo.BspRootNode);

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
                startPoint = e.GetPosition(this);

                if (!_editState)
                {
                    _editState = true;
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

        private IEnumerable<FunAndGamesWithSlimDX.Entities.Polygon> CreateMeshes(GameData.MapData mapdata, GameData.Sector sector, float currentScale)
        {
            List<FunAndGamesWithSlimDX.Entities.Polygon> meshList = new List<FunAndGamesWithSlimDX.Entities.Polygon>();

            var lineSegments = mapdata
                .LineSegments
                .Where(x => sector.SideDefinitions.Contains(x.Key))
                .OrderBy(x => x.Key)
                .Select(x => x.Value)
                .Distinct();

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

        private List<FunAndGamesWithSlimDX.Entities.Polygon> CreateMesh(List<GameData.Vertex> triangles, float floorHeight, float ceilingHeight, int floorTextureId, int ceilingTextureId, Vector2 lowerBound, Vector2 upperBound)
        {
            List<FunAndGamesWithSlimDX.Entities.Polygon> meshes = new List<FunAndGamesWithSlimDX.Entities.Polygon>();
            MeshBuilder meshBuilder = new MeshBuilder(demo.Device, demo.GetShader);

            int numberOfTriangles = triangles.Count / 3;

            for (int i = 0; i < numberOfTriangles; i++)
            {
                float scaleFactor = _currentScale;

                Model[] modelFloor = new Model[3];
                Vector3[] vectorsFloor = new Vector3[3];

                vectorsFloor[0].X = (triangles[i * 3].X * scaleFactor) - _midWidth;
                vectorsFloor[0].Y = floorHeight;
                vectorsFloor[0].Z = _midHeight - (triangles[i * 3].Y * scaleFactor);

                vectorsFloor[1].X = (triangles[(i * 3) + 1].X * scaleFactor) - _midWidth;
                vectorsFloor[1].Y = floorHeight;
                vectorsFloor[1].Z = _midHeight - (triangles[(i * 3) + 1].Y * scaleFactor);

                vectorsFloor[2].X = (triangles[(i * 3) + 2].X * scaleFactor) - _midWidth;
                vectorsFloor[2].Y = floorHeight;
                vectorsFloor[2].Z = _midHeight - (triangles[(i * 3) + 2].Y * scaleFactor);

                Vector2 boundingBoxSize = (upperBound - lowerBound)/16;

                Vector2 relativeP1 = new Vector2();
                relativeP1.X = vectorsFloor[0].X - lowerBound.X;
                relativeP1.Y = vectorsFloor[0].Z - lowerBound.Y;
                Vector2 relativeP2 = new Vector2();
                relativeP2.X = vectorsFloor[1].X - lowerBound.X;
                relativeP2.Y = vectorsFloor[1].Z - lowerBound.Y;
                Vector2 relativeP3 = new Vector2();
                relativeP3.X = vectorsFloor[2].X - lowerBound.X;
                relativeP3.Y = vectorsFloor[2].Z - lowerBound.Y;

                short[] faceIndex = new short[3] {
                    0, 1, 2
                };

                var image = new BitmapImage(new Uri(_globalMapData.GetMaps().First().TextureData[floorTextureId]));
                float imageWidth = image.PixelWidth / 16.0f; //grid size

                Vector3 normal = Vector3.Cross(vectorsFloor[2] - vectorsFloor[1], vectorsFloor[1] - vectorsFloor[0]);
                normal = Vector3.Normalize(normal);

                modelFloor[0].x = vectorsFloor[0].X;
                modelFloor[0].y = vectorsFloor[0].Y;
                modelFloor[0].z = vectorsFloor[0].Z;
                modelFloor[0].nx = normal.X;
                modelFloor[0].ny = normal.Y;
                modelFloor[0].nz = normal.Z;
                modelFloor[0].tx = relativeP1.X / boundingBoxSize.X;
                modelFloor[0].ty = relativeP1.Y / boundingBoxSize.Y;

                modelFloor[1].x = vectorsFloor[1].X;
                modelFloor[1].y = vectorsFloor[1].Y;
                modelFloor[1].z = vectorsFloor[1].Z;
                modelFloor[1].nx = normal.X;
                modelFloor[1].ny = normal.Y;
                modelFloor[1].nz = normal.Z;
                modelFloor[1].tx = relativeP2.X / boundingBoxSize.X;//vectors[1].X;//4.0f; length divided by texture width
                modelFloor[1].ty = relativeP2.Y / boundingBoxSize.Y;

                modelFloor[2].x = vectorsFloor[2].X;
                modelFloor[2].y = vectorsFloor[2].Y;
                modelFloor[2].z = vectorsFloor[2].Z;
                modelFloor[2].nx = normal.X;
                modelFloor[2].ny = normal.Y;
                modelFloor[2].nz = normal.Z;
                modelFloor[2].tx = relativeP3.X / boundingBoxSize.X;//vectors[2].X;//4.0f;
                modelFloor[2].ty = relativeP3.Y / boundingBoxSize.Y;

                var floorMesh = meshBuilder
                                  .New()
                                  .SetModel(modelFloor)
                                  .SetScaling(4, 1, 4)
                                  .SetTextureIndex(floorTextureId)
                                  .SetMaterialIndex(0)
                                  .WithTransformToWorld()
                                  .Build();

                meshes.Add(floorMesh);

                Model[] modelCeiling = new Model[3];
                Vector3[] vectorsCeiling = new Vector3[3];

                vectorsCeiling[0].X = (triangles[i * 3].X * scaleFactor) - _midWidth;
                vectorsCeiling[0].Y = ceilingHeight;
                vectorsCeiling[0].Z = _midHeight - (triangles[i * 3].Y * scaleFactor);

                vectorsCeiling[1].X = (triangles[(i * 3) + 1].X * scaleFactor) - _midWidth;
                vectorsCeiling[1].Y = ceilingHeight;
                vectorsCeiling[1].Z = _midHeight - (triangles[(i * 3) + 1].Y * scaleFactor);

                vectorsCeiling[2].X = (triangles[(i * 3) + 2].X * scaleFactor) - _midWidth;
                vectorsCeiling[2].Y = ceilingHeight;
                vectorsCeiling[2].Z = _midHeight - (triangles[(i * 3) + 2].Y * scaleFactor);

                faceIndex = new short[3] {
                    2, 1, 0
                };

                normal = Vector3.Cross(vectorsCeiling[2] - vectorsCeiling[1], vectorsCeiling[1] - vectorsCeiling[0]);
                normal = Vector3.Normalize(normal);

                modelCeiling[0].x = vectorsCeiling[faceIndex[0]].X;
                modelCeiling[0].y = vectorsCeiling[faceIndex[0]].Y;
                modelCeiling[0].z = vectorsCeiling[faceIndex[0]].Z;
                modelCeiling[0].nx = normal.X;
                modelCeiling[0].ny = normal.Y;
                modelCeiling[0].nz = normal.Z;
                modelCeiling[0].tx = relativeP3.X / boundingBoxSize.X;
                modelCeiling[0].ty = relativeP3.Y / boundingBoxSize.Y;

                modelCeiling[1].x = vectorsCeiling[faceIndex[1]].X;
                modelCeiling[1].y = vectorsCeiling[faceIndex[1]].Y;
                modelCeiling[1].z = vectorsCeiling[faceIndex[1]].Z;
                modelCeiling[1].nx = normal.X;
                modelCeiling[1].ny = normal.Y;
                modelCeiling[1].nz = normal.Z;
                modelCeiling[1].tx = relativeP2.X / boundingBoxSize.X;//vectors[1].X;//4.0f; length divided by texture width
                modelCeiling[1].ty = relativeP2.Y / boundingBoxSize.Y;

                modelCeiling[2].x = vectorsCeiling[faceIndex[2]].X;
                modelCeiling[2].y = vectorsCeiling[faceIndex[2]].Y;
                modelCeiling[2].z = vectorsCeiling[faceIndex[2]].Z;
                modelCeiling[2].nx = normal.X;
                modelCeiling[2].ny = normal.Y;
                modelCeiling[2].nz = normal.Z;
                modelCeiling[2].tx = relativeP1.X / boundingBoxSize.X;//vectors[2].X;//4.0f;
                modelCeiling[2].ty = relativeP1.Y / boundingBoxSize.Y;

                var ceilingMesh = meshBuilder
                                    .New()
                                    .SetModel(modelCeiling)
                                    .SetScaling(4, 1, 4)
                                    .SetTextureIndex(ceilingTextureId)
                                    .SetMaterialIndex(0)
                                    .WithTransformToWorld()
                                    .Build();

                meshes.Add(ceilingMesh);
            }

            return meshes;
        }

        private FunAndGamesWithSlimDX.Entities.Polygon CreateMesh(GameData.LineSegment lineSegment, float currentScale)
        {
            if (lineSegment == null)
                throw new ArgumentException(nameof(GameData.LineSegment));

          //  if (!lineSegment.IsSolid)
           //     return null;

            float scaleFactor = currentScale;

            var start = lineSegment.Start;
            var end = lineSegment.End;

            //roomMesh.SetPosition((float)startPoint.X * scaleFactor, 0.0f, (float)startPoint.Y * scaleFactor);

            Model[] model = new Model[6];
            Vector3[] vectors = new Vector3[4];

            vectors[0].X = ((float)(start.X * scaleFactor) - _midWidth);
            vectors[0].Y = 64.0f;
            vectors[0].Z = _midHeight - (float)(start.Y * scaleFactor);

            //start.Y = _midheight - vectors[0].z / 

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

            var image = new BitmapImage(new Uri(_globalMapData.GetMaps().First().TextureData[lineSegment.TextureId]));
            float imageWidth = image.PixelWidth / 8.0f; //grid size

            if (Math.Abs(start.X - end.X) < EPSILON)
            {
                maxTX = (float)Math.Round(Math.Abs(end.Y - start.Y) / imageWidth);
            }
            else if (Math.Abs(start.Y - end.Y) < EPSILON)
            {
                maxTX = (float)Math.Round(Math.Abs(end.X - start.X) / imageWidth);
            }
            else //use the diagonal of the triangle.
            {
                float sideA = Math.Abs(end.X - start.X);
                float sideB = Math.Abs(end.Y - start.Y);

                maxTX = (float)Math.Round(Math.Sqrt((sideA * sideA) + (sideB * sideB)) / imageWidth);
            }

            if (maxTX < EPSILON)
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

            MeshBuilder meshBuilder = new MeshBuilder(demo.Device, demo.GetShader);

            var roomMesh = meshBuilder
                            .New()
                            .SetModel(model)
                            .SetTextureIndex(lineSegment.TextureId)
                            .SetMaterialIndex(0)
                            .SetScaling(4, 1, 4)
                            .WithTransformToWorld()
                            .Build();

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

        private void btnShowBoundaryVolumes_Click(object sender, RoutedEventArgs e)
        {
            //Task task = new Task(() =>
            {

                var meshList = new List<FunAndGamesWithSlimDX.Entities.Polygon>();

                foreach (var map in _globalMapData.GetMaps())
                {
                    foreach (var sector in map.Sectors.Values)
                    {
                        meshList.AddRange(CreateMeshes(map, sector, 1.0f));
                    }
                }

                demo.Meshes = meshList;

                BspTreeBuilder bspTreeBuilder = new BspTreeBuilder(demo.Device, demo.GetShader);
                BspBoundingVolumeCalculator bspBoudingVolumeCalculator = new BspBoundingVolumeCalculator();

                var bspRootNode = bspTreeBuilder.BuildTree(demo.Meshes);
                bspBoudingVolumeCalculator.ComputeBoundingVolumes(bspRootNode);
                              

                Rectangle lastRectangle = null;

                bspTreeBuilder.TraverseBspTreeAndPerformActionOnNode(bspRootNode, x =>
                {
                    if (!x.BoundingVolume.HasValue)
                    {
                        return;
                    }

                    if (lastRectangle != null)
                    {
                        canvasXZ.Children.Remove(lastRectangle);
                    }

                    SlimDX.Matrix invWorld;

                    var world = x.Splitter.WorldMatrix;

                    SlimDX.Matrix.Invert(ref world, out invWorld);

                    var boundingBox = new BoundingBox(
                        Vector3.TransformCoordinate(x.BoundingVolume.Value.Minimum, invWorld),
                        Vector3.TransformCoordinate(x.BoundingVolume.Value.Maximum, invWorld));

                    var rectangle = CreateAndAddRectangle(boundingBox);

                    //lastRectangle = rectangle;
                    //System.Threading.Thread.Sleep(1000);
                });
            };

           // task.Start();
        }

        private Rectangle CreateAndAddRectangle(BoundingBox box)
        {
            Rectangle rectangle = new Rectangle();
            rectangle.Height = (_midHeight - Math.Abs(box.Maximum.Z - box.Minimum.Z)) / _currentScale;
            rectangle.Width = (Math.Abs(box.Maximum.X - box.Minimum.X) + _midWidth) / _currentScale;
            rectangle.Stroke = new SolidColorBrush(Color.FromRgb(128, 0, 0));
            canvasXZ.Children.Add(rectangle);
            Canvas.SetLeft(rectangle, _midWidth);
            Canvas.SetRight(rectangle, _midHeight);
            
            return rectangle;
        }
    }
}