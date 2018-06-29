using DungeonHack.QuadTree;
using FunAndGamesWithSharpDX.Entities;
using SharpDX;

namespace DungeonHack.Entities
{
    public class Item
    {
        public Polygon Polygon { get; set; }

        public Vector3 Location { get; set; }

        public QuadTreeNode LeafNode { get; set; }
    }
}
