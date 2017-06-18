using SlimDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DungeonHack.BSP.LeafBsp
{
    public class Leaf
    {
        public int StartPolygon { get; set; }
        public int EndPolygon { get; set; }
        public int[] PortalIndexList { get; set; }
        public int NumberOfPortals { get; set; }
        public int PVSIndex { get; set; }
        public BoundingBox BoundingBox { get; set; }
    }
}
