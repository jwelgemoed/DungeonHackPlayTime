using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FunAndGamesWithSharpDX.DirectX;
using SharpDX.Direct3D11;
using FunAndGamesWithSharpDX.Entities;
using DungeonHack.BSP.LeafBsp;
using SharpDX;
using DungeonHack.Builders;

namespace DungeonHack.BSP
{
    public class PortalSplitter : PolygonSplitter
    {
        private readonly PortalBuilder _portalBuilder;

        public PortalSplitter(PointClassifier pointClassifier, Device device, Shader shader) 
            : base(pointClassifier, device, shader)
        {
            _portalBuilder = new PortalBuilder(device, shader);
        }

        public void Split(Portal testMesh, Entities.Plane plane, out Portal frontSplit, out Portal backSplit)
        {
            Split(testMesh, plane.PointOnPlane, plane.Normal, out frontSplit, out backSplit);
        }

        public void Split(Portal testMesh, Vector3 pointOnPlane, Vector3 planeNormal, out Portal frontSplit, out Portal backSplit)
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

            frontSplit = _portalBuilder
                        .New()
                        .SetTranslationMatrix(testMesh.TranslationMatrix)
                        .SetRotationMatrix(testMesh.RotationMatrix)
                        .SetScaleMatrix(testMesh.ScaleMatrix)
                        .SetVertexData(frontList.ToArray())
                        .SetIndexData(indexListFront)
                        .SetTextureIndex(testMesh.TextureIndex)
                        .SetMaterialIndex(testMesh.MaterialIndex)
                        .Build();

            backSplit = _portalBuilder
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
    }
}
