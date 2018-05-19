using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media;

namespace MazeEditor
{
    //http://weblog.jamisbuck.org/2011/1/27/maze-generation-growing-tree-algorithm#
    public class CorridorCreator
    {
        private Dungeon _dungeon;
        private Canvas _canvas;
        private Random _random;
        private List<Tuple<int, int>> _cells;
        private int _step = 2;
        private int[] _dx = { -1, 1, 0, 0 };
        private int[] _dy = { 0, 0, -1, 1 };
        private int _gridSize = 8;
        private Brush _corridorBrush;

        public CorridorCreator(Dungeon dungeon, Canvas canvas)
        {
            _dungeon = dungeon;
            _random = new Random((int)DateTime.Now.Ticks);
            _cells = new List<Tuple<int, int>>();
            _canvas = canvas;
            _corridorBrush = new SolidColorBrush(Color.FromRgb(0, 0, 100));
        }

        public void GrowingTree(bool visualize, SelectionMethodType selectionMethod)
        {
            var firstPosition = FindNextXY();
            _cells.Add(firstPosition);

            while (_cells.Any())
            {
                int index = GetNextCellIndex(selectionMethod);
                int x = _cells[index].Item1;
                int y = _cells[index].Item2;
                bool[] directions = { false, false, false, false };

                bool emptyCellfound = false;
                //0 = W, 1=E, 2=N, 3=S

                while (!(directions[0] && directions[1] && directions[2] && directions[3]))
                {
                    var direction = _random.Next(0, 4);

                    if (directions[direction])
                        continue;

                    directions[direction] = true;
                    int nx = x + (_dx[direction] * _step);
                    int ny = y + (_dy[direction] * _step);

                    if (nx > 0 && ny > 0 && nx < _dungeon.GridBoard.SizeX && ny < _dungeon.GridBoard.SizeY
                        && _dungeon.GridBoard.Grid[nx, ny] == NodeType.Empty)
                    {
                        int startx = x <= nx ? x : nx;
                        int starty = y <= ny ? y : ny;
                        int endx = x > nx ? x : nx;
                        int endy = y > ny ? y : ny;

                        for (int i = startx; i <= endx; i++)
                        {
                            _dungeon.GridBoard.Grid[i, starty] = NodeType.Corridor;
                        }

                        for (int j = starty; j <= endy; j++)
                        {
                            _dungeon.GridBoard.Grid[startx, j] = NodeType.Corridor;
                        }

                        System.Threading.Thread.Sleep(1);

                        _cells.Add(new Tuple<int, int>(nx, ny));
                        emptyCellfound = true;
                        break;
                    }
                }

                if (!emptyCellfound)
                {
                    _cells.RemoveAt(index);
                }
            }
        }

        public void SetPlayerStartRandomLocation()
        {
            bool placed = false;
            Random random = new Random();

            while (!placed)
            {
                int x = _random.Next(_dungeon.GridBoard.SizeX);
                int y = _random.Next(_dungeon.GridBoard.SizeY);

                if (_dungeon.GridBoard.Grid[x, y] == NodeType.Corridor || 
                    _dungeon.GridBoard.Grid[x, y] == NodeType.Room)
                {
                    _dungeon.GridBoard.Grid[x, y] = NodeType.PlayerStart;
                    placed = true;
                }
            }
        }

        public void SetItemLocation()
        {
            bool placed = false;
            Random random = new Random();

            while (!placed)
            {
                int x = _random.Next(_dungeon.GridBoard.SizeX);
                int y = _random.Next(_dungeon.GridBoard.SizeY);

                if (_dungeon.GridBoard.Grid[x, y] == NodeType.Corridor ||
                    _dungeon.GridBoard.Grid[x, y] == NodeType.Room)
                {
                    _dungeon.GridBoard.Grid[x, y] = NodeType.Item;
                    placed = true;
                }
            }
        }

        private int GetNextCellIndex(SelectionMethodType method)
        {
            switch (method)
            {
                case SelectionMethodType.Newest: return _cells.Count - 1;
                case SelectionMethodType.Oldest: return 0;
                case SelectionMethodType.Random: return _random.Next(_cells.Count);

            }

            return _cells.Count - 1;
        }

        private Tuple<int, int> FindNextXY()
        {
            bool done = false;
            int maxAmount = _dungeon.GridBoard.SizeX * _dungeon.GridBoard.SizeY;
            int counter = 0;

            while (!done)
            {
                counter++;

                int x = _random.Next(_dungeon.GridBoard.SizeX);
                int y = _random.Next(_dungeon.GridBoard.SizeY);

                if (_dungeon.GridBoard.Grid[x, y] == NodeType.Empty)
                {
                    return new Tuple<int, int>(x, y);
                }

                if (counter == maxAmount)
                {
                    done = true;
                }
            }

            return null;
        }

    }
}
