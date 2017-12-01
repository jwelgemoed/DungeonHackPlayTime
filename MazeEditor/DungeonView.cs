using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        private int _gridSize = 8;

        public DungeonView(Dungeon dungeon, Canvas canvas)
        {
            _dungeon = dungeon;
            _canvas = canvas;
            RoomBrush = new SolidColorBrush(Color.FromRgb(100, 0, 0));
            CorridorBrush = new SolidColorBrush(Color.FromRgb(0, 0, 100));
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

                    if (_dungeon.GridBoard.Grid[i, j] == NodeType.Corridor)
                    {
                        _rectangle.Fill = CorridorBrush;
                    }
                    else
                    {
                        _rectangle.Fill = RoomBrush;
                    }

                    _canvas.Children.Add(_rectangle);

                    Canvas.SetLeft(_rectangle, (i * _gridSize) + 1);
                    Canvas.SetTop(_rectangle, (j * _gridSize) + 1);
                }
            }
        }
    }
}
