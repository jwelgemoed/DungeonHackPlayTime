using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

    }
}
