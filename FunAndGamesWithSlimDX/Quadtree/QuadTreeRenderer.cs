using FunAndGamesWithSharpDX.Engine;
using FunAndGamesWithSharpDX.Entities;
using System.Collections.Generic;

namespace DungeonHack.QuadTree
{
    public class QuadTreeRenderer
    {
        private PolygonRenderer _renderer;
        private int _endOfList = 0;

        public QuadTreeRenderer(PolygonRenderer renderer)
        {
            _renderer = renderer;
        }

        public void DrawQuadTree(int threadNumber, QuadTreeNode node, Frustrum frustrum, Camera camera, ref int meshRenderedCount)
        {
            var renderList = new Polygon[5000];

            DrawQuadTreeIterative(node, frustrum, renderList);

            for (int i = 0; i < _endOfList; i++)
            {
               _renderer.Render(threadNumber, frustrum, renderList[i], ref meshRenderedCount);
            }

            _endOfList = 0;
        }

        private void DrawQuadTreeIterative(QuadTreeNode root, Frustrum frustrum, Polygon[] renderList)
        {
            var nodeStack = new Stack<QuadTreeNode>();
            nodeStack.Push(root);
            QuadTreeNode node;

            while (nodeStack.Count > 0)
            {
                node = nodeStack.Pop();

                if (node.IsLeaf)
                {
                    foreach (var polygon in node.Polygons)
                    {
                        renderList[_endOfList] = polygon;
                        _endOfList++;
                    }
                }
                else
                {
                    if (node.Octant1 != null && frustrum.CheckBoundingBox(node.Octant1.BoundingBox) != 0)
                    {
                        nodeStack.Push(node.Octant1);
                    }
                    if (node.Octant2 != null && frustrum.CheckBoundingBox(node.Octant2.BoundingBox) != 0)
                    {
                        nodeStack.Push(node.Octant2);
                    }
                    if (node.Octant3 != null && frustrum.CheckBoundingBox(node.Octant3.BoundingBox) != 0)
                    {
                        nodeStack.Push(node.Octant3);
                    }
                    if (node.Octant4 != null && frustrum.CheckBoundingBox(node.Octant4.BoundingBox) != 0)
                    {
                        nodeStack.Push(node.Octant4);
                    }
                }
            }
        }
     }
}
