using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DungeonHack.BSP.LeafBsp
{
    public class Node
    {
        public bool IsLeaf { get; set; }
        public int Plane { get; set; }
        public int Front { get; set; }
        public int Back { get; set; }
        public BoundingBox BoundingBox { get; set; }
    }
}
