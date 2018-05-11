using DungeonHack.QuadTree;
using FunAndGamesWithSharpDX.Engine;
using System.Collections.Generic;

namespace DungeonHack.Quadtree
{
    public class QuadTreeTraverser
    {
        private QuadTreeNode _rootNode;
        private Stack<QuadTreeNode> _nodeStack;

        public QuadTreeTraverser(QuadTreeNode rootNode)
        {
            _rootNode = rootNode;
            _nodeStack = new Stack<QuadTreeNode>();
        }

        public QuadTreeNode FindCurrentCameraLeafNode(Camera camera)
        {
            QuadTreeNode node;
            _nodeStack.Clear();
            _nodeStack.Push(_rootNode);
            int depth = 1;

            while (_nodeStack.Count > 0)
            {
                depth--;
                node = _nodeStack.Pop();

                if (node.IsLeaf)
                {
                    //We've arrived at the leaf.
                    return node;
                }
                else
                {
                    if ((node.Octant1 != null) && (node.Octant1.BoundingBox.ContainsOrIntersectsCamera(camera)))
                    {
                        _nodeStack.Push(node.Octant1);
                        depth++;
                    }
                    if ((node.Octant2 != null) && (node.Octant2.BoundingBox.ContainsOrIntersectsCamera(camera)))
                    {
                        _nodeStack.Push(node.Octant2);
                        depth++;
                    }
                    if ((node.Octant3 != null) && (node.Octant3.BoundingBox.ContainsOrIntersectsCamera(camera)))
                    {
                        _nodeStack.Push(node.Octant3);
                        depth++;
                    }
                    if ((node.Octant4 != null) && (node.Octant4.BoundingBox.ContainsOrIntersectsCamera(camera)))
                    {
                        _nodeStack.Push(node.Octant4);
                        depth++;
                    }
                }
            }

            return null;
        }
    }
}
