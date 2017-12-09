using FunAndGamesWithSharpDX.Entities;
using SharpDX;
using System.Collections.Generic;

namespace DungeonHack.QuadTree
{
    public class QuadTreeNode
    {
        public BoundingBox BoundingBox { get; set; }

        public IEnumerable<Polygon> Polygons { get; set; }

        public QuadTreeNode Parent { get; set; }

        public QuadTreeNode Octant1 { get; set; }

        public QuadTreeNode Octant2 { get; set; }

        public QuadTreeNode Octant3 { get; set; }

        public QuadTreeNode Octant4 { get; set; }

        public bool IsLeaf { get; internal set; }
    }
}
