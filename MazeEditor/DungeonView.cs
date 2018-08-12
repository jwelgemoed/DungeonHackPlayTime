using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace MazeEditor
{
    public class DungeonView
    {
        private Dungeon _dungeon;

        private Canvas _canvas;

        public Brush RoomBrush { get; set; }

        public Brush CorridorBrush { get; set; }

        public Brush PlayerStartBrush { get; set; }

        public Brush ItemLocationBrush { get; set; }

        private int _gridSize = 8;

        public DungeonView(Dungeon dungeon, Canvas canvas)
        {
            _dungeon = dungeon;
            _canvas = canvas;
            RoomBrush = new SolidColorBrush(Color.FromRgb(100, 0, 0));
            CorridorBrush = new SolidColorBrush(Color.FromRgb(0, 0, 100));
            PlayerStartBrush = new SolidColorBrush(Color.FromRgb(0, 100, 0));
            ItemLocationBrush = new SolidColorBrush(Color.FromRgb(100, 100, 0));
        }

        public void DrawCanvas()
        {
            _canvas.Children.Clear();

            for (int i = 0; i < _dungeon.GridBoard.SizeX; i++)
            {
                for (int j = 0; j < _dungeon.GridBoard.SizeY; j++)
                {
                    if (_dungeon.GridBoard.Grid[i, j] == NodeType.Empty)
                        continue;

                    Rectangle _rectangle = new Rectangle()
                    {
                        Width = _gridSize,
                        Height = _gridSize
                    };
                    
                    switch (_dungeon.GridBoard.Grid[i,j])
                    {
                        case NodeType.Corridor:
                            _rectangle.Fill = CorridorBrush;
                            break;
                        case NodeType.Room:
                            _rectangle.Fill = RoomBrush;
                            break;
                        case NodeType.Item:
                            _rectangle.Fill = ItemLocationBrush;
                            break;
                        case NodeType.PlayerStart:
                            _rectangle.Fill = PlayerStartBrush;
                            break;
                        default:
                            break;
                    }

                    _canvas.Children.Add(_rectangle);

                    Canvas.SetLeft(_rectangle, (i * _gridSize) + 1);
                    Canvas.SetTop(_rectangle, (j * _gridSize) + 1);
                }
            }
        }
    }
}
