using DungeonHack.Entities;
using FunAndGamesWithSharpDX.Engine;
using SharpDX;
using System.Collections.Generic;

namespace DungeonHack.OcclusionCulling
{
    public struct Triangle
    {
        public Vector3[] Vectors;
        public int minX, maxX, minY, maxY;
    }

    public class DepthBuffer
    {
        public int Width { get; set; }
        public int Height { get; set; }

        public int MaxDepth { get; set; }

        private List<Triangle>[] triangles;
        private float[] _depthBuffer;
        private float _nearClipPane;
        private Camera _camera;
        private float l, r, t, b;
        private float termX1, termX2, termY1, termY2, halfWidth, halfHeight;

        public DepthBuffer(Camera camera, int numberOfThreads)
        {
            _nearClipPane = ConfigManager.ScreenNear;
            _camera = camera;
            l = t = 0;
            r = ConfigManager.ScreenWidth;
            b = ConfigManager.ScreenHeight;
            Width = ConfigManager.ScreenWidth / 8;
            Height = ConfigManager.ScreenHeight / 8;
            MaxDepth = 10000;
            _depthBuffer = new float[Width * Height];

            for (int i=0; i<_depthBuffer.Length; i++)
            {
                _depthBuffer[i] = MaxDepth;
            }

            termX1 = 2 / (r - l);
            termX2 = (r + l) / termX1;
            termY1 = 2 / (t - b);
            termY2 = (t + b) / termY1;
            halfWidth = Width / 2;
            halfHeight = Height / 2;
            triangles = new List<Triangle>[numberOfThreads];

            for (int i = 0; i < numberOfThreads; i++)
            {
                triangles[i] = new List<Triangle>();
            }
        }

        public void ClearBuffer()
        {
            for (int i = 0; i < _depthBuffer.Length; i++)
            {
                _depthBuffer[i] = MaxDepth;
            }
        }

        public void SaveBufferToFile()
        {
            //System.Drawing.ImageConverter ic = new System.Drawing.ImageConverter();

            //System.Drawing.Image img = (System.Drawing.Image)ic.ConvertFrom(_depthBuffer);

            //System.Drawing.Bitmap bitmap1 = new System.Drawing.Bitmap(img);

            //bitmap1.Save(@"c:\buffer.bmp");
        }

        public bool IsBoundingBoxOccluded(BoundingBox box)
        {
            var corners = box.GetCorners();
            bool occludedBox = true;

            for (int i=0; i<corners.Length;i++)
            {
                Vector4 vector = new Vector4(corners[i], 1.0f);
                Vector4 camVec = Multiply(_camera.ViewProjectionMatrix, vector);

                float ndcX = camVec.X / camVec.W;
                float ndcY = camVec.Y / camVec.W;

                float rasterVecX = (int) ((ndcX + 1) * halfWidth);/// 2 * Width);
                float rasterVecY = (int) ((1 - ndcY) * halfHeight);/// 2 * Height);

                //if (rasterVecX > Width || rasterVecX < 0)
                //{
                //    occludedBox = false;
                //    break;
                //}

                //if (rasterVecY > Height || rasterVecY < 0)
                //{
                //    occludedBox = false;
                //    break;
                //}

                if (rasterVecX < 0)
                    rasterVecX = 0;

                if (rasterVecX > Width)
                    rasterVecX = Width;

                if (rasterVecY < 0)
                    rasterVecY = 0;

                if (rasterVecY > Height)
                    rasterVecY = Height;

                int bufferLocation = ((int) rasterVecY * Width) + (int) rasterVecX;

                if (bufferLocation >= _depthBuffer.Length)
                {
                    occludedBox = false;
                    break;
                }

                if (camVec.Z < _depthBuffer[bufferLocation])
                {
                    occludedBox = false;
                    break;
                }
            }

            return occludedBox;
        }

