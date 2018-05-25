using DungeonHack.BSP.LeafBsp;
using DungeonHack.Builders;
using FunAndGamesWithSharpDX.Entities;
using SharpDX;
using System.Collections.Generic;

namespace DungeonHack.BSP
{
    public class PortalSplitter : PolygonSplitter
    {
        private readonly PortalBuilder _portalBuilder;

        public PortalSplitter(PointClassifier pointClassifier, PolygonBuilder polygonBuilder, PortalBuilder portalBuilder) 
            : base(pointClassifier, polygonBuilder)
        {
            _portalBuilder = portalBuilder;
        }

        public void Split(Portal testMesh, Entities.Plane plane, out Portal frontSplit, out Portal backSplit)
        {
            Split(testMesh, plane.PointOnPlane, plane.Normal, out frontSplit, out backSplit);
        }

        public void Split(Portal testMesh, Vector3 pointOnPlane, Vector3 planeNormal, out Portal frontSplit, out Portal backSplit)
        {
            var frontList = new List<Vertex>();
            var backList = new List<Vertex>();

            var firstVertex = testMesh.VertexData[0];

            switch (PointClassifier.ClassifyPoint(
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
                int currentVertex;
                if (i == testMesh.VertexData.Length)
                {
                    currentVertex = 0;
                }
                else
                {
                    currentVertex = i;
                }

                var pointA = testMesh.VertexData[i - 1];
                var pointB = testMesh.VertexData[currentVertex];

                var pointClassification = PointClassifier.ClassifyPoint(
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
                                     out var intersectPoint,
                                     out var percent))
                    {
                        var deltax = testMesh.VertexData[currentVertex].Texture.X - testMesh.VertexData[i - 1].Texture.X;
                        var deltay = testMesh.VertexData[currentVertex].Texture.Y - testMesh.VertexData[i - 1].Texture.Y;
                        var texx = testMesh.VertexData[i - 1].Texture.X + (deltax * percent);
                        var texy = testMesh.VertexData[i - 1].Texture.Y + (deltay * percent);

                        var copy = new Vertex
                        {
                            Position = new Vector4(intersectPoint.X, intersectPoint.Y, intersectPoint.Z, 1.0f),
                            Texture = new Vector2(texx, texy),
                            Normal = new Vector3(planeNormal.X, planeNormal.Y, planeNormal.Z)
                        };

                        switch (pointClassification)
                        {
                            case PointClassification.Front:
                                backList.Add(copy);
                                frontList.Add(copy);

                                if (currentVertex != 0)
                                {
                                    frontList.Add(testMesh.VertexData[currentVertex]);
                                }

                                break;
                            case PointClassification.Back:
                                frontList.Add(copy);
                                backList.Add(copy);

                                if (currentVertex != 0)
                                {
                                    backList.Add(testMesh.VertexData[currentVertex]);
                                }

                                break;
                        }
                    }
                    else
                    {
                        switch (pointClassification)
                        {
                            case PointClassification.Front:
                                if (currentVertex != 0)
                                {
                                    frontList.Add(testMesh.VertexData[currentVertex]);
                                }

                                break;
                            case PointClassification.Back:
                                if (currentVertex != 0)
                                {
                                    backList.Add(testMesh.VertexData[currentVertex]);
                                }

                                break;
                        }
                    }
                }
            }

            short v0 = 0, v1 = 0, v2 = 0;
            var numberOfFrontIndexes = (frontList.Count - 2) * 3;
            var numberOfBackIndexes = (backList.Count - 2) * 3;
            var indexListFront = new short[numberOfFrontIndexes];
            var indexListBack = new short[numberOfBackIndexes];

            for (var i = 0; i < numberOfFrontIndexes / 3; i++)
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
