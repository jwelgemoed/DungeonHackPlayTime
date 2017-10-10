using FunAndGamesWithSharpDX.Entities;
using System.Collections.Generic;

namespace DungeonHack.BSP.LeafBsp
{
    public class LeafStackItem
    {
        private int _node;
        private List<Polygon> _meshes;

        public LeafStackItem(int node, List<Polygon> meshes)
        {
            _node = node;
            _meshes = meshes;
        }

        public int NumberOfNodes { get { return _node; } }

        public List<Polygon> Meshes { get { return _meshes; }  }
    }
}
