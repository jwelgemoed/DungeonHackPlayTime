using DungeonHack.Builders;
using FunAndGamesWithSlimDX.Entities;
using SlimDX;
using SlimDX.Direct3D11;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DungeonHack.BSP
{
    public class BspTreeBuilder
    {
        private SlimDX.Direct3D11.Device _device;
        private FunAndGamesWithSlimDX.DirectX.IShader _shader;
        private PolygonClassifier _polyClassifier;
        private PointClassifier _pointClassifier;

        public int NumberOfNodesUpdated { get; private set; }
        
        public BspTreeBuilder(Device device, FunAndGamesWithSlimDX.DirectX.IShader shader)
        {
            _device = device;
            _shader = shader;
            _polyClassifier = new PolygonClassifier();
            _pointClassifier = new PointClassifier();
        }

        public BspNode BuildTree(List<Mesh> meshList)
        {
            BspNode bspRootNode = new BspNode();
            bspRootNode.IsRoot = true;

            BuildBspTree(bspRootNode, meshList);

            return bspRootNode;
        }

        public void TraverseBspTreeAndPerformAction(BspNode rootNode, Action<Mesh> action)
        {
            if (rootNode.Splitter == null)
                return;

            TraverseBspTreeAndPerformAction(rootNode.Back, action);

            action.Invoke(rootNode.Splitter);

            NumberOfNodesUpdated++;

            TraverseBspTreeAndPerformAction(rootNode.Front, action);
        }

        public void TraverseBspTreeAndPerformActionOnNode(BspNode node, Action<BspNode> action)
        {
            if (node.IsLeaf)
                return;

            TraverseBspTreeAndPerformActionOnNode(node.Back, action);

            action.Invoke(node);

            NumberOfNodesUpdated++;

            TraverseBspTreeAndPerformActionOnNode(node.Front, action);
        }

        private void BuildBspTree(BspNode currentNode, List<Mesh> meshList)
        {
            Mesh frontSplit;
            Mesh backSplit;
            List<Mesh> frontList = new List<Mesh>();
            List<Mesh> backList = new List<Mesh>();
            Vector3 vec1, vec2;

            currentNode.Splitter = SelectBestSplitter(meshList);

            for (int i=0; i<meshList.Count;i++)
            {
                var testMesh = meshList[i];

                if (testMesh == currentNode.Splitter)
                    continue;

                switch (_polyClassifier.ClassifyPolygon(currentNode.Splitter, testMesh))
                {
                    case PolygonClassification.Front:
                        frontList.Add(testMesh);
                        break;
                    case PolygonClassification.Back:
                        backList.Add(testMesh);
                        break;
                    case PolygonClassification.Spanning:
                        frontList.Add(testMesh);
                        backList.Add(testMesh);
                       /* SplitMesh(testMesh, currentNode.Splitter, out frontSplit, out backSplit);

                        if (frontSplit != null)
                        {
                            frontList.Add(frontSplit);
                            meshList.Insert(meshList.IndexOf(testMesh), frontSplit);
                        }

                        if (backSplit != null)
                        {
                            backList.Add(backSplit);
                            meshList.Insert(meshList.IndexOf(testMesh), backSplit);
                        }

                        if (frontSplit != null || backSplit != null)
                        {
                            meshList.Remove(testMesh);
                        }*/
                      
                        break;
                    default:
                        break;
                }
            }

            if (!frontList.Any())
            {
                BspNode leafNode = new BspNode();
                leafNode.IsLeaf = true;
                leafNode.IsSolid = false;
                leafNode.Parent = currentNode;
                leafNode.ConvexPolygonSet = meshList;
                currentNode.Front = leafNode;
            }
            else
            {
                BspNode newNode = new BspNode();
                newNode.IsLeaf = false;
                newNode.Parent = currentNode;
                currentNode.Front = newNode;
                BuildBspTree(newNode, frontList);
            }

            if (!backList.Any())
            {
                BspNode leafNode = new BspNode();
                leafNode.IsLeaf = true;
                leafNode.IsSolid = true;
                leafNode.Parent = currentNode;
                leafNode.ConvexPolygonSet = meshList;
                currentNode.Back = leafNode;
            }
            else
            {
                BspNode newNode = new BspNode();
                newNode.IsLeaf = false;
                newNode.Parent = currentNode;
                currentNode.Back = newNode;
                BuildBspTree(newNode, backList);
            }
        }

        private void SplitMesh(Mesh testMesh, Mesh splitter, out Mesh frontSplit, out Mesh backSplit)
        {
            List<Vertex> frontList = new List<Vertex>();
            List<Vertex> backList = new List<Vertex>();
            Vertex firstModel, pointA, pointB;
            Vector3 planeNormal, intersectPoint;
            Vertex pointOnPlane;
            int frontcounter, backcounter, loop, currentVertex;
            float percent;

            pointOnPlane = splitter.VertexData[0];

            firstModel = testMesh.VertexData[0];

            switch (_pointClassifier.ClassifyPoint(
                    new Vector3(firstModel.Position.X, firstModel.Position.Y, firstModel.Position.Z), 
                    splitter))
            {
                case PointClassification.Front:
                    frontList.Add(firstModel);
                    break;
                case PointClassification.Back:
                    backList.Add(firstModel);
                    break;
                case PointClassification.OnPlane:
                    backList.Add(firstModel);
                    frontList.Add(firstModel);
                    break;
                default:
                    break;
            }

            for (int i=1; i<testMesh.VertexData.Length+1; i++)
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

                planeNormal = splitter.VertexData[0].Normal;

                var pointClassification = _pointClassifier.ClassifyPoint(
                    new Vector3(pointB.Position.X, pointB.Position.Y, pointB.Position.Z), splitter);

                if (pointClassification == PointClassification.OnPlane)
                {
                    backList.Add(testMesh.VertexData[currentVertex]);
                    frontList.Add(testMesh.VertexData[currentVertex]);
                }
                else
                {
                    if (GetIntersect(new Vector3(pointA.Position.X, pointA.Position.Y, pointA.Position.Z),
                                     new Vector3(pointB.Position.X, pointB.Position.Y, pointB.Position.Z),
                                     new Vector3(pointOnPlane.Position.X, pointOnPlane.Position.Y, pointOnPlane.Position.Z),
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

            MeshBuilder meshBuilder = new MeshBuilder(_device, _shader);

            frontSplit = meshBuilder
                        .New()
                        .SetTranslationMatrix(testMesh.TranslationMatrix)
                        .SetRotationMatrix(testMesh.RotationMatrix)
                        .SetScaleMatrix(testMesh.ScaleMatrix)
                        .SetVertexData(frontList.ToArray())
                        .SetTextureIndex(testMesh.TextureIndex)
                        .SetMaterialIndex(testMesh.MaterialIndex)
                        .Build();

            backSplit = meshBuilder
                        .New()
                        .SetTranslationMatrix(testMesh.TranslationMatrix)
                        .SetRotationMatrix(testMesh.RotationMatrix)
                        .SetScaleMatrix(testMesh.ScaleMatrix)
                        .SetVertexData(backList.ToArray())
                        .SetTextureIndex(testMesh.TextureIndex)
                        .SetMaterialIndex(testMesh.MaterialIndex)
                        .Build();
        }

        private bool GetIntersect(Vector3 lineStart, 
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

        private Mesh SelectBestSplitter(IEnumerable<Mesh> meshList)
        {
            long bestScore = 100000000;
            Mesh selectedMesh = meshList.First();

            foreach (var splitter in meshList)
            {
                long score = 0;
                long splits = 0;
                long backfaces = 0;
                long frontfaces = 0;

                foreach (var mesh in meshList)
                {
                    if (mesh == splitter)
                        continue;

                    var polyClassification = _polyClassifier.ClassifyPolygon(splitter, mesh);

                    switch (polyClassification)
                    {
                        case PolygonClassification.Front:
                            frontfaces++;
                            break;
                        case PolygonClassification.Back:
                            backfaces++;
                            break;
                        case PolygonClassification.Spanning:
                            splits++;
                            break;
                    }

                    score = Math.Abs(frontfaces - backfaces) + (splits * 8);

                    if (score < bestScore)
                    {
                        bestScore = score;
                        selectedMesh = splitter;
                    }
                }
            }

            return selectedMesh;
        }
    }
}
