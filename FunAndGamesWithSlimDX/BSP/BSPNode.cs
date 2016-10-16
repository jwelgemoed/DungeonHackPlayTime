using FunAndGamesWithSlimDX.Entities;

namespace DungeonHack.BSP
{
    public class BspNode
    {
        public Mesh Splitter { get; set; }

        public BspNode Front { get; set; }

        public BspNode Back { get; set; }

        public bool IsLeaf { get; set; }

        public bool IsSolid { get; set; }
    }
}
