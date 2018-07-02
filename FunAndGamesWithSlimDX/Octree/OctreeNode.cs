using FunAndGamesWithSharpDX.Entities;
using SharpDX;
using System.Collections.Generic;
using DungeonHack.Entities;

namespace DungeonHack.Octree
{
    public class OctreeNode
    {
        public BoundingBox BoundingBox { get; set; }

        public IEnumerable<Polygon> Polygons { get; set; }

        public OctreeNode Parent { get; set; }

        public OctreeNode Octant1 { get; set; }

        public OctreeNode Octant2 { get; set; }

        public OctreeNode Octant3 { get; set; }

        public OctreeNode Octant4 { get; set; }

        public OctreeNode Octant5 { get; set; }

        public OctreeNode Octant6 { get; set; }

        public OctreeNode Octant7 { get; set; }

        public OctreeNode Octant8 { get; set; }
        public bool IsLeaf { get; internal set; }
    }
}
