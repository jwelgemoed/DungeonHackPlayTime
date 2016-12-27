using FunAndGamesWithSlimDX.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DungeonHack.BSP
{
    public static class BspSector
    {
        private static List<Tuple<int, IEnumerable<Mesh>>> _sectors;

        public static int NumberOfSectors
        {
            get
            {
                return _sectors.Count;
            }
        }
        
        public static void DetermineSectors(BspNode rootNode)
        {
            _sectors = new List<Tuple<int, IEnumerable<Mesh>>>();
            List<BspNode> leafNodes = new List<BspNode>();
            
            FindAllLeafNodes(rootNode, leafNodes);

            int i = 0;

            foreach (var node in leafNodes)
            {
                _sectors.Add(new Tuple<int, IEnumerable<Mesh>>(i, node.ConvexPolygonSet));
                i++;
            }
        }

        public static int? FindSector(Mesh mesh)
        {
            return _sectors.Find(x => (x.Item2 ?? new List<Mesh>()).Contains(mesh))?.Item1;
        }
        
        private static void FindAllLeafNodes(BspNode node, List<BspNode> leafNodes)
        {
            if (node.IsLeaf)
            {
                leafNodes.Add(node);
                return;
            }

            FindAllLeafNodes(node.Front, leafNodes);
            FindAllLeafNodes(node.Back, leafNodes);
        }
    }
}
