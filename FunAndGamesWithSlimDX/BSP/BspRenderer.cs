using FunAndGamesWithSlimDX.Engine;
using SlimDX;

namespace DungeonHack.BSP
{
    public class BspRenderer
    {
        private readonly MeshRenderer _meshRenderer;
        private readonly PointClassifier _pointClassifier;

        public int NodesVisited { get; private set; }

        public BspRenderer(MeshRenderer meshRenderer, PointClassifier pointClassifier)
        {
            _meshRenderer = meshRenderer;
            _pointClassifier = pointClassifier;
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

        public void DrawBspTreeFrontToBack(BspNode node, Vector3 position, Frustrum frustrum, ref int meshRenderedCount)
        {
            if (node.IsLeaf)
            {
                return;
            }

            if (node.BoundingVolume.HasValue)
            {
                var BoundingBox = new BoundingBox(node.BoundingVolume.Value.Minimum, node.BoundingVolume.Value.Maximum);

                if (frustrum.CheckBoundingBox(BoundingBox) == 0)
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
                    DrawBspTreeFrontToBack(node.Back, position, frustrum, ref meshRenderedCount);

                _meshRenderer.Render(frustrum, node.Splitter, ref meshRenderedCount);

                if (node.Front != null)
                    DrawBspTreeFrontToBack(node.Front, position, frustrum, ref meshRenderedCount);
            }
            else
            {
                if (node.Front != null)
                    DrawBspTreeFrontToBack(node.Front, position, frustrum, ref meshRenderedCount);

                _meshRenderer.Render(frustrum, node.Splitter, ref meshRenderedCount);

                if (node.Back != null)
                    DrawBspTreeFrontToBack(node.Back, position, frustrum, ref meshRenderedCount);
            }
        }
    }
}
