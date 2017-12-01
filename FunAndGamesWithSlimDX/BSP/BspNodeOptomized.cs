using FunAndGamesWithSharpDX.Entities;
using SharpDX;
using System.Collections.Generic;

namespace DungeonHack.BSP
{
    public class BspNodeOptomized
    {
        public Polygon Splitter { get; set; }

        public IEnumerable<Polygon> ConvexPolygonSet { get; set; }

        public BoundingBox? BoundingVolume { get; set; }

        public int Front { get; set; }

        public int Back { get; set; }

        public int Parent { get; set; }

        public bool IsLeaf { get; set; }

        public bool IsSolid { get; set; }

        public bool IsRoot { get; set; }

    }
}
