using DungeonHack.Entities;
using FunAndGamesWithSharpDX.Entities;
using SharpDX;
using System.Collections.Generic;
using System.Linq;
using DungeonHack.DirectX;
using SharpDX.Direct3D11;

namespace DungeonHack.QuadTree
{
    public class QuadTreeBuilder
    {
        public int TreeDepth { get; private set; }

        public int NumberOfNodes { get; private set; }

        public IList<QuadTreeNode> LeafNodeList { get; private set; }

        private readonly Device _device;

        public QuadTreeBuilder(Device device)
        {
            _device = device;
        }

        public QuadTreeNode BuildTree(IEnumerable<Polygon> polygons)
        {
            NumberOfNodes = 1;

            LeafNodeList = new List<QuadTreeNode>();
           
            Vector3 minimumVector = new Vector3();
            Vector3 maximumVector = new Vector3();

            foreach (var polygon in polygons)
            {
                var minBox = polygon.BoundingBox.BoundingBox.Minimum;
                var maxBox = polygon.BoundingBox.BoundingBox.Maximum;

                if (minBox.X < minimumVector.X)
                {
                    minimumVector.X = minBox.X;
                }

                if (minBox.Z < minimumVector.Z)
                {
                    minimumVector.Z = minBox.Z;
                }

                if (minBox.Y < minimumVector.Y)
                {
                    minimumVector.Y = minBox.Y;
                }

                if (maxBox.X > maximumVector.X)
                {
                    maximumVector.X = maxBox.X;
                }

                if (maxBox.Z > maximumVector.Z)
                {
                    maximumVector.Z = maxBox.Z;
                }

                if (maxBox.Y > maximumVector.Y)
                {
                    maximumVector.Y = maxBox.Y;
                }
            }

            QuadTreeNode rootNode = new QuadTreeNode()
            {
                Id = NumberOfNodes,
                Polygons = polygons.ToList(),
                BoundingBox = new AABoundingBox(new BoundingBox()
                {
                    Minimum = minimumVector,
                    Maximum = maximumVector
                }, new BufferFactory(_device))
            };

            BuildQuadTree(rootNode, polygons, 0);

            MarkLeaves(rootNode);

            return rootNode;
        }

        private void MarkLeaves(QuadTreeNode node)
        {
            if ((node.Octant1 == null) && (node.Octant2 == null) && (node.Octant3 == null) && (node.Octant4 == null))
            {
                node.IsLeaf = true;

                LeafNodeList.Add(node);
                return;
            }

            if (node.Octant1 != null)
                MarkLeaves(node.Octant1);

            if (node.Octant2 != null)
                MarkLeaves(node.Octant2);

            if (node.Octant3 != null)
                MarkLeaves(node.Octant3);

            if (node.Octant4 != null)
                MarkLeaves(node.Octant4);
        }


        private void BuildQuadTree(QuadTreeNode node, IEnumerable<Polygon> polygons, int treeDepth)
        {
            TreeDepth = treeDepth;

            //Build subnodes
            var min = node.BoundingBox.BoundingBox.Minimum ;
            var max = node.BoundingBox.BoundingBox.Maximum ;
            var halfMax = (node.BoundingBox.BoundingBox.Maximum - node.BoundingBox.BoundingBox.Minimum) / 2;

            //octant1
            node.Octant1 = CreateQuadTreeNode(min, 
                                           new Vector3(min.X + halfMax.X, max.Y, min.Z + halfMax.Z), 
                                           node, polygons, treeDepth);

            //octant2
            node.Octant2 = CreateQuadTreeNode(new Vector3(min.X + halfMax.X, min.Y, min.Z),
                                           new Vector3(max.X, max.Y, min.Z + halfMax.Z), 
                                           node, polygons, treeDepth);

            //octant3
            node.Octant3 = CreateQuadTreeNode(new Vector3(min.X + halfMax.X, min.Y, min.Z + halfMax.Z),
                                           new Vector3(max.X, max.Y, max.Z),
                                           node, polygons, treeDepth);

            //octant4
            node.Octant4 = CreateQuadTreeNode(new Vector3(min.X, min.Y, min.Z + halfMax.Z),
                                           new Vector3(min.X + halfMax.X, max.Y, max.Z),
                                           node, polygons, treeDepth);
        }

        public QuadTreeNode CreateQuadTreeNode(Vector3 minimum, Vector3 maximum, QuadTreeNode parent, IEnumerable<Polygon> polygons, int treeDepth)
        {
            NumberOfNodes++;

            QuadTreeNode node = new QuadTreeNode
            {
                Id = NumberOfNodes,
                Parent = parent,
                BoundingBox = new AABoundingBox(new BoundingBox(minimum, maximum), new BufferFactory(_device))
            };

            //contained or intersected polygons...that means intersected polygons will end up in more than 1 octant.
            var containedPolygons = polygons.Where(x => node.BoundingBox.BoundingBox.Contains(x.BoundingBox.BoundingBox) != ContainmentType.Disjoint);

            if (containedPolygons.Count() == 0)
                return null;

            node.Polygons = containedPolygons.ToList();

            if ((node.Polygons.Count() > 64) &&
                    (treeDepth < 2))
            { 
                BuildQuadTree(node, containedPolygons, treeDepth + 1);
            }

            return node;
        }
    }
}
