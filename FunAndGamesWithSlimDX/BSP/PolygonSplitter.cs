using DungeonHack.Builders;
using FunAndGamesWithSharpDX.DirectX;
using FunAndGamesWithSharpDX.Entities;
using SharpDX;
using SharpDX.Direct3D11;
using System;
using System.Collections.Generic;

namespace DungeonHack.BSP
{
    public class PolygonSplitter
    {
        protected readonly PointClassifier _pointClassifier;
        private readonly PolygonBuilder _polygonBuilder;
        private readonly Device _device;
        private readonly Shader _shader;

        public PolygonSplitter(PointClassifier pointClassifier, Device device, Shader shader)
        {
            _pointClassifier = pointClassifier;
            _device = device;
            _shader = shader;
            _polygonBuilder = new PolygonBuilder(device, shader);
        }

        public void Split(Polygon testMesh, Polygon plane, out Polygon frontSplit, out Polygon backSplit)
        {
            Vector3 pointOnPlane = new Vector3(plane.VertexData[0].Position.X, 
                                                plane.VertexData[0].Position.Y, 
                                                plane.VertexData[0].Position.Z);

            Split(testMesh, pointOnPlane, plane.VertexData[0].Normal, out frontSplit, out backSplit);
        }

        public void Split(Polygon testMesh, Vector3 pointOnPlane, Vector3 planeNormal, out Polygon frontSplit, out Polygon backSplit)
        {
            List<Vertex> frontList = new List<Vertex>();
            List<Vertex> backList = new List<Vertex>();
            Vertex firstVertex, pointA, pointB;
            Vector3 intersectPoint;
            int currentVertex;
            float percent;

            firstVertex = testMesh.VertexData[0];

            switch (_pointClassifier.ClassifyPoint(
                   new Vector3(firstVertex.Position.X, firstVertex.Position.Y, firstVertex.Position.Z),
                   pointOnPlane,
                   planeNormal))
            {
                case PointClassification.Front:
                    frontList.Add(firstVertex);
                    break;
                case PointClassification.Back:
                    backList.Add(firstVertex);
                    break;
                case PointClassification.OnPlane:
                    backList.Add(firstVertex);
                    frontList.Add(firstVertex);
                    break;
                default:
                    break;
            }

            for (int i = 1; i < testMesh.VertexData.Length + 1; i++)
            {
                if (i == testMesh.VertexData.Length)
                {
                    currentVertex = 0;
                }
                else
                {
                    currentVertex = i;
                }

                pointA = testMesh.VertexData[i - 1];
                pointB = testMesh.VertexData[currentVertex];

                var pointClassification = _pointClassifier.ClassifyPoint(
                    new Vector3(pointB.Position.X, pointB.Position.Y, pointB.Position.Z), pointOnPlane, planeNormal);

                if (pointClassification == PointClassification.OnPlane)
                {
                    backList.Add(testMesh.VertexData[currentVertex]);
                    frontList.Add(testMesh.VertexData[currentVertex]);
                }
                else
                {
                    if (GetIntersect(new Vector3(pointA.Position.X, pointA.Position.Y, pointA.Position.Z),
                                     new Vector3(pointB.Position.X, pointB.Position.Y, pointB.Position.Z),
                                     pointOnPlane,
                                     planeNormal,
                                     out intersectPoint,
                                     out percent))
                    {
                        float deltax, deltay, texx, texy;
                        deltax = testMesh.VertexData[currentVertex].Texture.X - testMesh.VertexData[i - 1].Texture.X;
                        deltay = testMesh.VertexData[currentVertex].Texture.Y - testMesh.VertexData[i - 1].Texture.Y;
                        texx = testMesh.VertexData[i - 1].Texture.X + (deltax * percent);
                        texy = testMesh.VertexData[i - 1].Texture.Y + (deltay * percent);
                        Vertex copy = new Vertex();
                        copy.Position = new Vector4(intersectPoint.X, intersectPoint.Y, intersectPoint.Z, 1.0f);
                        copy.Texture = new Vector2(texx, texy);
                        copy.Normal = new Vector3(planeNormal.X, planeNormal.Y, planeNormal.Z);

                        if (pointClassification == PointClassification.Front)
                        {
                            backList.Add(copy);
                            frontList.Add(copy);

                            if (currentVertex != 0)
                            {
                                frontList.Add(testMesh.VertexData[currentVertex]);
                            }
                        }
                        else if (pointClassification == PointClassification.Back)
                        {
                            frontList.Add(copy);
                            backList.Add(copy);

                            if (currentVertex != 0)
                            {
                                backList.Add(testMesh.VertexData[currentVertex]);
                            }
                        }
                    }
                    else
                    {
                        if (pointClassification == PointClassification.Front)
                        {
                            if (currentVertex != 0)
                            {
                                frontList.Add(testMesh.VertexData[currentVertex]);
                            }
                        }
                        else if (pointClassification == PointClassification.Back)
                        {
                            if (currentVertex != 0)
                            {
                                backList.Add(testMesh.VertexData[currentVertex]);
                            }
                        }
                    }
                }
            }

            short v0 = 0, v1 = 0, v2 = 0;
            int numberOfFrontIndexes = (frontList.Count - 2) * 3;
            int numberOfBackIndexes = (backList.Count - 2) * 3;
            short[] indexListFront = new short[numberOfFrontIndexes];
            short[] indexListBack = new short[numberOfBackIndexes];

            for (int i = 0; i < numberOfFrontIndexes / 3; i++)
            {
                if (i == 0)
                {
                    v0 = 0;
                    v1 = 1;
                    v2 = 2;
                }
                else
                {
                    v1 = v2;
                    v2++;
                }
                indexListFront[i * 3] = v0;
                indexListFront[(i * 3) + 1] = v1;
                indexListFront[(i * 3) + 2] = v2;
            }

            for (int i = 0; i < numberOfBackIndexes / 3; i++)
            {
                if (i == 0)
                {
                    v0 = 0;
                    v1 = 1;
                    v2 = 2;
                }
                else
                {
                    v1 = v2;
                    v2++;
                }
                indexListBack[i * 3] = v0;
                indexListBack[(i * 3) + 1] = v1;
                indexListBack[(i * 3) + 2] = v2;
            }

            frontSplit = _polygonBuilder
                        .New()
                        .SetTranslationMatrix(testMesh.TranslationMatrix)
                        .SetRotationMatrix(testMesh.RotationMatrix)
                        .SetScaleMatrix(testMesh.ScaleMatrix)
                        .SetVertexData(frontList.ToArray())
                        .SetIndexData(indexListFront)
                        .SetTextureIndex(testMesh.TextureIndex)
                        .SetMaterialIndex(testMesh.MaterialIndex)
                        .Build();

            backSplit = _polygonBuilder
                        .New()
                        .SetTranslationMatrix(testMesh.TranslationMatrix)
                        .SetRotationMatrix(testMesh.RotationMatrix)
                        .SetScaleMatrix(testMesh.ScaleMatrix)
                        .SetVertexData(backList.ToArray())
                        .SetIndexData(indexListBack)
                        .SetTextureIndex(testMesh.TextureIndex)
                        .SetMaterialIndex(testMesh.MaterialIndex)
                        .Build();
        }

        protected bool GetIntersect(Vector3 lineStart,
                                    Vector3 lineEnd,
                                    Vector3 vertex,
                                    Vector3 normal,
                                    out Vector3 intersection,
                                    out float percentage)
        {
            intersection = default(Vector3);
            percentage = 0;
            Vector3 direction, l1;
            float linelength, distFromPlane;

            direction = lineEnd - lineStart;

            linelength = Vector3.Dot(direction, normal);

            if (Math.Abs(linelength) < 0.0001)
            {
                return false;
            }

            l1 = vertex - lineStart;

            distFromPlane = Vector3.Dot(l1, normal);
            percentage = distFromPlane / linelength;

            if ((percentage < 0.0f) || (percentage > 1.0f))
            {
                return false;
            }

            intersection = lineStart + (direction * percentage);

            return true;
        }
    }
}