        public bool IsBoundingBoxOccluded(AABoundingBox box)
        {
            for (int i = 0; i < 12; i++)
            {
                //Triangle
                Triangle triangle = new Triangle();
                Vector3[] rasterVecs = new Vector3[3];
                int minx, maxx, miny, maxy;
                maxx = maxy = 0;
                miny = Height;
                minx = Width;

                for (int j = 0; j < 3; j++)
                {
                    int index = (i * 3) + j;

                    Vector4 camVec = Multiply(_camera.ViewProjectionMatrix, box.Vectors[box.Indexes[index]]);

                    float ndcX = camVec.X / camVec.W;
                    float ndcY = camVec.Y / camVec.W;

                    rasterVecs[j].X = (int)((ndcX + 1) * halfWidth);/// 2 * Width);
                    rasterVecs[j].Y = (int)((1 - ndcY) * halfHeight);/// 2 * Height);
                    rasterVecs[j].Z = camVec.Z;

                    if (rasterVecs[j].X > maxx)
                    {
                        maxx = (int)rasterVecs[j].X;

                        if (maxx >= Width)
                        {
                            maxx = Width;
                            return false;
                        }
                    }

                    if (rasterVecs[j].X < minx)
                    {
                        minx = (int)rasterVecs[j].X;

                        if (minx < 0)
                        {
                            minx = 0;
                            return false;
                        }
                    }

                    if (rasterVecs[j].Y > maxy)
                    {
                        maxy = (int)rasterVecs[j].Y;

                        if (maxy > Height)
                        {
                            maxy = Height;
                            return false;
                        }
                    }

                    if (rasterVecs[j].Y < miny)
                    {
                        miny = (int)rasterVecs[j].Y;

                        if (miny < 0)
                        {
                            miny = 0;
                            return false;
                        }
                    }
                }

                triangle.Vectors = rasterVecs;
                triangle.minX = minx;
                triangle.maxX = maxx;
                triangle.minY = miny;
                triangle.maxY = maxy;

                Point v0 = new Point((int)triangle.Vectors[0].X, (int)triangle.Vectors[0].Y);
                Point v1 = new Point((int)triangle.Vectors[1].X, (int)triangle.Vectors[1].Y);
                Point v2 = new Point((int)triangle.Vectors[2].X, (int)triangle.Vectors[2].Y);

                int A01 = v0.Y - v1.Y; int B01 = v1.X - v0.X;
                int A12 = v1.Y - v2.Y; int B12 = v2.X - v1.X;
                int A20 = v2.Y - v0.Y; int B20 = v0.X - v2.X;

                Point p = new Point(triangle.minX, triangle.minY);
                int area = Orient2d(v0, v1, v2);
                int w0_row = Orient2d(v1, v2, p);
                int w1_row = Orient2d(v2, v0, p);
                int w2_row = Orient2d(v0, v1, p);

                for (int y = triangle.minY; y < triangle.maxY; y++)
                {
                    int w0 = w0_row;
                    int w1 = w1_row;
                    int w2 = w2_row;

                    for (int x = triangle.minX; x < triangle.maxX; x++)
                    {
                        if ((w0 >= 0 && w1 >= 0 && w2 >= 0) && (area > 0))
                        {
                            //float w0area = (float) w0 / area;
                            //float w1area = (float) w1 / area;
                            //float w2area = (float) w2 / area;
                            // linearly interpolate sample depth
                            //float interZ = triangle.Vectors[0].Z * w0area + triangle.Vectors[1].Z * w1area + triangle.Vectors[2].Z * w2area;
                            float interZ = (triangle.Vectors[0].Z * w0 + triangle.Vectors[1].Z * w1 + triangle.Vectors[2].Z * w2) / area;
                            int bufLocation = y * Width + x;

                            if (interZ < _depthBuffer[bufLocation])
                            {
                                return false;
                            }
                        }

                        w0 += A12;
                        w1 += A20;
                        w2 += A01;
                    }

                    w0_row += B12;
                    w1_row += B20;
                    w2_row += B01;
                }
            }

            return true;
        }

