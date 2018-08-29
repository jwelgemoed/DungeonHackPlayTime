using DungeonHack.Entities;
using FunAndGamesWithSharpDX.DirectX;
using FunAndGamesWithSharpDX.Engine;
using SharpDX;

namespace DungeonHack.Engine
{
    public class InteractiveEngine
    {
        private float _width;
        private float _height;
        private float _aspectRatio;

        public InteractiveEngine()
        {
        }

        public void Initialize(Renderer renderer)
        {
            _width = Renderer.Width;
            _height = Renderer.Height;
            _aspectRatio = renderer.AspectRatio();
        }

        public Polygon GetPickedPolygon(Point sc, Camera camera, RenderedItems renderedItems)
        {
            int numberOfThreads = renderedItems.RenderedItemLists.Length;

            float vX = (+2.0f * sc.X / _width - 1.0f) / camera.ProjectionMatrix.M11;
            float vY = (-2.0f * sc.Y / _height + 1.0f) / camera.ProjectionMatrix.M22;
            
            Matrix v = camera.ViewMatrix;
            Matrix invV = Matrix.Invert(v);

            Polygon pickedPolygon = null;

            float tmin = 100000;

            float[] threadMin = new float[numberOfThreads];
            Polygon[] threadPicked = new Polygon[numberOfThreads];

            for (int i = 0; i < numberOfThreads; i++)
            {
                threadMin[i] = 10000.0f;
                threadPicked[i] = null;

                foreach (var polygon in renderedItems.RenderedItemLists[i])
                {
                    Matrix world = polygon.WorldMatrix;
                    Matrix invWorld = Matrix.Invert(world);

                    Matrix toLocal = Matrix.Multiply(invV, invWorld);

                    Vector3 rayOrigin = new Vector3(0.0f, 0.0f, 0.0f);
                    Vector3 rayDirection = new Vector3(vX, vY, 1.0f);

                    rayOrigin = Vector3.TransformCoordinate(rayOrigin, toLocal);
                    rayDirection = Vector3.TransformNormal(rayDirection, toLocal);
                    rayDirection.Normalize();

                    Ray ray = new Ray(rayOrigin, rayDirection);

                    if (polygon.BoundingBox.BoundingBox.Intersects(ref ray, out tmin))
                    {
                        for (int j = 0; j < polygon.VertexData.Length / 3; j++)
                        {
                            int i0 = polygon.IndexData[j * 3 + 0];
                            int i1 = polygon.IndexData[j * 3 + 1];
                            int i2 = polygon.IndexData[j * 3 + 2];

                            Vector3 v0 = new Vector3(polygon.VertexData[i0].Position.X, polygon.VertexData[i0].Position.Y, polygon.VertexData[i0].Position.Z);
                            Vector3 v1 = new Vector3(polygon.VertexData[i1].Position.X, polygon.VertexData[i1].Position.Y, polygon.VertexData[i1].Position.Z);
                            Vector3 v2 = new Vector3(polygon.VertexData[i2].Position.X, polygon.VertexData[i2].Position.Y, polygon.VertexData[i2].Position.Z);

                            float t = 0.0f;

                            if (ray.Intersects(ref v0, ref v1, ref v2, out t))
                            {
                                if (t <= tmin)
                                {
                                    tmin = t;
                                    threadMin[i] = tmin;
                                    threadPicked[i] = polygon;
                                }
                            }
                        }
                    }
                }
            }

            tmin = 10000.0f;

            for (int i=0; i<numberOfThreads; i++)
            {
                if (threadMin[i] <= tmin)
                {
                    tmin = threadMin[i];
                    pickedPolygon = threadPicked[i];
                }
            }

            return pickedPolygon;
        }
    }
}
