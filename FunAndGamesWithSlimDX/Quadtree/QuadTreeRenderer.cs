using DungeonHack.Engine;
using DungeonHack.OcclusionCulling;
using DungeonHack.Renderers;
using FunAndGamesWithSharpDX.Engine;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DungeonHack.QuadTree
{
    public class QuadTreeRenderer
    {
        private PolygonRenderer _renderer;
        private Stack<QuadTreeNode>[] _nodeStack;
        private int _threadCount;
        private Task[] _renderTasks;
        private DepthBuffer _depthBuffer;
        private RenderedItems _renderedItems;

        public int MeshesRendered;

        public QuadTreeRenderer(PolygonRenderer renderer, DepthBuffer depthBuffer,
            RenderedItems renderedItems, int numberOfThreads)
        {
            _threadCount = numberOfThreads;
            _renderedItems = renderedItems;
            _renderer = renderer;
            _renderTasks = new Task[_threadCount];
            _nodeStack = new Stack<QuadTreeNode>[_threadCount];
            _depthBuffer = depthBuffer;

            for (int i = 0; i < _threadCount; i++)
            {
                _nodeStack[i] = new Stack<QuadTreeNode>();
            }
        }

        public void DrawQuadTree(QuadTreeNode node, Frustrum frustrum, Camera camera)
        {
            MeshesRendered = 0;

            _renderer.RenderFrame(camera);

            _depthBuffer.LockShadowBuffer();

            // DrawQuadTreeIterative(0, node, camera, frustrum);

            //_depthBuffer.UnlockShadowBuffer();

            for (int i = 0; i < _threadCount; i++)
            {
                int j = i;
                _renderTasks[i] = new Task(() =>
                {
                    switch (j)
                    {
                        case 0:
                            DrawQuadTreeIterative(0, node.Octant1, camera, frustrum);
                            break;
                        case 1:
                            DrawQuadTreeIterative(1, node.Octant2, camera, frustrum);
                            break;
                        case 2:
                            DrawQuadTreeIterative(2, node.Octant3, camera, frustrum);
                            break;
                        case 3:
                            DrawQuadTreeIterative(3, node.Octant4, camera, frustrum);
                            break;
                        default:
                            DrawQuadTreeIterative(0, node, camera, frustrum);
                            break;
                    }
                });

                _renderTasks[i].Start();
            }

            Task.WaitAll(_renderTasks);

            _depthBuffer.UnlockShadowBuffer();
            _renderer.RenderAll();
        }

        private void DrawQuadTreeIterative(int threadCount, QuadTreeNode root, Camera camera, Frustrum frustrum)
        {
            QuadTreeNode node;
            _nodeStack[threadCount].Push(root);
            int depth = 1;
            _renderedItems.RenderedItemLists[threadCount].Clear();

            while (_nodeStack[threadCount].Count > 0)
            {
                depth--;
                node = _nodeStack[threadCount].Pop();

                if
                   (!node.BoundingBox.ContainsOrIntersectsCamera(camera) &&
                (frustrum.CheckBoundingBox(node.BoundingBox.BoundingBox) == 0
                || node.BoundingBox.DistanceToCamera(camera) >= 2500
                || _depthBuffer.IsBoundingBoxOccluded(node.BoundingBox)))
                {
                    continue;
                }

                if (node.IsLeaf)
                {
                    int polygonsdrawn = 0;

                    foreach (var polygon in node.Polygons)
                    {
                        if (frustrum.CheckBoundingBox(polygon.BoundingBox.BoundingBox) == 0
                           /* || _depthBuffer.IsBoundingBoxOccluded(polygon.BoundingBox)*/)
                        {
                            continue;
                        }

                        _renderer.Render(threadCount, polygon, ref polygonsdrawn);
                        _renderedItems.RenderedItemLists[threadCount].Add(polygon);

                        Interlocked.Increment(ref MeshesRendered);
                    }
                }
                else
                {
                    if (node.Octant1 != null)
                    {
                        _nodeStack[threadCount].Push(node.Octant1);
                        depth++;
                    }
                    if (node.Octant2 != null)
                    {
                        _nodeStack[threadCount].Push(node.Octant2);
                        depth++;
                    }
                    if (node.Octant3 != null)
                    {
                        _nodeStack[threadCount].Push(node.Octant3);
                        depth++;
                    }
                    if (node.Octant4 != null)
                    {
                        _nodeStack[threadCount].Push(node.Octant4);
                        depth++;
                    }
                }
            }

            _renderer.FinalizeRender(threadCount);
        }
     }
}
