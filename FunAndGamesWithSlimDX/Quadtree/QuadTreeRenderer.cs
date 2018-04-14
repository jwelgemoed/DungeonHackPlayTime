using DungeonHack.OcclusionCulling;
using FunAndGamesWithSharpDX.Engine;
using FunAndGamesWithSharpDX.Entities;
using SharpDX;
using System.Collections.Generic;
using System.Diagnostics;
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
        private Stopwatch _stopwatch;
        private DepthBuffer _depthBuffer;

        public QuadTreeRenderer(PolygonRenderer renderer, Camera camera)
        {
            _threadCount = 4;
            _renderer = renderer;
            _renderList = new Polygon[_threadCount][];
            _endOfList = new int[_threadCount];
            _tasks = new Task[_threadCount];
            _nodeStack = new Stack<QuadTreeNode>[_threadCount];
            _stopwatch = new Stopwatch();
            _depthBuffer = new DepthBuffer(camera, _threadCount);

            for (int i = 0; i < _threadCount; i++)
            {
                _renderList[i] = new Polygon[5000];
                _nodeStack[i] = new Stack<QuadTreeNode>();
            }
        }

        public void DrawQuadTree(QuadTreeNode node, Frustrum frustrum, Camera camera, ref int meshRenderedCount)
        {
            _stopwatch.Start();

            for (int i = 0; i < _threadCount; i++)
            {
                int j = i;
                _tasks[i] = new Task(() =>
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

                _tasks[i].Start();
            }

            Task.WaitAll(_tasks);
            _stopwatch.Stop();

            long elapsedMilliseconds = _stopwatch.ElapsedMilliseconds;

            _stopwatch.Restart();

            for (int i = 0; i < _threadCount; i++)
            {
                for (int j = 0; j < _endOfList[i]; j++)
                {
                    var polygon = _renderList[i][j];
                    _renderer.Render(frustrum, _renderList[i][j], camera.RenderViewProjectionMatrix, ref meshRenderedCount);
                }
                _endOfList[i] = 0;
            }

            _stopwatch.Stop();
            long elapsed2 = _stopwatch.ElapsedMilliseconds;
        }

        private void DrawQuadTreeIterative(int threadCount, QuadTreeNode root, Camera camera, Frustrum frustrum)
        {
            QuadTreeNode node;
            _nodeStack[threadCount].Push(root);
            int depth = 1;

            while (_nodeStack[threadCount].Count > 0)
            {
                depth--;
                node = _nodeStack[threadCount].Pop();

                if (!node.BoundingBox.ContainsOrIntersectsCamera(camera) &&
                    (frustrum.CheckBoundingBox(node.BoundingBox.BoundingBox) == 0
                        || _depthBuffer.IsBoundingBoxOccluded(node.BoundingBox)
                        || node.BoundingBox.DistanceToCamera(camera) >= 2500))
                {
                    continue;
                }

                if (node.IsLeaf)
                {
                    int polygonsdrawn = 0;
                    int backfaceculled = 0;

                    foreach (var polygon in node.Polygons)
                    {
                        if (ConfigManager.FrustrumCullingEnabled &&
                            frustrum.CheckBoundingBox(polygon.BoundingBox) == 0)
                        {
                            continue;
                        }
                        
                        bool draw = true;

                        if (polygon.WorldVectors.Length == 6)
                        {
                            draw = _depthBuffer.TransformPolygon(new[] { polygon.VertexData[0].Position, polygon.VertexData[1].Position, polygon.VertexData[2].Position }, threadCount);
                            draw &= _depthBuffer.TransformPolygon(new[] { polygon.VertexData[3].Position, polygon.VertexData[4].Position, polygon.VertexData[5].Position }, threadCount);
                        }

                        if (draw)
                        {
                            _renderList[threadCount][_endOfList[threadCount]] = polygon;
                            _endOfList[threadCount]++;
                            polygonsdrawn++;
                        }
                        else
                        {
                            backfaceculled++;
                        }
                    }

                    _depthBuffer.RasterizeTriangles(threadCount);
                }
                else
                {
                    int currentdepth = depth;

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
        }
     }
}
