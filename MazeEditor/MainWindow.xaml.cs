﻿using DungeonHack.BSP;
using DungeonHack.BSP.LeafBsp;
using DungeonHack.Octree;
using DungeonHack.QuadTree;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace MazeEditor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<MyRectangle> _rectangles = new List<MyRectangle>();
        private int MaxAttempts = 500;
        private Dungeon _dungeon;
        private MazeRunner _mazeRunner;

        private int MaxMazeWidth { get { return (int)maxMazeWidthSlider.Value; } }
        private int MaxMazeHeight { get { return (int)maxMazeHeightSlider.Value; } }
        private int MaxRoomSize { get { return (int)maxRoomSizeSlider.Value; } }
        private int MinRoomSize { get { return (int)minRoomSizeSlider.Value; } }
        private bool OverlappingRoomsAllowed { get { return OverlappingRoomsChkBox.IsChecked ?? false; } }
        private int NumberOfRooms { get { return (int) numberOfRoomsSlider.Value; } }
        private SelectionMethodType SelectionMethod { get { return GetSelectionMethodTypeFromListBox(); } }

        public MainWindow()
        {
            InitializeComponent();
            _mazeRunner = new MazeRunner();

            _mazeRunner.Initialize(4);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            _dungeon = new Dungeon(MaxMazeWidth, MaxMazeHeight);
            var dungeonView = new DungeonView(_dungeon, canvas);
            Random randomizer = new Random((int)DateTime.UtcNow.Ticks);
            canvas.Children.Clear();
            _rectangles.Clear();

            for (int i = 0; i < NumberOfRooms; i++)
            {
                bool done = false;
                int attempts = 0;
                while (!done && (attempts < MaxAttempts))
                {
                    attempts++;
                    MyRectangle rectangle = new MyRectangle()
                    {
                        Height = randomizer.Next(MinRoomSize, MaxRoomSize),
                        Width = randomizer.Next(MinRoomSize, MaxRoomSize)
                    };

                    rectangle.PositionX = randomizer.Next(0, MaxMazeWidth - (int)rectangle.Width);
                    rectangle.PositionY = randomizer.Next(0, MaxMazeHeight - (int)rectangle.Height);

                    if (_rectangles.Exists(x => x.Overlaps(rectangle)) && (!OverlappingRoomsAllowed))
                    {
                        continue;
                    }

                    _dungeon.SetRoom(rectangle.PositionX, rectangle.PositionY, (int)rectangle.Width, (int)rectangle.Height);

                    _rectangles.Add(rectangle);
                   
                    done = true;
                }
            }

            CorridorCreator corridorCreator = new CorridorCreator(_dungeon, canvas);

            corridorCreator.GrowingTree(false, SelectionMethod);

            dungeonView.DrawCanvas();

        }

        private SelectionMethodType GetSelectionMethodTypeFromListBox()
        {
            string content = string.Empty;

            foreach (var item in selectionMethodListBox.Items)
            {
                var radioButton = item as RadioButton;

                if (radioButton.IsChecked ?? false)
                {
                    content = radioButton.Content as string;
                }
            }

            switch (content)
            {
                case "Latest":
                    return SelectionMethodType.Newest;
                case "Oldest":
                    return SelectionMethodType.Oldest;
                case "Random":
                    return SelectionMethodType.Random;
                default:
                    return SelectionMethodType.Newest;

            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            GridPolygonBuilder builder = new GridPolygonBuilder(_dungeon.GridBoard, _mazeRunner.Device, _mazeRunner.GetShader);
            var meshList = builder.GeneratePolygons();

            _mazeRunner.Meshes = meshList.ToList();

            BspTreeBuilder bspTreeBuilder = new BspTreeBuilder(_mazeRunner.Device, _mazeRunner.GetShader);
            BspBoundingVolumeCalculator bspBoudingVolumeCalculator = new BspBoundingVolumeCalculator();
            LeafBspTreeBuilder leafTreeBuilder = new LeafBspTreeBuilder(_mazeRunner.Device, _mazeRunner.GetShader);

           /* var rootNode = bspTreeBuilder.BuildTree(_mazeRunner.Meshes);
            bspBoudingVolumeCalculator.ComputeBoundingVolumes(rootNode);

            _mazeRunner.BspNodes = bspTreeBuilder.TransformNodesToOptomizedNodes(rootNode);*/

            _mazeRunner.InitializeScene();

            var octbuilder = new OctreeBuilder();

            var octRootNode = octbuilder.BuildTree(_mazeRunner.Meshes);
            _mazeRunner.OctreeRootNode = octRootNode;

            var quadTreeBuilder = new QuadTreeBuilder();
            var quadtreeNode = quadTreeBuilder.BuildTree(_mazeRunner.Meshes);
            _mazeRunner.QuadTreeNode = quadtreeNode;

            int numberOfNodes = quadTreeBuilder.NumberOfNodes;
            int treeDepth = quadTreeBuilder.TreeDepth;
           
            _mazeRunner.Start();
            _mazeRunner.Run(1);
        }

        private void btnQuadTree_Click(object sender, RoutedEventArgs e)
        {
            GridPolygonBuilder builder = new GridPolygonBuilder(_dungeon.GridBoard, _mazeRunner.Device, _mazeRunner.GetShader);
            var meshList = builder.GeneratePolygons();

            _mazeRunner.Meshes = meshList.ToList();

            var quadTreeBuilder = new QuadTreeBuilder();
            var root = quadTreeBuilder.BuildTree(_mazeRunner.Meshes);
            Stack<QuadTreeNode> nodeStack = new Stack<QuadTreeNode>();

            nodeStack.Push(root);
            QuadTreeNode node;

            while (nodeStack.Count > 0)
            {
                node = nodeStack.Pop();

                MyRectangle rectangle = new MyRectangle()
                {
                    PositionX = (int)(node.BoundingBox.Minimum.X / 8),
                    PositionY = (int)(node.BoundingBox.Minimum.Z / 8),
                    Height = (node.BoundingBox.Maximum - node.BoundingBox.Minimum).Z /8,
                    Width = (node.BoundingBox.Maximum - node.BoundingBox.Minimum).X /8
                };

                rectangle.ChangeFill(null);
                rectangle.ChangeStroke(new SolidColorBrush(Color.FromRgb(0, 100, 0)));

                rectangle.AddRectangleToCanvas(canvas);

                MessageBox.Show("ok");

                if (!node.IsLeaf)
                {
                    if (node.Octant1 != null)
                    {
                        nodeStack.Push(node.Octant1);
                    }
                    if (node.Octant2 != null)
                    {
                        nodeStack.Push(node.Octant2);
                    }
                    if (node.Octant3 != null)
                    {
                        nodeStack.Push(node.Octant3);
                    }
                    if (node.Octant4 != null)
                    {
                        nodeStack.Push(node.Octant4);
                    }
                }
            }
        }
    }
}