        public bool TransformPolygon(Vector4[] vectors, int threadNumber)
        {
            Vector4[] cameraVectors = new Vector4[vectors.Length];
            Vector3[] rasterVectors = new Vector3[vectors.Length];
            Vector4[] projectedVectors = new Vector4[vectors.Length];
            int minx, maxx, miny, maxy;
            maxx = maxy = 0;
            miny = Height;
            minx = Width;

            for (int i=0; i<vectors.Length; i++)
            {
                cameraVectors[i] = Multiply(_camera.ViewProjectionMatrix, vectors[i]);

                float ndcVectorsX = cameraVectors[i].X / cameraVectors[i].W;
                float ndcVectorsY = cameraVectors[i].Y / cameraVectors[i].W;

                rasterVectors[i].X = ((ndcVectorsX + 1) * halfWidth);/// 2 * Width);
                rasterVectors[i].Y = ((1 - ndcVectorsY) * halfHeight);/// 2 * Height);
                rasterVectors[i].Z = cameraVectors[i].Z;

                if (rasterVectors[i].X > maxx)
                {
                    maxx = (int)rasterVectors[i].X;

                    if (maxx >= Width)
                    {
                        maxx = Width;
                    }
                }

                if (rasterVectors[i].X < minx)
                {
                    minx = (int)rasterVectors[i].X;

                    if (minx < 0)
                    {
                        minx = 0;
                    }
                }

                if (rasterVectors[i].Y > maxy)
                {
                    maxy = (int)rasterVectors[i].Y;

                    if (maxy > Height)
                    {
                        maxy = Height;
                    }
                }

                if (rasterVectors[i].Y < miny)
                {
                    miny = (int)rasterVectors[i].Y;

                    if (miny < 0)
                    {
                        miny = 0;
                    }
                }
            }

            //backface culling
            Vector3 u = rasterVectors[1] - rasterVectors[0];
            Vector3 v = rasterVectors[2] - rasterVectors[0];
            
            if (((u.X * v.Y - u.Y * v.X) < 0) && rasterVectors[0].Z > 100)
            {
                return false;
            }

            Triangle triangle = new Triangle()
            {
                Vectors = rasterVectors,
                minX = minx,
                maxX = maxx,
                minY = miny,
                maxY = maxy
            };

            triangles[threadNumber].Add(triangle);

            return true;
        }

        public void RasterizeTriangles(int threadNumber)
        {
            foreach (var triangle in triangles[threadNumber])
            {
                Point v0 = new Point((int) triangle.Vectors[0].X, (int) triangle.Vectors[0].Y);
                Point v1 = new Point((int) triangle.Vectors[1].X, (int) triangle.Vectors[1].Y);
                Point v2 = new Point((int) triangle.Vectors[2].X, (int) triangle.Vectors[2].Y);

                int A01 = v0.Y - v1.Y; int B01 = v1.X - v0.X;
                int A12 = v1.Y - v2.Y; int B12 = v2.X - v1.X;
                int A20 = v2.Y - v0.Y; int B20 = v0.X - v2.X;

                Point p = new Point(triangle.minX, triangle.minY);
                int area = Orient2d(v0, v1, v2);

                if (area <= 0)
                    continue;

                int w0_row = Orient2d(v1, v2, p);
                int w1_row = Orient2d(v2, v0, p);
                int w2_row = Orient2d(v0, v1, p);
                
                for (int y = triangle.minY; y < triangle.maxY; y++)
                {
                    int w0 = w0_row;
                    int w1 = w1_row;
                    int w2 = w2_row;
                    
                    for (int x = triangle.minX; x < triangle.maxX; x++)
                    {
                        if ((w0 >= 0 && w1 >= 0 && w2 >= 0) && (area > 0))
                        {
                            //float w0area = (float) w0 / area;
                            //float w1area = (float) w1 / area;
                            //float w2area = (float) w2 / area;
                            // linearly interpolate sample depth
                            //float interZ = triangle.Vectors[0].Z * w0area + triangle.Vectors[1].Z * w1area + triangle.Vectors[2].Z * w2area;
                            float interZ = (triangle.Vectors[0].Z * w0 + triangle.Vectors[1].Z * w1 + triangle.Vectors[2].Z * w2) / area;
                            int bufLocation = y * Width + x;

                            if (interZ < _depthBuffer[bufLocation])
                            {
                                _depthBuffer[bufLocation] = interZ;
                            }
                        }

                        w0 += A12;
                        w1 += A20;
                        w2 += A01;
                    }

                    w0_row += B12;
                    w1_row += B20;
                    w2_row += B01;
                }
            };

            triangles[threadNumber].Clear();
        }
    
        private int Orient2d(Point a, Point b, Point c)
        {
            return (b.X - a.X) * (c.Y - a.Y) - (b.Y - a.Y) * (c.X - a.X);
        }
        
        private Vector4 Multiply(Matrix m, Vector4 v)
        {
            Vector4 ret;
            ret.X = v.X * m[0, 0] + v.Y * m[1, 0] + v.Z * m[2, 0] + v.W * m[3, 0];
            ret.Y = v.X * m[0, 1] + v.Y * m[1, 1] + v.Z * m[2, 1] + v.W * m[3, 1];
            ret.Z = v.X * m[0, 2] + v.Y * m[1, 2] + v.Z * m[2, 2] + v.W * m[3, 2];
            ret.W = v.X * m[0, 3] + v.Y * m[1, 3] + v.Z * m[2, 3] + v.W * m[3, 3];
            return ret;
        }
    }
}
