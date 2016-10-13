using FunAndGamesWithSlimDX.Entities;
using log4net;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using GameData;
using Geometry;
using Poly2Tri;

namespace MapEditor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class RoomEditWindow : Window
    {
        private Point startPoint;
        private bool _editState = false;
        private List<Mesh> areaList = new List<Mesh>();
        private List<Line> gridLines = new List<Line>();
        private List<System.Windows.Shapes.Polygon> areasList = new List<System.Windows.Shapes.Polygon>();
        private MapDemoRunner demo;
        private int _gridSize = 16;
        private bool _showGrid = true;
        private bool _snapToGrid = true;
        private bool _snapToClosestPoint = true;
        private SolidColorBrush _gridBrush;
        private SolidColorBrush _areaBrush;
        private SolidColorBrush _selectedShapeBrush;
        private float _currentScale = 1.0f;
        private float _midWidth;
        private float _midHeight;
        private Point _currentMousePosition;
        private bool _activeEdit = false;
        private WallSegmentEditor _wallSegmentEditor;
        private SectorEditor _sectorEditor;
        private GameData.MapData _mapData;
        private int _selectedCeilingTextureId;
        private int _selectedFloorTextureId;

        public GameData.LineSegment SelectedLineSegment { get; set; }

        public GameData.Sector SelectedSector { get; set; }

        private ILog _logger = LogManager.GetLogger("application-logger");

        public string SavePath { get; set; }

        public string ResourcePath { get; set; }

        public RoomEditWindow()
        {
            try
            {
                InitializeComponent();

                SavePath = ConfigurationManager.AppSettings["BaseSavePath"];
                ResourcePath = ConfigurationManager.AppSettings["ResourcePath"];

                demo = new MapDemoRunner();
                demo.Initialize();

                _gridBrush = new SolidColorBrush(Color.FromArgb(50, 128, 128, 0));
                _areaBrush = new SolidColorBrush(Color.FromArgb(50, 128, 0, 0));
                _selectedShapeBrush = new SolidColorBrush(Color.FromArgb(50, 0, 128, 0));
                _midWidth = (float)canvasXZ.ActualWidth / 2;
                _midHeight = (float)canvasXZ.ActualHeight / 2;

                FocusManager.SetFocusedElement(canvasXZ, Keyboard.Focus(canvasXZ));

                _mapData = new GameData.MapData();

                _wallSegmentEditor = new WallSegmentEditor(canvasXZ, demo.Device, demo.GetShader, _midWidth, _midHeight, _gridSize, _mapData);
                _wallSegmentEditor.WallSelectedAction = UpdateSelectedLineSegment;

                _sectorEditor = new MapEditor.SectorEditor(canvasXZ, demo.Device, demo.GetShader, _midWidth, _midHeight, _gridSize, _mapData);
                _sectorEditor.SectorSelectedAction = UpdateSelectedSector;
                _sectorEditor.SegmentSelectedAction = UpdateSelectedLineSegment;

                this.KeyDown += canvasXZ_PreviewKeyDown;

                LoadTextures();
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
        }

        private void UpdateSelectedSector(Sector obj)
        {
            SelectedSector = obj;
        }

        private void UpdateSelectedLineSegment(GameData.LineSegment lineSegment)
        {
            SelectedLineSegment = lineSegment;

            var texture = lstTextures.Items[SelectedLineSegment.TextureId] as string;

            UpdateLineSegmentTexture(System.IO.Path.Combine(ResourcePath, texture));

            lstTextures.IsEnabled = true;
            imgTexture.IsEnabled = true;
            imgTextureLineSeg.IsEnabled = true;
            chkSolidSegment.IsEnabled = true;
        } 

        private void LoadTextures()
        {
            foreach (var file in Directory.GetFileSystemEntries(ResourcePath, "*.png"))
            {
                lstTextures.Items.Add(file.Substring(file.LastIndexOf('\\')+1));
                _mapData.AddTextureData(file);
            }

            _sectorEditor.SelectedTexture = lstTextures.Items[0].ToString();
            _wallSegmentEditor.SelectedTexture = _sectorEditor.SelectedTexture;
        }

        private void Canvas_Loaded(object sender, RoutedEventArgs e)
        {
            FocusManager.SetFocusedElement(canvasXZ, Keyboard.Focus(canvasXZ));

            if (_showGrid)
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
                //_wallSegmentEditor.MoveAction(_currentMousePosition, startPoint, _currentScale, _snapToGrid, _snapToClosestPoint);
                _sectorEditor.MoveAction(_currentMousePosition, startPoint, _currentScale, _snapToGrid, _snapToClosestPoint);
            }
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

                    if (_sectorEditor.IsSectorComplete())
                    {
                        startPoint = e.GetPosition(this);
                    }
                    else
                    {
                        startPoint = _sectorEditor.GetLastPoint() ?? e.GetPosition(this);
                    }
                }
                else
                {
                    _editState = false;
                    //_wallSegmentEditor.EditAction(startPoint, _currentScale);
                    _sectorEditor.AddLineSegment(startPoint, _currentScale);
                }
            }

            _activeEdit = false;
        }

        private void canvasXZ_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (!e.IsDown)
                return;

            _activeEdit = false;

            if (e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl)
            {
                _activeEdit = true;
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            
            _wallSegmentEditor.SaveLines(System.IO.Path.Combine(SavePath, txtRoomName.Text.Trim()));
        }

        private void lstTextures_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var textureFileName = System.IO.Path.Combine(ResourcePath, lstTextures.SelectedValue as string);

            UpdatePreviewTexture(textureFileName);

            _wallSegmentEditor.SelectedTexture = textureFileName;
            _sectorEditor.SelectedTexture = textureFileName;
        }

        private void UpdatePreviewTexture(string texture)
        {
            var fileInfo = new FileInfo(texture);

            var image = new BitmapImage(new Uri(fileInfo.FullName));

            imgTexture.Source = image;
        }

        private void UpdateLineSegmentTexture(string texture)
        {
            var fileInfo = new FileInfo(texture);

            var image = new BitmapImage(new Uri(fileInfo.FullName));

            imgTextureLineSeg.Source = image;
        }

        private void UpdateFloorTexture(string texture)
        {
            var fileInfo = new FileInfo(texture);

            var image = new BitmapImage(new Uri(fileInfo.FullName));

            imgFloor.Source = image;
        }

        private void UpdateCeilingTexture(string texture)
        {
            var fileInfo = new FileInfo(texture);

            var image = new BitmapImage(new Uri(fileInfo.FullName));

            imgCeiling.Source = image;
        }

        private void lstTextures_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (SelectedLineSegment == null)
                return;

            SelectedLineSegment.TextureId = lstTextures.SelectedIndex;

            var textureFileName = System.IO.Path.Combine(ResourcePath, lstTextures.SelectedValue as string);

            UpdateLineSegmentTexture(textureFileName);
        }

       
        private void chkGrid_Click(object sender, RoutedEventArgs e)
        {
            _showGrid = (sender as CheckBox).IsChecked ?? false;

            if (_showGrid)
            {
                foreach (var line in gridLines)
                    canvasXZ.Children.Remove(line);

                DrawGrid(_gridSize, canvasXZ);
            }
            else
            {
                foreach (var line in gridLines)
                    canvasXZ.Children.Remove(line);
            }
        }

        private void chkSnapToGrid_Click(object sender, RoutedEventArgs e)
        {
            _snapToGrid = (sender as CheckBox).IsChecked ?? false;
        }

        private void chkSnapToClosestPoint_Click(object sender, RoutedEventArgs e)
        {
            _snapToClosestPoint = (sender as CheckBox).IsChecked ?? false;
        }

        private void btnLineSegment_Click(object sender, RoutedEventArgs e)
        {
            _sectorEditor.SelectSegment = true;
            _sectorEditor.SelectSector = false;
        }

        private void btnSector_Click(object sender, RoutedEventArgs e)
        {
            _sectorEditor.SelectSector = true;
            _sectorEditor.SelectSegment = false;
        }

        private void btnSetSegmentTexture_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedLineSegment == null)
            {
                MessageBox.Show("No Line Segment Selected!", "Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }

            var fileName = System.IO.Path.Combine(ResourcePath, lstTextures.SelectedValue as string);

            SelectedLineSegment.TextureId = _mapData.TextureData.First(x => x.Value == fileName).Key;

            _mapData.UpdateLineSegment(SelectedLineSegment);

            UpdateLineSegmentTexture(fileName);
        }

        private void btnSetFloorTexture_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedSector == null)
            {
                MessageBox.Show("No Sector Selected!", "Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }
                        
            var fileName = System.IO.Path.Combine(ResourcePath, lstTextures.SelectedValue as string);

            SelectedSector.FloorTextureId = _mapData.TextureData.First(x => x.Value == fileName).Key;

            UpdateFloorTexture(fileName);
        }

        private void btnSetCeilingTexture_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedSector == null)
            {
                MessageBox.Show("No Sector Selected!", "Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }

            var fileName = System.IO.Path.Combine(ResourcePath, lstTextures.SelectedValue as string);

            SelectedSector.CeilingTextureId = _mapData.TextureData.First(x => x.Value == fileName).Key;

            UpdateCeilingTexture(fileName);
        }

        private void btnTriangulate_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedSector == null)
            {
                MessageBox.Show("No Sector Selected!", "Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }

            var lineSegments = _mapData.LineSegments.Where(x => SelectedSector.SideDefinitions.Contains(x.Key)).Select(x => x.Value);
            var vertexes = new List<GameData.Vertex>();

            vertexes.AddRange(lineSegments.Select(x => x.End));

            List<PolygonPoint> polygonPoints = new List<PolygonPoint>();

            Poly2Tri.Polygon polygon = new Poly2Tri.Polygon(vertexes.Select(x => new PolygonPoint(x.X, x.Y)));

            P2T.Triangulate(polygon);

            foreach (var triangle in polygon.Triangles)
            {
                System.Windows.Shapes.Polygon poly = new System.Windows.Shapes.Polygon();
                poly.Points.Add(new Point(triangle.Points[0].X, triangle.Points[0].Y));
                poly.Points.Add(new Point(triangle.Points[1].X, triangle.Points[1].Y));
                poly.Points.Add(new Point(triangle.Points[2].X, triangle.Points[2].Y));

                poly.Stroke = _areaBrush;

                var texture = _mapData.TextureData[SelectedSector.FloorTextureId];

                var fileInfo = new FileInfo(texture);

                var image = new BitmapImage(new Uri(fileInfo.FullName));

                poly.Fill = new System.Windows.Media.ImageBrush(image);

                canvasXZ.Children.Add(poly);
            }
        }
    }
}