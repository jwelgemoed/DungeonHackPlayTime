using FunAndGamesWithSlimDX.Entities;
using SlimDX;
using System.Collections.Generic;

namespace DungeonHack.BSP
{
    public class BspNode
    {
        public Mesh Splitter { get; set; }

        public BoundingBox? BoundingVolume { get; set; }

        public BspNode Front { get; set; }

        public BspNode Back { get; set; }

        public BspNode Parent { get; set; }

        public bool IsLeaf { get; set; }

        public bool IsSolid { get; set; }

        public bool IsRoot { get; set; }
    }
}
