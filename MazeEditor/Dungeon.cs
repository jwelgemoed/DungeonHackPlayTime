using System;

namespace MazeEditor
{
    public class Dungeon
    {
        public GridBoard GridBoard { get; set; }

        public Dungeon(int sizex, int sizey)
        {
            GridBoard = new GridBoard(sizex, sizey);
        }

        public void SetCorridor(int x, int y)
        {
            GridBoard.SetGridNodeType(x, y, NodeType.Corridor);
        }

        public void SetRoom(int x, int y, int sizex, int sizey)
        {
            for (int i = x; i < (x + sizex); i++)
            {
                for (int j = y; j < (y + sizey); j++)
                {
                    GridBoard.SetGridNodeType(i, j, NodeType.Room);
                }
            }
        }

        public Tuple<int, int> GetPlayerStartLocation()
        {
            return GetLocation(NodeType.PlayerStart);
        }

        public Tuple<int, int> GetItemLocation()
        {
            return GetLocation(NodeType.Item);
        }

        private Tuple<int, int> GetLocation(NodeType nodeType)
        {
            for (int i = 0; i < GridBoard.SizeX; i++)
            {
                for (int j = 0; j < GridBoard.SizeY; j++)
                {
                    if (GridBoard.Grid[i, j] == nodeType)
                    {
                        return new Tuple<int, int>(i, j);
                    }
                }
            }

            return new Tuple<int, int>(0, 0);
        }
    }
}
