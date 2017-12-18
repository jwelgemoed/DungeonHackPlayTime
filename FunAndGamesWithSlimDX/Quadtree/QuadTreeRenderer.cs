using FunAndGamesWithSharpDX.Engine;
using FunAndGamesWithSharpDX.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DungeonHack.QuadTree
{
    public class QuadTreeRenderer
    {
        private PolygonRenderer _renderer;
        private Polygon[][] _renderList;
        private Stack<QuadTreeNode>[] _nodeStack;
        private int[] _endOfList;
        private int _threadCount;
        private Task[] _tasks;

        public QuadTreeRenderer(PolygonRenderer renderer)
        {
            _threadCount = 4;
            _renderer = renderer;
            _renderList = new Polygon[_threadCount][];
            _endOfList = new int[_threadCount];
            _tasks = new Task[_threadCount];
            _nodeStack = new Stack<QuadTreeNode>[_threadCount];

            for (int i = 0; i < _threadCount; i++)
            {
                _renderList[i] = new Polygon[5000];
                _nodeStack[i] = new Stack<QuadTreeNode>();
            }
        }

        public void DrawQuadTree(QuadTreeNode node, Frustrum frustrum, Camera camera, ref int meshRenderedCount)
        {
            //DrawQuadTreeRecursive(node, frustrum, camera);
            for (int i = 0; i < _threadCount; i++)
            {
                int j = i;
                _tasks[i] = new Task(() => 
                {
                    switch (j)
                    {
                        case 0:
                            DrawQuadTreeIterative(j, node.Octant1, frustrum);
                            break;
                        case 1:
                            DrawQuadTreeIterative(j, node.Octant2, frustrum);
                            break;
                        case 2:
                            DrawQuadTreeIterative(j, node.Octant3, frustrum);
                            break;
                        case 3:
                            DrawQuadTreeIterative(j, node.Octant4, frustrum);
                            break;
                        default:
                            DrawQuadTreeIterative(j, node, frustrum);
                            break;
                    }
                });

                _tasks[i].Start();
            }
            Task.WaitAll(_tasks);

            //DrawQuadTreeAlternative(node, camera);

            for (int i = 0; i < _threadCount; i++)
            {
                for (int j = 0; j < _endOfList[i]; j++)
                {
                    _renderer.Render(frustrum, _renderList[i][j], ref meshRenderedCount);
                }
                _endOfList[i] = 0;
            }
        }

        private void DrawQuadTreeAlternative(int threadCount, QuadTreeNode root, Camera camera)
        {
            _nodeStack[threadCount].Push(root);
            QuadTreeNode node;

            while (_nodeStack[threadCount].Count > 0)
            {
                node = _nodeStack[threadCount].Pop();

                if (node.IsLeaf)
                {
                    foreach (var polygon in node.Polygons)
                    {
                        _renderList[threadCount][_endOfList[threadCount]] = polygon;
                        _endOfList[threadCount]++;
                    }

                    break;
                }
                else
                {
                    if (node.Octant1 != null && node.Octant1.BoundingBox.Contains(camera.EyeAt) == SharpDX.ContainmentType.Contains)
                    {
                        _nodeStack[threadCount].Push(node.Octant1);
                    }
                    if (node.Octant2 != null && node.Octant2.BoundingBox.Contains(camera.EyeAt) == SharpDX.ContainmentType.Contains)
                    {
                        _nodeStack[threadCount].Push(node.Octant2);
                    }
                    if (node.Octant3 != null && node.Octant3.BoundingBox.Contains(camera.EyeAt) == SharpDX.ContainmentType.Contains)
                    {
                        _nodeStack[threadCount].Push(node.Octant3);
                    }
                    if (node.Octant4 != null && node.Octant4.BoundingBox.Contains(camera.EyeAt) == SharpDX.ContainmentType.Contains)
                    {
                        _nodeStack[threadCount].Push(node.Octant4);
                    }
                }
            }
        }

        private void DrawQuadTreeIterative(int threadCount, QuadTreeNode root, Frustrum frustrum)
        {
            _nodeStack[threadCount].Push(root);
            QuadTreeNode node;

            while (_nodeStack[threadCount].Count > 0)
            {
                node = _nodeStack[threadCount].Pop();

                if (node.IsLeaf)
                {
                    foreach (var polygon in node.Polygons)
                    {
                        _renderList[threadCount][_endOfList[threadCount]] = polygon;
                        _endOfList[threadCount]++;
                    }
                }
                else
                {
                    if (node.Octant1 != null && frustrum.CheckBoundingBox(node.Octant1.BoundingBox) != 0)
                    {
                        _nodeStack[threadCount].Push(node.Octant1);
                    }
                    if (node.Octant2 != null && frustrum.CheckBoundingBox(node.Octant2.BoundingBox) != 0)
                    {
                        _nodeStack[threadCount].Push(node.Octant2);
                    }
                    if (node.Octant3 != null && frustrum.CheckBoundingBox(node.Octant3.BoundingBox) != 0)
                    {
                        _nodeStack[threadCount].Push(node.Octant3);
                    }
                    if (node.Octant4 != null && frustrum.CheckBoundingBox(node.Octant4.BoundingBox) != 0)
                    {
                        _nodeStack[threadCount].Push(node.Octant4);
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
