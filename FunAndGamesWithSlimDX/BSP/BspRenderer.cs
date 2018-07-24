using FunAndGamesWithSharpDX.Engine;
using FunAndGamesWithSharpDX.Entities;
using SharpDX;
using SharpDX.Direct3D11;
using System.Collections.Generic;
using DungeonHack.Entities;
using DungeonHack.Renderers;

namespace DungeonHack.BSP
{
    public class BspRenderer
    {
        private readonly PolygonRenderer _meshRenderer;
        private readonly PointClassifier _pointClassifier;
        private List<Polygon> _renderList;
        private List<BoundingBox> _renderedBoxList;
        private Device _device;

        public int NodesVisited { get; private set; }

        public BspRenderer(Device device, PolygonRenderer meshRenderer, PointClassifier pointClassifier)
        {
            _device = device;
            _meshRenderer = meshRenderer;
            _pointClassifier = pointClassifier;
            _renderList = new List<Polygon>();
            _renderedBoxList = new List<BoundingBox>();
        }
        
        /// <summary>
        /// BSP tree traversal inorder to draw polygons in back to front order (painter's algorithm)
        /// </summary>
        /// <param name="node"></param>
        /// <param name="position"></param>
        public void DrawBspTreeBackToFront(BspNode node, Vector3 position, Frustrum frustrum)
        {
            if (node.IsLeaf)
            {
                return;
            }

            PointClassification result = _pointClassifier.ClassifyPoint(position,
                                            new Vector3(node.Splitter.VertexData[0].Position.X,
                                                        node.Splitter.VertexData[0].Position.Y,
                                                        node.Splitter.VertexData[0].Position.Z),
                                            node.Splitter.Normal);

            NodesVisited++;

            if (result == PointClassification.Front)
            {
                if (node.Back != null)
                    DrawBspTreeBackToFront(node.Back, position, frustrum);

                //  node.Splitter.Render(_frustrum, Renderer.Context, Camera, ref _meshRenderedCount);

                if (node.Front != null)
                    DrawBspTreeBackToFront(node.Front, position, frustrum);
            }
            else
            {
                if (node.Front != null)
                    DrawBspTreeBackToFront(node.Front, position, frustrum);

                //                node.Splitter.Render(_frustrum, Renderer.Context, Camera, ref _meshRenderedCount);

                if (node.Back != null)
                    DrawBspTreeBackToFront(node.Back, position, frustrum);
            }
        }

        public void DrawBspTreeFrontToBack(BspNode node, Vector3 position, Frustrum frustrum, ref int meshRenderedCount, Camera camera)
        {
            DrawBspTreeFrontToBackRecurse(node, position, frustrum, ref meshRenderedCount);

            //var viewProjMatrix = camera.ViewMatrix * camera.ProjectionMatrix;

            //queries

           /* _device.ImmediateContext.OutputMerger.DepthStencilState = new DepthStencilState(_device, new DepthStencilStateDescription()
            {
                
            })
                
                .Description.DepthWriteMask = DepthWriteMask.Zero;*/
                
            for (int i=0; i < _renderList.Count; i++)
            {
                //if (!_renderList[i].OcclusionQuery.IsComplete())

                _meshRenderer.Render(_renderList[i], ref meshRenderedCount);

              //  _renderList[i].OcclusionQuery.End();
            }

           /* for (int i=0; i < _renderList.Count; i++)
            {
                while (!_renderList[i].OcclusionQuery.IsComplete())

                //if (isComplete)
                {
                    int pixelCount = _renderList[i].OcclusionQuery.PixelCount;

                    if (pixelCount > 0)
                    {
                        _meshRenderer.Render(frustrum, _renderList[i], ref meshRenderedCount);
                    }
                }
                /*else
                {
                    _meshRenderer.Render(frustrum, _renderList[i], ref meshRenderedCount);
                }
            }*/

            _renderList.Clear();
        }

        private void DrawBspTreeFrontToBackRecurse(BspNode node, Vector3 position, Frustrum frustrum, ref int meshRenderedCount)
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
                if (node.Back != null)
                {
                    DrawBspTreeFrontToBackRecurse(node.Back, position, frustrum, ref meshRenderedCount);
                }

                //Do frustrum culling for current polygon
                if (frustrum.CheckBoundingBox(node.Splitter.BoundingBox.BoundingBox) != 0)
                {
                    _renderList.Add(node.Splitter);
                }

                if (node.Front != null)
                    DrawBspTreeFrontToBackRecurse(node.Front, position, frustrum, ref meshRenderedCount);
            }
            else
            {
                if (node.Front != null)
                    DrawBspTreeFrontToBackRecurse(node.Front, position, frustrum, ref meshRenderedCount);

                //Do frustrum culling for current polygon
                if (frustrum.CheckBoundingBox(node.Splitter.BoundingBox.BoundingBox) != 0)
                {
                    _renderList.Add(node.Splitter);
                }

                if (node.Back != null)
                    DrawBspTreeFrontToBackRecurse(node.Back, position, frustrum, ref meshRenderedCount);
            }
        }
    }
}
