using DungeonHack.BSP;
using FunAndGamesWithSlimDX.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DungeonHack.Bspv2
{
    public class BspCompiler
    {
        private BspCompilerHelper _compilerHelper;

        public BspCompiler()
        {
            _compilerHelper = new BspCompilerHelper();
        }

        public BspNode BuildTree(List<Mesh> polygonMeshLest)
        {
            BspNode rootNode = new BspNode();
            rootNode.IsRoot = true;

            BuildTree(rootNode, polygonMeshLest);

            return rootNode;
        }

        private void BuildTree(BspNode node, List<Mesh> polygonMeshList)
        {
            if (_compilerHelper.IsConvexSet(polygonMeshList))
            {
                node.ConvexPolygonSet = polygonMeshList;
                node.IsLeaf = true;
            }
            else
            {
                node.PartitionPlane = _compilerHelper.SelectPartitionPlane(polygonMeshList);
                List<Mesh> frontList = new List<Mesh>();
                List<Mesh> backList = new List<Mesh>();

                foreach (var polygonMesh in polygonMeshList)
                {
                    var value = _compilerHelper.ClassifyPolygon(node.PartitionPlane, polygonMesh);

                    if ((value == PolygonClassification.Infront) ||
                       (value == PolygonClassification.Coincident))
                    {
                        frontList.Add(polygonMesh);
                    }
                    else if (value == PolygonClassification.Behind)
                    {
                        backList.Add(polygonMesh);
                    }
                    else if (value == PolygonClassification.Spanning)
                    {
                        //TODO: split polygon
                        frontList.Add(polygonMesh);
                        backList.Add(polygonMesh);
                    }
                }

                BspNode frontChild = new BspNode();
                frontChild.Parent = node;
                node.Front = frontChild;

                BspNode backChild = new BspNode();
                backChild.Parent = node;
                node.Back = backChild;

                BuildTree(node.Front, frontList);
                BuildTree(node.Back, backList);

            }
        }

    }
}
