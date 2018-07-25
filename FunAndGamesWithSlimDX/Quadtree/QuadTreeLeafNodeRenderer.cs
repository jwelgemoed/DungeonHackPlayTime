using DungeonHack.Entities;
using DungeonHack.OcclusionCulling;
using DungeonHack.Renderers;
using FunAndGamesWithSharpDX.Engine;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace DungeonHack.QuadTree
{
    public class QuadTreeLeafNodeRenderer
    {
        private PolygonRenderer _renderer;
        private Polygon[][] _renderList;
        private QuadTreeNode[] _nodes;
        private int[] _endOfList;
        private int _threadCount;
        private Task[] _tasks;
        private Stopwatch _stopwatch;
        private DepthBuffer _depthBuffer;

        public QuadTreeLeafNodeRenderer(PolygonRenderer renderer, Camera camera, IEnumerable<QuadTreeNode> leafNodes)
        {
            _nodes = leafNodes.ToArray();
            _threadCount = leafNodes.Count();
            _renderer = renderer;
            _renderList = new Polygon[_threadCount][];
            _endOfList = new int[_threadCount];
            _tasks = new Task[_threadCount];
            _stopwatch = new Stopwatch();
            _depthBuffer = new DepthBuffer(camera, _threadCount);

            for (int i = 0; i < _threadCount; i++)
            {
                _renderList[i] = new Polygon[5000];
            }
        }

        public void DrawQuadTree(Frustrum frustrum, Camera camera, ref int meshRenderedCount)
        {
            for (int i = 0; i < _threadCount; i++)
            {
                int j = i;
                _tasks[i] = new Task(() =>
                {
                    DrawNode(j, _nodes[j], camera, frustrum);

                });

                _tasks[i].Start();
            }

            Task.WaitAll(_tasks);
        }

        private void DrawNode(int threadCount, QuadTreeNode node, Camera camera, Frustrum frustrum)
        {

            if (!node.BoundingBox.ContainsOrIntersectsCamera(camera) &&
                (frustrum.CheckBoundingBox(node.BoundingBox.BoundingBox) == 0
                    || node.BoundingBox.DistanceToCamera(camera) >= 2500)
                    || _depthBuffer.IsBoundingBoxOccluded(node.BoundingBox)
                    )
            {
                return;
            }

            int polygonsdrawn = 0;
            int backfaceculled = 0;

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

                if (draw)
                {
                    polygonsdrawn++;
                    int meshRenderedCount = 0;
                   // _renderer.Render(polygon, ref meshRenderedCount);
                }
                else
                {
                    backfaceculled++;
                }
            }

            _depthBuffer.RasterizeTriangles(threadCount);
        }

    }
     
}
