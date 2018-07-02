using FunAndGamesWithSharpDX.Entities;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DungeonHack.Entities;

namespace DungeonHack.Octree
{
    public class OctreeBuilder
    {
        
        public OctreeNode BuildTree(IEnumerable<Polygon> polygons)
        {
            OctreeNode rootNode = new OctreeNode();
            rootNode.Polygons = polygons;
           
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

                if (minBox.Y < minimumVector.Y)
                {
                    minimumVector.Y = minBox.Y;
                }

                if (minBox.Z < minimumVector.Z)
                {
                    minimumVector.Z = minBox.Z;
                }

                if (maxBox.X > maximumVector.X)
                {
                    maximumVector.X = maxBox.X;
                }

                if (maxBox.Y > maximumVector.Y)
                {
                    maximumVector.Y = maxBox.Y;
                }

                if (maxBox.Z > maximumVector.Z)
                {
                    maximumVector.Z = maxBox.Z;
                }
            }

            rootNode.BoundingBox = new SharpDX.BoundingBox()
            {
                Minimum = minimumVector,
                Maximum = maximumVector
            };

            BuildOctree(rootNode, polygons, 0);

            MarkLeaves(rootNode);

            return rootNode;
        }

        private void MarkLeaves(OctreeNode node)
        {
            if ((node.Octant1 == null) && (node.Octant2 == null) && (node.Octant3 == null) && (node.Octant4 == null) &&
                    (node.Octant5 == null) && (node.Octant6 == null) && (node.Octant7 == null) && (node.Octant8 == null))
            {
                node.IsLeaf = true;
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

            if (node.Octant5 != null)
                MarkLeaves(node.Octant5);

            if (node.Octant6 != null)
                MarkLeaves(node.Octant6);

            if (node.Octant7 != null)
                MarkLeaves(node.Octant7);

            if (node.Octant8 != null)
                MarkLeaves(node.Octant8);
        }


        private void BuildOctree(OctreeNode node, IEnumerable<Polygon> polygons, int treeDepth)
        {
            //Build subnodes
            var min = node.BoundingBox.Minimum;
            var max = node.BoundingBox.Maximum;
            var halfMax = (node.BoundingBox.Maximum - node.BoundingBox.Minimum) / 2;

            //octant1
            node.Octant1 = CreateOctreeNode(min, new Vector3(min.X + halfMax.X, min.Y + halfMax.Y, min.Z + halfMax.Z), node, polygons, treeDepth);

            //octant2
            node.Octant2 = CreateOctreeNode(new Vector3(min.X + halfMax.X, min.Y, min.Z),
                                                  new Vector3(max.X, min.Y + halfMax.Y, min.Z + halfMax.Z), 
                                                  node, polygons, treeDepth);

            //octant3
            node.Octant3 = CreateOctreeNode(new Vector3(min.X + halfMax.X, min.Y + halfMax.Y, min.Z),
                                           new Vector3(max.X, max.Y, min.Z + halfMax.Z),
                                           node,
                                           polygons,
                                           treeDepth);

            //octant4
            node.Octant4 = CreateOctreeNode(new Vector3(min.X, min.Y + halfMax.Y, min.Z),
                                           new Vector3(min.X + halfMax.X, max.Y, min.Z + halfMax.Z),
                                           node,
                                           polygons,
                                           treeDepth);

            //octant5
            node.Octant5 = CreateOctreeNode(new Vector3(min.X, min.Y, min.Z + halfMax.Z), 
                                            new Vector3(min.X + halfMax.X, min.Y + halfMax.Y, max.Z), 
                                            node, 
                                            polygons,
                                            treeDepth);

            //octant6
            node.Octant6 = CreateOctreeNode(new Vector3(min.X + halfMax.X, min.Y, min.Z + halfMax.Z),
                                                  new Vector3(max.X, min.Y + halfMax.Y, max.Z),
                                                  node, polygons, treeDepth);

            //octant7
            node.Octant7 = CreateOctreeNode(new Vector3(min.X + halfMax.X, min.Y + halfMax.Y, min.Z + halfMax.Z),
                                           new Vector3(max.X, max.Y, max.Z),
                                           node,
                                           polygons,
                                           treeDepth);

            //octant8
            node.Octant8 = CreateOctreeNode(new Vector3(min.X, min.Y + halfMax.Y, min.Z + halfMax.Z),
                                           new Vector3(min.X + halfMax.X, max.Y, max.Z),
                                           node,
                                           polygons,
                                           treeDepth);

            //if ((node.Octant1 == null) && (node.Octant2 == null) && (node.Octant3 == null) && (node.Octant4 == null) &&
            //        (node.Octant5 == null) && (node.Octant6 == null) && (node.Octant7 == null) && (node.Octant8 == null))
            //{
            //    node.IsLeaf = true;
            //}

        }

        public OctreeNode CreateOctreeNode(Vector3 minimum, Vector3 maximum, OctreeNode parent, IEnumerable<Polygon> polygons, int treeDepth)
        {
            OctreeNode node = new OctreeNode
            {
                Parent = parent,
                BoundingBox = new BoundingBox(minimum, maximum)
            };

            //contained or intersected polygons...that means intersected polygons will end up in more than 1 octant.
            var containedPolygons = polygons.Where(x => node.BoundingBox.Contains(x.BoundingBox.BoundingBox) != ContainmentType.Disjoint);

            if (containedPolygons.Count() == 0)
                return null;

            node.Polygons = containedPolygons;

            if ((node.Polygons.Count() > 5) &&
                    (treeDepth < 3))
            {
                BuildOctree(node, containedPolygons, treeDepth + 1);
            }

            return node;
        }
    }
}
