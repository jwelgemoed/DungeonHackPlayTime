using FunAndGamesWithSharpDX.Engine;
using FunAndGamesWithSharpDX.Entities;
using SharpDX;
using SharpDX.Direct3D11;
using System.Collections.Generic;

namespace DungeonHack.BSP
{
    public class BspNodeStackItem
    {
        public BspNodeOptomized Node { get; set; }
        public int JumpBackPoint { get; set; }
    }

    public class BspRendererOptomized
    {
        private readonly PolygonRenderer _meshRenderer;
        private readonly PointClassifier _pointClassifier;
        private List<Polygon> _renderList;
        private List<BoundingBox> _renderedBoxList;
        private Device _device;
        public BspNodeOptomized[] Nodes { get; set; }

        public int NodesVisited { get; private set; }
        private Stack<BspNodeStackItem> _nodeStack;


        public BspRendererOptomized(Device device, PolygonRenderer meshRenderer, PointClassifier pointClassifier, BspNodeOptomized[] nodes)
        {
            _device = device;
            _meshRenderer = meshRenderer;
            _pointClassifier = pointClassifier;
            _renderList = new List<Polygon>();
            _renderedBoxList = new List<BoundingBox>();
            Nodes = nodes;
            _nodeStack = new Stack<BspNodeStackItem>();
        }

        public void DrawBspTreeFrontToBack(Vector3 position, Frustrum frustrum, ref int meshRenderedCount, Camera camera)
        {
            DrawBspTreeFrontToBackRecurse(Nodes[0], position, frustrum, ref meshRenderedCount);

            for (int i = 0; i < _renderList.Count; i++)
            {
                _meshRenderer.Render(frustrum, _renderList[i], ref meshRenderedCount);

            }

            _renderList.Clear();
        }

        private void DrawBspTreeFrontToBackRecurse(BspNodeOptomized node, Vector3 position, Frustrum frustrum, ref int meshRenderedCount)
        {
            if (node.IsLeaf)
            {
                return;
            }

            //Do frustrum culling for boundingvolume of current node.
            if (node.BoundingVolume.HasValue)
            {
                if (frustrum.CheckBoundingBox(node.BoundingVolume.Value) == 0)
                {
                    return;
                }
            }

            PointClassification result = _pointClassifier.ClassifyPoint(position,
                                            new Vector3(node.Splitter.VertexData[0].Position.X,
                                                        node.Splitter.VertexData[0].Position.Y,
                                                        node.Splitter.VertexData[0].Position.Z),
                                            node.Splitter.Normal);

            NodesVisited++;

            if (result == PointClassification.Back)
            {
                if (node.Back != -1)
                {
                    DrawBspTreeFrontToBackRecurse(Nodes[node.Back], position, frustrum, ref meshRenderedCount);
                }

                //Do frustrum culling for current polygon
                if (frustrum.CheckBoundingBox(node.Splitter.BoundingBox) != 0)
                {
                    _renderList.Add(node.Splitter);
                }

                if (node.Front != -1)
                    DrawBspTreeFrontToBackRecurse(Nodes[node.Front], position, frustrum, ref meshRenderedCount);
            }
            else
            {
                if (node.Front != -1)
                    DrawBspTreeFrontToBackRecurse(Nodes[node.Front], position, frustrum, ref meshRenderedCount);

                //Do frustrum culling for current polygon
                if (frustrum.CheckBoundingBox(node.Splitter.BoundingBox) != 0)
                {
                    _renderList.Add(node.Splitter);
                }

                if (node.Back != -1)
                    DrawBspTreeFrontToBackRecurse(Nodes[node.Back], position, frustrum, ref meshRenderedCount);
            }
        }
 
    }
}
