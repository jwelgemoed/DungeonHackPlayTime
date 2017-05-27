using FunAndGamesWithSlimDX.Entities;
using SlimDX;
using SlimDX.Direct3D11;
using System;
using System.Collections.Generic;
using Plane = DungeonHack.Entities.Plane;

namespace DungeonHack.BSP.LeafBsp
{
    public class LeafBspTreeBuilder
    {
        public int NumberOfPolygons { get; set; }
        public int NumberOfNodes { get; set; }
        public int NumberOfLeafs { get; set; }
        public int NumberOfPlanes { get; set; } 
        public int NumberOfPortals { get; set; }

        private List<Mesh> MeshArray = new List<Mesh>();
        private List<Node> NodeArray = new List<Node>();
        private List<Leaf> LeafArray = new List<Leaf>();
        private List<Plane> PlaneArray;
        //public Portal[] PortalArray;
        private List<byte> pvsData = new List<byte>();

        private readonly PolygonClassifier _polygonClassifier;
        private readonly PolygonSplitter _polygonSplitter;
        private readonly SplitterSelector _splitterSelector;

        public LeafBspTreeBuilder(Device device, FunAndGamesWithSlimDX.DirectX.IShader shader)
        {
            _polygonClassifier = new PolygonClassifier();
            _polygonSplitter = new PolygonSplitter(new PointClassifier(), device, shader);
            _splitterSelector = new SplitterSelector(_polygonClassifier);
        }

        public void BuildTree(int node, List<Mesh> meshes)
        {
            List<Mesh> _meshTest;
            List<Mesh> _frontList = new List<Mesh>();
            List<Mesh> _backList = new List<Mesh>();
            Mesh _frontSplit;
            Mesh _backSplit;
            Vector3 vec1, vec2;
            Vector3 a, b;
            float result;

            NodeArray[node].Plane = _splitterSelector.SelectBestSplitterPlane(meshes, PlaneArray);

            NodeArray[node].BoundingBox = new BoundingBox(
                                        new Vector3(-40000, -40000, -40000),
                                        new Vector3(40000, 40000, 40000));

            foreach (var mesh in meshes)
            {
                switch (_polygonClassifier.ClassifyPolygon(PlaneArray[NodeArray[node].Plane], mesh))
                {
                    case PolygonClassification.OnPlane:
                        a = PlaneArray[NodeArray[node].Plane].Normal;
                        b = mesh.Normal;
                        result = Math.Abs((a.X - b.X) + (a.Y - b.Y) + (a.Z - b.Z));
                        if (result < 0.1)
                        {
                            _frontList.Add(mesh);
                        }
                        else
                        {
                            _backList.Add(mesh);
                        }
                        break;
                    case PolygonClassification.Front:
                        _frontList.Add(mesh);
                        break;
                    case PolygonClassification.Back:
                        _backList.Add(mesh);
                        break;
                    case PolygonClassification.Spanning:
                        HandleSpanningPolygon(NodeArray[node], 
                                                meshes, 
                                                out _frontSplit, 
                                                out _backSplit, 
                                                _frontList, 
                                                _backList, 
                                                mesh);
                        break;
                    default:
                        break;
                }
            }
           
        }


        private void HandleSpanningPolygon(Node currentNode, List<Mesh> meshList, out Mesh frontSplit, out Mesh backSplit, List<Mesh> frontList, List<Mesh> backList, Mesh testMesh)
        {
            _polygonSplitter.SplitMesh(testMesh, 
                                        PlaneArray[currentNode.Plane].PointOnPlane, 
                                        PlaneArray[currentNode.Plane].Normal, 
                                        out frontSplit, 
                                        out backSplit);

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
            }
        }
    }
}
