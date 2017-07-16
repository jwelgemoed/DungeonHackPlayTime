using FunAndGamesWithSlimDX.Entities;
using SlimDX;
using SlimDX.Direct3D11;
using System;
using System.Collections.Generic;
using System.Linq;
using Plane = DungeonHack.Entities.Plane;

namespace DungeonHack.BSP.LeafBsp
{
    public class LeafBspTreeBuilder
    {
        public int NumberOfMeshes { get { return MeshArray.Count - 1; } }
        public int NumberOfNodes { get { return NodeArray.Count - 1; } }
        public int NumberOfLeafs { get { return LeafArray.Count - 1; } }
        public int NumberOfPlanes { get { return PlaneArray.Count - 1; } } 
        public int NumberOfPortals { get; set; }

        private List<Polygon> MeshArray = new List<Polygon>();
        private List<Node> NodeArray = new List<Node>();
        private List<Leaf> LeafArray = new List<Leaf>();
        private List<Plane> PlaneArray = new List<Plane>();
        //public Portal[] PortalArray;
        private List<byte> pvsData = new List<byte>();

        private readonly PolygonClassifier _polygonClassifier;
        private readonly PolygonSplitter _polygonSplitter;
        private readonly SplitterSelector _splitterSelector;
        private readonly BoundingBoxCalculator _boundingBoxCalculator;

        private int _recursionDepth = 0;

        public LeafBspTreeBuilder(Device device, FunAndGamesWithSlimDX.DirectX.IShader shader)
        {
            _polygonClassifier = new PolygonClassifier();
            _polygonSplitter = new PolygonSplitter(new PointClassifier(), device, shader);
            _splitterSelector = new SplitterSelector(_polygonClassifier, 3);
            _boundingBoxCalculator = new BoundingBoxCalculator();

            Node newNode = new Node();
            NodeArray.Add(newNode);
        }

        public void BuildTree(int currentNode, List<Polygon> currentMeshes)
        {
            Stack<LeafStackItem> _leafTreeStack = new Stack<LeafStackItem>();
            _leafTreeStack.Push(new LeafStackItem(currentNode, currentMeshes));

            while (_leafTreeStack.Count > 0)
            {
                var item = _leafTreeStack.Pop();
                var node = item.NumberOfNodes;
                var meshes = item.Meshes;

                if (meshes.Count == 0)
                    continue;

                _recursionDepth++;
                List<Polygon> _frontList = new List<Polygon>();
                List<Polygon> _backList = new List<Polygon>();
                Polygon _frontSplit;
                Polygon _backSplit;
                Vector3 a, b;
                float result;

                if (meshes.All(x => x.HasBeenUsedAsSplitPlane)) // all polygons have already been used as splitters.
                {
                    continue;
                }

                NodeArray[node].Plane = _splitterSelector.SelectBestSplitterPlane(meshes, PlaneArray);

                NodeArray[node].BoundingBox = new BoundingBox();

                for (int i = 0; i < meshes.Count; i++)
                {
                    var mesh = meshes[i];

                    switch (_polygonClassifier.ClassifyPolygon(PlaneArray[NodeArray[node].Plane], mesh))
                    {
                        case PolygonClassification.OnPlane:
                            a = PlaneArray[NodeArray[node].Plane].Normal;
                            b = mesh.Normal;
                            result = Math.Abs((a.X - b.X) + (a.Y - b.Y) + (a.Z - b.Z));
                            if (result < 0.1)
                            {
                                _frontList.Insert(0, mesh);
                            }
                            else
                            {
                                _backList.Insert(0, mesh);
                            }
                            break;
                        case PolygonClassification.Front:
                            _frontList.Insert(0, mesh);
                            break;
                        case PolygonClassification.Back:
                            _backList.Insert(0, mesh);
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

                NodeArray[node].BoundingBox = _boundingBoxCalculator
                                                    .CalculateBoundingBox(NodeArray[node].BoundingBox, _frontList);

                BoundingBox leafBox = NodeArray[node].BoundingBox;

                NodeArray[node].BoundingBox = _boundingBoxCalculator
                                            .CalculateBoundingBox(NodeArray[node].BoundingBox, _backList);

                if (!_frontList.Any(x => !x.HasBeenUsedAsSplitPlane) & _frontList.Count > 0)
                {
                    Leaf newLeaf = new Leaf();
                    newLeaf.StartPolygon = NumberOfMeshes;

                    LeafArray.Add(newLeaf);

                    _frontList.ForEach(x => MeshArray.Add(x));

                    newLeaf.EndPolygon = NumberOfMeshes;
                    newLeaf.BoundingBox = leafBox;
                    NodeArray[node].Front = NumberOfLeafs;
                    NodeArray[node].IsLeaf = true;

                    LeafArray.Add(newLeaf);
                }
                else if (_frontList.Any())
                {
                    NodeArray[node].IsLeaf = false;
                    NodeArray[node].Front = NumberOfNodes+1;

                    Node newNode = new Node();
                    NodeArray.Add(newNode);

                    _leafTreeStack.Push(new LeafStackItem(NumberOfNodes, _frontList));
                }

                if (_backList.Count == 0)
                {
                    NodeArray[node].Back = -1;
                }
                else
                {
                    NodeArray[node].Back = NumberOfNodes+1;

                    Node newNode = new Node();
                    NodeArray.Add(newNode);

                    _leafTreeStack.Push(new LeafStackItem(NumberOfNodes, _backList));
                }
            }
        }


        private void HandleSpanningPolygon(Node currentNode, List<Polygon> meshList, out Polygon frontSplit, out Polygon backSplit, List<Polygon> frontList, List<Polygon> backList, Polygon testMesh)
        {
            _polygonSplitter.Split(testMesh, 
                                        PlaneArray[currentNode.Plane].PointOnPlane, 
                                        PlaneArray[currentNode.Plane].Normal, 
                                        out frontSplit, 
                                        out backSplit);

            if (frontSplit != null)
            {
                frontSplit.HasBeenUsedAsSplitPlane = testMesh.HasBeenUsedAsSplitPlane;
                frontList.Insert(0, frontSplit);
                meshList.Insert(meshList.IndexOf(testMesh), frontSplit);
            }

            if (backSplit != null)
            {
                backSplit.HasBeenUsedAsSplitPlane = testMesh.HasBeenUsedAsSplitPlane;
                backList.Insert(0, backSplit);
                meshList.Insert(meshList.IndexOf(testMesh), backSplit);
            }

            if (frontSplit != null || backSplit != null)
            {
                meshList.Remove(testMesh);
            }
        }
    }
}
