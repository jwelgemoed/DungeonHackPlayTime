using DungeonHack.Engine;
using DungeonHack.OcclusionCulling;
using DungeonHack.QuadTree;
using FunAndGamesWithSharpDX.Engine;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace DungeonHack.Quadtree
{
    public class QuadTreeDepthRenderer
    {
        private Stack<QuadTreeNode>[] _nodeStack;
        private int _threadCount;
        private int _threadCountPerThread;
        private Task[] _tasks;
        private Stopwatch _stopwatch;
        private DepthBuffer _depthBuffer;
        private float _maxDistance;

        public QuadTreeDepthRenderer(Camera camera, DepthBuffer depthBuffer)
        {
            _threadCount = 4;
            _threadCountPerThread = 4;
            _tasks = new Task[_threadCount];
            _nodeStack = new Stack<QuadTreeNode>[_threadCount * _threadCountPerThread];
            _stopwatch = new Stopwatch();
            _depthBuffer = depthBuffer;
            _maxDistance = ConfigManager.ScreenFar;
            //_depthBuffer = new DepthBuffer(camera, _threadCount * _threadCountPerThread);

            for (int i = 0; i < (_threadCount * _threadCountPerThread); i++)
            {
                _nodeStack[i] = new Stack<QuadTreeNode>();
            }
        }

        public void DrawDepthQuadTree(QuadTreeNode node, Frustrum frustrum, Camera camera)
        {
            _depthBuffer.ClearBuffer();

            DrawQuadMultiThread(0, node, frustrum, camera);

            _depthBuffer.CopyBufferToShadow();

            //for (int i = 0; i < _threadCount; i++)
            //{
            //    int j = i;
            //    _tasks[i] = new Task(() =>
            //    {
            //        switch (j)
            //        {
            //            case 0:
            //                DrawQuadMultiThread(0, node.Octant1, frustrum, camera);
            //                break;
            //            case 1:
            //                DrawQuadMultiThread(1, node.Octant2, frustrum, camera);
            //                break;
            //            case 2:
            //                DrawQuadMultiThread(2, node.Octant3, frustrum, camera);
            //                break;
            //            case 3:
            //                DrawQuadMultiThread(3, node.Octant4, frustrum, camera);
            //                break;
            //            default:
            //                DrawQuadMultiThread(0, node, frustrum, camera);
            //                break;
            //        }
            //    });

            //    _tasks[i].Start();
            //}

            //Task.WaitAll(_tasks);
        }

        private void DrawQuadMultiThread(int threadNumber, QuadTreeNode node,
                                            Frustrum frustrum, Camera camera)
        {
            Task[] tasks = new Task[_threadCountPerThread];
            int basenumber = _threadCount * threadNumber;

            for (int i = 0; i < _threadCountPerThread; i++)
            {
                int j = i;
                tasks[i] = new Task(() =>
                {
                    switch (j)
                    {
                        case 0:
                            DrawQuadTreeIterative(basenumber, node.Octant1, camera, frustrum);
                            break;
                        case 1:
                            DrawQuadTreeIterative(basenumber + 1, node.Octant2, camera, frustrum);
                            break;
                        case 2:
                            DrawQuadTreeIterative(basenumber + 2, node.Octant3, camera, frustrum);
                            break;
                        case 3:
                            DrawQuadTreeIterative(basenumber + 3, node.Octant4, camera, frustrum);
                            break;
                        default:
                            DrawQuadTreeIterative(basenumber, node, camera, frustrum);
                            break;
                    }
                });

                tasks[i].Start();
            }

            Task.WaitAll(tasks);
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
                    || node.BoundingBox.DistanceToCamera(camera) >= _maxDistance
                    || _depthBuffer.IsBoundingBoxOccluded(node.BoundingBox)))
                {
                    continue;
                }

                if (node.IsLeaf)
                {
                    foreach (var polygon in node.Polygons)
                    {
                        if (frustrum.CheckBoundingBox(polygon.BoundingBox.BoundingBox) == 0)
                        {
                            continue;
                        }

                        bool draw = true;

                        if (polygon.WorldVectors.Length == 6)
                        {
                            draw = _depthBuffer.TransformPolygon(new[] { polygon.VertexData[0].Position, polygon.VertexData[1].Position, polygon.VertexData[2].Position }, threadCount);
                            draw &= _depthBuffer.TransformPolygon(new[] { polygon.VertexData[3].Position, polygon.VertexData[4].Position, polygon.VertexData[5].Position }, threadCount);
                        }
                    }

                    _depthBuffer.RasterizeTriangles(threadCount);
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
        }
    }
}
