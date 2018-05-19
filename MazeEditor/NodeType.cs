using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MazeEditor
{
    public enum NodeType
    {
        Empty = 0,
        Corridor = 1,
        Room = 2,
        Item = 3,
        PlayerStart = 4
    }
}
