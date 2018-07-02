using FunAndGamesWithSharpDX.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using DungeonHack.Entities;

namespace DungeonHack.BSP
{
    public class BspTreeBuilder
    {
        private readonly PolygonClassifier _polyClassifier;
        private readonly SplitterSelector _splitterSelector;
        private readonly PolygonSplitter _polygonSplitter;
        private readonly List<BspNode> _bspWorkList;

        public int NumberOfNodesUpdated { get; private set; }

        public BspTreeBuilder(PolygonClassifier polygonClassifier, SplitterSelector splitterSelector, PolygonSplitter polygonSplitter)
        {
            _polyClassifier = polygonClassifier;
            _splitterSelector = splitterSelector;
            _polygonSplitter = polygonSplitter;
            _bspWorkList = new List<BspNode>();
        }

        public BspNode BuildTree(List<Polygon> meshList)
        {
            var bspRootNode = new BspNode
            {
                IsRoot = true
            };

            _bspWorkList.Add(bspRootNode);
            BuildBspTree(bspRootNode, meshList);

            return bspRootNode;
        }


        public void TraverseBspTreeAndPerformAction(BspNode rootNode, Action<Polygon> action)
        {
            while (true)
            {
                if (rootNode.Splitter == null) return;

                TraverseBspTreeAndPerformAction(rootNode.Back, action);

                action.Invoke(rootNode.Splitter);

                NumberOfNodesUpdated++;

                rootNode = rootNode.Front;
            }
        }

        public void TraverseBspTreeAndPerformActionOnNode(BspNode node, Action<BspNode> action)
        {
            while (true)
            {
                if (node.IsLeaf) return;

                TraverseBspTreeAndPerformActionOnNode(node.Back, action);

                action.Invoke(node);

                NumberOfNodesUpdated++;

                node = node.Front;
            }
        }

        private void AddAllNodesToList(BspNode node)
        {
            while (true)
            {
                _bspWorkList.Add(node);

                if (node.IsLeaf) return;

                AddAllNodesToList(node.Back);

                node = node.Front;
            }
        }

        public BspNodeOptomized[] TransformNodesToOptomizedNodes(BspNode rootNode)
        {
            AddAllNodesToList(rootNode);

            return _bspWorkList.Select(x => new BspNodeOptomized()
            {
                Back = x.Back != null ? _bspWorkList.IndexOf(x.Back) : -1,
                BoundingVolume = x.BoundingVolume,
                ConvexPolygonSet = x.ConvexPolygonSet,
                Front = x.Front != null ? _bspWorkList.IndexOf(x.Front) : -1,
                IsLeaf = x.IsLeaf,
                IsRoot = x.IsRoot,
                IsSolid = x.IsSolid,
                Parent = x.Parent != null ? _bspWorkList.IndexOf(x.Parent) : -1,
                Splitter = x.Splitter
            }).ToArray();
        }

        private void BuildBspTree(BspNode currentNode, List<Polygon> meshList)
        {
            while (true)
            {
                var frontList = new List<Polygon>();
                var backList = new List<Polygon>();

                currentNode.Splitter = _splitterSelector.SelectBestSplitter(meshList);

                foreach (var testMesh in meshList)
                {
                    if (testMesh == currentNode.Splitter) continue;

                    switch (_polyClassifier.ClassifyPolygon(currentNode.Splitter, testMesh))
                    {
                        case PolygonClassification.OnPlane:
                        case PolygonClassification.Front:
                            frontList.Insert(0, testMesh);
                            break;
                        case PolygonClassification.Back:
                            backList.Insert(0, testMesh);
                            break;
                        case PolygonClassification.Spanning:
                            //frontList.Insert(0, testMesh);
                            //backList.Insert(0, testMesh);
                            HandleSpanningPolygon(currentNode, meshList, out var frontSplit, out var backSplit, frontList, backList, testMesh);

                            break;
                        default:
                            break;
                    }
                }

                if (!frontList.Any())
                {
                    var leafNode = new BspNode
                    {
                        IsLeaf = true,
                        IsSolid = false,
                        Parent = currentNode
                    };
                    currentNode.Front = leafNode;
                }
                else
                {
                    var newNode = new BspNode
                    {
                        IsLeaf = false,
                        Parent = currentNode
                    };
                    currentNode.Front = newNode;
                    BuildBspTree(newNode, frontList);
                }

                if (!backList.Any())
                {
                    var leafNode = new BspNode
                    {
                        IsLeaf = true,
                        IsSolid = true,
                        Parent = currentNode
                    };
                    currentNode.Back = leafNode;
                }
                else
                {
                    var newNode = new BspNode
                    {
                        IsLeaf = false,
                        Parent = currentNode
                    };
                    currentNode.Back = newNode;
                    currentNode = newNode;
                    meshList = backList;
                    continue;
                }

                break;
            }
        }

        private void HandleSpanningPolygon(BspNode currentNode, IList<Polygon> meshList, out Polygon frontSplit, out Polygon backSplit, IList<Polygon> frontList, List<Polygon> backList, Polygon testMesh)
        {
            _polygonSplitter.Split(testMesh, currentNode.Splitter, out frontSplit, out backSplit);

            if (frontSplit != null)
            {
                frontList.Insert(0, frontSplit);
                meshList.Insert(meshList.IndexOf(testMesh), frontSplit);
            }

            if (backSplit != null)
            {
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
