using FunAndGamesWithSharpDX.Entities;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using DungeonHack.Entities;

namespace DungeonHack.BSP.LeafBsp
{
    public class LeafBspTreeBuilder
    {
        private readonly PolygonClassifier _polygonClassifier;
        private readonly PolygonSplitter _polygonSplitter;
        private readonly SplitterSelector _splitterSelector;
        private readonly BoundingBoxCalculator _boundingBoxCalculator;
        private readonly LeafBspMasterData _masterData;

        private int _recursionDepth = 0;

        public LeafBspTreeBuilder(PolygonClassifier polygonClassifier, PolygonSplitter polygonSplitter, 
            SplitterSelector splitterSelector, BoundingBoxCalculator boundingBoxCalculator, LeafBspMasterData masterData)
        {
            _polygonClassifier = polygonClassifier;
            _polygonSplitter = polygonSplitter;
            _splitterSelector = splitterSelector;
            _boundingBoxCalculator = boundingBoxCalculator;
            _masterData = masterData;

            Node newNode = new Node();
            _masterData.NodeArray.Add(newNode);
        }

        public LeafBspMasterData BuildTree(int currentNode, List<Polygon> currentMeshes)
        {
            Stack<LeafStackItem> leafTreeStack = new Stack<LeafStackItem>();
            leafTreeStack.Push(new LeafStackItem(currentNode, currentMeshes));

            while (leafTreeStack.Count > 0)
            {
                var item = leafTreeStack.Pop();
                var node = item.NumberOfNodes - 1;
                if (node < 0)
                {
                    node = 0;
                }
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

                _masterData.NodeArray[node].Plane = _splitterSelector.SelectBestSplitterPlane(meshes, _masterData.PlaneArray);

                _masterData.NodeArray[node].BoundingBox = new BoundingBox();

                foreach (var mesh in meshes)
                {
                    switch (_polygonClassifier.ClassifyPolygon(_masterData.PlaneArray[_masterData.NodeArray[node].Plane], mesh))
                    {
                        case PolygonClassification.OnPlane:
                            a = _masterData.PlaneArray[_masterData.NodeArray[node].Plane].Normal;
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
                            HandleSpanningPolygon(_masterData.NodeArray[node],
                                meshes,
                                out _frontSplit,
                                out _backSplit,
                                _frontList,
                                _backList,
                                mesh);
                            break;
                    }
                }

                _masterData.NodeArray[node].BoundingBox = _boundingBoxCalculator
                                                    .CalculateBoundingBox(_masterData.NodeArray[node].BoundingBox, _frontList);

                BoundingBox leafBox = _masterData.NodeArray[node].BoundingBox;

                _masterData.NodeArray[node].BoundingBox = _boundingBoxCalculator
                                            .CalculateBoundingBox(_masterData.NodeArray[node].BoundingBox, _backList);

                if (_frontList.All(x => x.HasBeenUsedAsSplitPlane) & _frontList.Count > 0)
                {
                    Leaf newLeaf = new Leaf
                    {
                        StartPolygon = _masterData.NumberOfPolygons - 1
                    };

                    if (newLeaf.StartPolygon < 0)
                        newLeaf.StartPolygon = 0;

                    _masterData.LeafArray.Add(newLeaf);

                    _frontList.ForEach(x => _masterData.PolygonArray.Add(x));

                    newLeaf.EndPolygon = _masterData.NumberOfPolygons - 1;
                    if (newLeaf.EndPolygon < 0)
                        newLeaf.EndPolygon = 0;

                    newLeaf.BoundingBox = leafBox;
                    _masterData.NodeArray[node].Front = _masterData.NumberOfLeaves-1;
                    _masterData.NodeArray[node].IsLeaf = true;

                    _masterData.LeafArray.Add(newLeaf);
                }
                else if (_frontList.Any())
                {
                    _masterData.NodeArray[node].IsLeaf = false;
                    _masterData.NodeArray[node].Front = _masterData.NumberOfNodes;

                    Node newNode = new Node();
                    _masterData.NodeArray.Add(newNode);

                    leafTreeStack.Push(new LeafStackItem(_masterData.NumberOfNodes, _frontList));
                }

                if (_backList.Count == 0)
                {
                    _masterData.NodeArray[node].Back = -1;
                }
                else
                {
                    _masterData.NodeArray[node].Back = _masterData.NumberOfNodes;

                    Node newNode = new Node();
                    _masterData.NodeArray.Add(newNode);

                    leafTreeStack.Push(new LeafStackItem(_masterData.NumberOfNodes, _backList));
                }
            }

            return _masterData;
        }


        private void HandleSpanningPolygon(Node currentNode, List<Polygon> meshList, out Polygon frontSplit, out Polygon backSplit, List<Polygon> frontList, List<Polygon> backList, Polygon testMesh)
        {
            _polygonSplitter.Split(testMesh, 
                                        _masterData.PlaneArray[currentNode.Plane].PointOnPlane, 
                                        _masterData.PlaneArray[currentNode.Plane].Normal, 
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
