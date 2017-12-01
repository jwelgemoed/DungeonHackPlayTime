using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

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
    }
}
