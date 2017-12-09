using FunAndGamesWithSharpDX.Engine;
using FunAndGamesWithSharpDX.Entities;
using System.Collections.Generic;
using System.Windows.Forms;

namespace DungeonHack.QuadTree
{
    public class QuadTreeRenderer
    {
        private PolygonRenderer _renderer;
        private Polygon[] _renderList = new Polygon[5000];
        private Stack<QuadTreeNode> _nodeStack = new Stack<QuadTreeNode>();
        private int _endOfList = 0;
 
        public QuadTreeRenderer(PolygonRenderer renderer)
        {
            _renderer = renderer;
        }

        public void DrawQuadTree(QuadTreeNode node, Frustrum frustrum, Camera camera, ref int meshRenderedCount)
        {
            //DrawQuadTreeRecursive(node, frustrum, camera);
            DrawQuadTreeIterative(node, frustrum);
            //DrawQuadTreeAlternative(node, camera);

            for (int i = 0; i < _endOfList; i++)
            {
                _renderer.Render(frustrum, _renderList[i], ref meshRenderedCount);
            }

            _endOfList = 0;
        }

        private void DrawQuadTreeAlternative(QuadTreeNode root, Camera camera)
        {
            _nodeStack.Push(root);
            QuadTreeNode node;

            while (_nodeStack.Count > 0)
            {
                node = _nodeStack.Pop();

                if (node.IsLeaf)
                {
                    foreach (var polygon in node.Polygons)
                    {
                        _renderList[_endOfList] = polygon;
                        _endOfList++;
                    }

                    break;
                }
                else
                {
                    if (node.Octant1 != null && node.Octant1.BoundingBox.Contains(camera.EyeAt) == SharpDX.ContainmentType.Contains)
                    {
                        _nodeStack.Push(node.Octant1);
                    }
                    if (node.Octant2 != null && node.Octant2.BoundingBox.Contains(camera.EyeAt) == SharpDX.ContainmentType.Contains)
                    {
                        _nodeStack.Push(node.Octant2);
                    }
                    if (node.Octant3 != null && node.Octant3.BoundingBox.Contains(camera.EyeAt) == SharpDX.ContainmentType.Contains)
                    {
                        _nodeStack.Push(node.Octant3);
                    }
                    if (node.Octant4 != null && node.Octant4.BoundingBox.Contains(camera.EyeAt) == SharpDX.ContainmentType.Contains)
                    {
                        _nodeStack.Push(node.Octant4);
                    }
                }
            }
        }

        private void DrawQuadTreeIterative(QuadTreeNode root, Frustrum frustrum)
        {
            _nodeStack.Push(root);
            QuadTreeNode node;

            while (_nodeStack.Count > 0)
            {
                node = _nodeStack.Pop();

                if (node.IsLeaf)
                {
                    foreach (var polygon in node.Polygons)
                    {
                        _renderList[_endOfList] = polygon;
                        _endOfList++;
                    }
                }
                else
                {
                    if (node.Octant1 != null && frustrum.CheckBoundingBox(node.Octant1.BoundingBox) != 0)
                    {
                        _nodeStack.Push(node.Octant1);
                    }
                    if (node.Octant2 != null && frustrum.CheckBoundingBox(node.Octant2.BoundingBox) != 0)
                    {
                        _nodeStack.Push(node.Octant2);
                    }
                    if (node.Octant3 != null && frustrum.CheckBoundingBox(node.Octant3.BoundingBox) != 0)
                    {
                        _nodeStack.Push(node.Octant3);
                    }
                    if (node.Octant4 != null && frustrum.CheckBoundingBox(node.Octant4.BoundingBox) != 0)
                    {
                        _nodeStack.Push(node.Octant4);
                    }
                }
            }
        }


        private void DrawQuadTreeRecursive(QuadTreeNode node, Frustrum frustrum)
        {
            if (node.Octant1 != null)
            {
                if (frustrum.CheckBoundingBox(node.Octant1.BoundingBox) != 0)
                    DrawQuadTreeRecursive(node.Octant1, frustrum);
            }

            if (node.Octant2 != null)
            {
                if (frustrum.CheckBoundingBox(node.Octant2.BoundingBox) != 0)
                    DrawQuadTreeRecursive(node.Octant2, frustrum);
            }

            if (node.Octant3 != null)
            {
                if (frustrum.CheckBoundingBox(node.Octant3.BoundingBox) != 0)
                    DrawQuadTreeRecursive(node.Octant3, frustrum);
            }

            if (node.Octant4 != null)
            {
                if (frustrum.CheckBoundingBox(node.Octant4.BoundingBox) != 0)
                    DrawQuadTreeRecursive(node.Octant4, frustrum);
            }

            if (node.IsLeaf)
            {
                foreach (var polygon in node.Polygons)
                {
                    //_renderList.Add(polygon);
                }
            }
        }
     }
}
