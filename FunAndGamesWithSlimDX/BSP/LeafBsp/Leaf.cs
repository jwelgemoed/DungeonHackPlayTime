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
        public long StartPolygon { get; set; }
        public long EndPolygon { get; set; }
        public long[] PortalIndexList { get; set; }
        public long NumberOfPortals { get; set; }
        public long PVSIndex { get; set; }
        public BoundingBox BoundingBox { get; set; }
    }
}
