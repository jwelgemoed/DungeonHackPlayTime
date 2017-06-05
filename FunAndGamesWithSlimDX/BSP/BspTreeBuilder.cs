using FunAndGamesWithSlimDX.Entities;
using SlimDX.Direct3D11;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DungeonHack.BSP
{
    public class BspTreeBuilder
    {
        private readonly PolygonClassifier _polyClassifier;
        private readonly SplitterSelector _splitterSelector;
        private readonly PolygonSplitter _polygonSplitter;

        public int NumberOfNodesUpdated { get; private set; }
        
        public BspTreeBuilder(Device device, FunAndGamesWithSlimDX.DirectX.IShader shader)
        {
            _polyClassifier = new PolygonClassifier();
            _splitterSelector = new SplitterSelector(_polyClassifier, 8);
            _polygonSplitter = new PolygonSplitter(new PointClassifier(), device, shader);
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

            currentNode.Splitter = _splitterSelector.SelectBestSplitter(meshList);

            for (int i=0; i<meshList.Count;i++)
            {
                var testMesh = meshList[i];

                if (testMesh == currentNode.Splitter)
                    continue;

                switch (_polyClassifier.ClassifyPolygon(currentNode.Splitter, testMesh))
                {
                    case PolygonClassification.OnPlane:
                    case PolygonClassification.Front:
                        frontList.Add(testMesh);
                        break;
                    case PolygonClassification.Back:
                        backList.Add(testMesh);
                        break;
                    case PolygonClassification.Spanning:
                        HandleSpanningPolygon(currentNode, 
                                                meshList, 
                                                out frontSplit, 
                                                out backSplit, 
                                                frontList, 
                                                backList, 
                                                testMesh);

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

        private void HandleSpanningPolygon(BspNode currentNode, List<Mesh> meshList, out Mesh frontSplit, out Mesh backSplit, List<Mesh> frontList, List<Mesh> backList, Mesh testMesh)
        {
            _polygonSplitter.SplitMesh(testMesh, currentNode.Splitter, out frontSplit, out backSplit);

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
