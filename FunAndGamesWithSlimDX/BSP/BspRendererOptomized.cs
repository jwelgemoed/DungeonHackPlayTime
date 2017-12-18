using FunAndGamesWithSharpDX.Engine;
using FunAndGamesWithSharpDX.Entities;
using SharpDX;
using SharpDX.Direct3D11;
using System;
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
            //DrawBspTreeFrontToBackIterative(frustrum, position);

            for (int i = 0; i < _renderList.Count; i++)
            {
                _meshRenderer.Render(0, frustrum, _renderList[i], ref meshRenderedCount);
            }

            _renderList.Clear();
        }

        private void DrawBspTreeFrontToBackIterative(Frustrum frustrum, Vector3 position)
        {
            Stack<Tuple<BspNodeOptomized, PointClassification>> _stack = 
                new Stack<Tuple<BspNodeOptomized, PointClassification>>();
            BspNodeOptomized node = Nodes[0];
            PointClassification result;
            int nextNode;

            while (_stack.Count > 0 || node == Nodes[0])
            {
                if (!node.IsLeaf)
                {
                    result = _pointClassifier.ClassifyPoint(position,
                                            new Vector3(node.Splitter.VertexData[0].Position.X,
                                                        node.Splitter.VertexData[0].Position.Y,
                                                        node.Splitter.VertexData[0].Position.Z),
                                            node.Splitter.Normal);

                    nextNode = result == PointClassification.Back ? node.Back : node.Front;

                    if (nextNode != -1)
                    {
                        _stack.Push(new Tuple<BspNodeOptomized, PointClassification>(Nodes[nextNode], result));
                        node = Nodes[nextNode];
                    }
                }
                else
                {
                    var nodeItem = _stack.Pop();
                    node = nodeItem.Item1;
                    result = nodeItem.Item2;

                    if (node.IsLeaf)
                    {
                        if (_stack.Count == 0)
                        {
                            break;
                        }

                        nodeItem = _stack.Pop();
                        node = nodeItem.Item1;
                        result = nodeItem.Item2;
                    }

                    if (frustrum.CheckBoundingBox(node.Splitter.BoundingBox) != 0)
                    {
                        _renderList.Add(node.Splitter);
                    }

                    nextNode = result == PointClassification.Back ? node.Front : node.Back;

                    if (nextNode != -1)
                    {
                        _stack.Push(new Tuple<BspNodeOptomized, PointClassification>(Nodes[nextNode], result));
                        node = Nodes[nextNode];
                    }

                }
            }
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

            int nextNodeLeft = result == PointClassification.Back ? node.Back : node.Front;
            int nextNodeRight = result == PointClassification.Back ? node.Front : node.Back;

            if (nextNodeLeft != -1)
            {
                DrawBspTreeFrontToBackRecurse(Nodes[nextNodeLeft], position, frustrum, ref meshRenderedCount);
            }

            //Do frustrum culling for current polygon
            if (frustrum.CheckBoundingBox(node.Splitter.BoundingBox) != 0)
            {
                _renderList.Add(node.Splitter);
            }

            if (nextNodeRight != -1)
            {
                DrawBspTreeFrontToBackRecurse(Nodes[nextNodeRight], position, frustrum, ref meshRenderedCount);
            }
        }
 
    }
}
