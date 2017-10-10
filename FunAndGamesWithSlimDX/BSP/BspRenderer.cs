using FunAndGamesWithSharpDX.Engine;
using FunAndGamesWithSharpDX.Entities;
using SharpDX;
using System.Collections.Generic;

namespace DungeonHack.BSP
{
    

    public class BspRenderer
    {
        private readonly PolygonRenderer _meshRenderer;
        private readonly PointClassifier _pointClassifier;
        private List<Polygon> _renderList;
        private List<BoundingBox> _renderedBoxList;

        public int NodesVisited { get; private set; }

        public BspRenderer(PolygonRenderer meshRenderer, PointClassifier pointClassifier)
        {
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
            bool breakloop;
            var viewProjMatrix = camera.ViewMatrix * camera.ProjectionMatrix;
            for (int i=0; i < _renderList.Count; i++)
            {
                //breakloop = false;
                //for (int j = 0; j < _renderedBoxList.Count; j++)
                //{
                //    var rblMin = Vector3.Transform(_renderedBoxList[j].Minimum, viewProjMatrix);
                //    var rblMax = Vector3.Transform(_renderedBoxList[j].Maximum, viewProjMatrix);
                //    var rbMin = Vector3.Transform(_renderList[i].BoundingBox.Minimum, viewProjMatrix);
                //    var rbMax = Vector3.Transform(_renderList[i].BoundingBox.Maximum, viewProjMatrix);
                    
                //    if ((rbMin.X >= rblMin.X) && (rbMax.X <= rblMax.X) &&
                //            (rbMin.Y >= rblMin.Y) && (rbMax.Y <= rblMax.Y))
                //    {
                //        breakloop = true;
                //        break;
                //    }
                //}

                //if (breakloop)
                //    continue;

                _renderedBoxList.Add(_renderList[i].BoundingBox);
                _meshRenderer.Render(frustrum, _renderList[i], ref meshRenderedCount);
            }

            _renderList.Clear();
            _renderedBoxList.Clear();
        }

        private void DrawBspTreeFrontToBackRecurse(BspNode node, Vector3 position, Frustrum frustrum, ref int meshRenderedCount)
        {
            if (node.IsLeaf)
            {
                return;
            }

            if (node.BoundingVolume.HasValue)
            {
                //var BoundingBox = new BoundingBox(node.BoundingVolume.Value.Minimum, node.BoundingVolume.Value.Maximum);

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

                _renderList.Add(node.Splitter);
                //_meshRenderer.Render(frustrum, node.Splitter, ref meshRenderedCount);

                if (node.Front != null)
                    DrawBspTreeFrontToBackRecurse(node.Front, position, frustrum, ref meshRenderedCount);
            }
            else
            {
                if (node.Front != null)
                    DrawBspTreeFrontToBackRecurse(node.Front, position, frustrum, ref meshRenderedCount);

                _renderList.Add(node.Splitter);
                //_meshRenderer.Render(frustrum, node.Splitter, ref meshRenderedCount);

                if (node.Back != null)
                    DrawBspTreeFrontToBackRecurse(node.Back, position, frustrum, ref meshRenderedCount);
            }
        }
    }
}
