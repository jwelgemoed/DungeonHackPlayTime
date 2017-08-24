using DungeonHack.Builders;
using FunAndGamesWithSlimDX.DirectX;
using FunAndGamesWithSlimDX.Entities;
using SlimDX;
using SlimDX.Direct3D11;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DungeonHack.BSP.LeafBsp
{
    public class PortalGenerator
    {
        private readonly List<Node> _nodeArray;
        private readonly List<Entities.Plane> _planeArray;
        private readonly List<Leaf> _leafArray;
        private readonly PortalBuilder _portalBuilder;
        private readonly PolygonClassifier _polyClassifier;
        private readonly PortalSplitter _splitter;
        private List<Portal> PortalArray;
        private int NumberOfNodes;

        private int NumberOfPortals { get { return PortalArray.Count; } }

        public PortalGenerator(List<Node> nodeArray, List<Entities.Plane> planeArray, List<Leaf> leafArray, Device device, IShader shader)
        {
            _nodeArray = nodeArray;
            NumberOfNodes = _nodeArray.Count - 1;
            _planeArray = planeArray;
            _leafArray = leafArray;
            _portalBuilder = new PortalBuilder(device, shader);
            _polyClassifier = new PolygonClassifier();
            _splitter = new PortalSplitter(new PointClassifier(), device, shader);
        }

        public void BuildPortals()
        {
            int stackPointer = 0;
            PortalStackItem[] nodeStack = new PortalStackItem[(NumberOfNodes+1)];
            int portalIndex = 0;

            nodeStack[stackPointer].Node = 0;
            nodeStack[stackPointer].JumpBackPoint = 0;

        START:

            Portal initialPortal = CalculateInitialPortal(nodeStack[stackPointer].Node);
            List<Portal> portalList = ClipPortal(0, initialPortal);

            for (int i = 0; i > portalList.Count; i++)
            {
                if (portalList[i].NumberOfLeafs != 2)
                {
                    portalList.Remove(portalList[i]);
                }
                else if (IsDuplicatePortal(portalList[i], out portalIndex))
                {
                    portalList.Remove(portalList[i]);
                }
                else
                {                      
                    PortalArray[portalIndex] = portalList[i];
                    if (portalIndex == NumberOfPortals)
                    {
                        for (int j = 0; i < portalList[i].NumberOfLeafs; j++)
                        {
                            int index = portalList[i].LeafOwnerArray[j];
                            _leafArray[index].PortalIndexList[_leafArray[index].NumberOfPortals] = NumberOfPortals;
                            _leafArray[index].NumberOfPortals++;
                        }
                    }
                }
            }

            if (!_nodeArray[nodeStack[stackPointer].Node].IsLeaf)
            {
                nodeStack[stackPointer + 1].Node = _nodeArray[nodeStack[stackPointer].Node].Front;
                nodeStack[stackPointer + 1].JumpBackPoint = 1;
                stackPointer++;
                goto START;
            }

            BACK:
            if (_nodeArray[nodeStack[stackPointer].Node].Back != -1)
            {
                nodeStack[stackPointer + 1].Node = _nodeArray[nodeStack[stackPointer].Node].Back;
                nodeStack[stackPointer + 1].JumpBackPoint = 2;
                stackPointer++;
                goto START;
            }

            END:
            stackPointer--;
            if (stackPointer > -1)
            {
                if (nodeStack[stackPointer].JumpBackPoint == 1)
                {
                    goto BACK;
                }
                else if (nodeStack[stackPointer].JumpBackPoint == 2)
                {
                    goto END;
                }
            }

        }

        public Portal CalculateInitialPortal(int node)
        {
            Vector3 maxP, minP, CB, CP, planeNormal;
            maxP = _nodeArray[node].BoundingBox.Maximum;
            minP = _nodeArray[node].BoundingBox.Minimum;
            planeNormal = _planeArray[_nodeArray[node].Plane].Normal;

            CB = (maxP + minP) / 2;
            float distanceToPlane = Vector3.Dot(_planeArray[_nodeArray[node].Plane].PointOnPlane - CB, planeNormal);
            CP = CB + (planeNormal * distanceToPlane);

            Vector3 startVector = new Vector3(0.0f, 0.0f, 0.0f);

            if (Math.Abs(planeNormal.Y) > Math.Abs(planeNormal.Z))
            {
                if (Math.Abs(planeNormal.Z) < Math.Abs(planeNormal.X))
                {
                    startVector.Z = 1.0f;
                }
                else
                {
                    startVector.X = 1.0f;
                }
            }
            else
            {
                if (Math.Abs(planeNormal.Y) < Math.Abs(planeNormal.X))
                {
                    startVector.Y = 1.0f;
                }
                else
                {
                    startVector.X = 1.0f;
                }
            }

            Vector3 U = Vector3.Normalize(Vector3.Cross(startVector, planeNormal));
            Vector3 V = Vector3.Normalize(Vector3.Cross(U, planeNormal));

            Vector3 boxhalfLength = maxP - CB;
            float length = boxhalfLength.Length();
            U = U * length;
            V = V * length;

            Vector3[] p = new Vector3[4];
            p[0] = CP + U - V;
            p[1] = CP + U + V;
            p[2] = CP - U + V;
            p[3] = CP - U - V;

            _portalBuilder.New();
            _portalBuilder.CreateFromVectorsAndNormal(p, planeNormal);
            return _portalBuilder.Build();
        }

        public List<Portal> ClipPortal(int node, Portal portal)
        {
            List<Portal> frontPortalList = new List<Portal>();
            List<Portal> backPortalList = new List<Portal>();
            List<Portal> portalList = new List<Portal>();
            Portal frontSplit;
            Portal backSplit;

            switch (_polyClassifier.ClassifyPolygon(_planeArray[_nodeArray[node].Plane], (Polygon)portal))
            {
                case PolygonClassification.OnPlane:
                    if (_nodeArray[node].IsLeaf)
                    {
                        portal.LeafOwnerArray[portal.NumberOfLeafs] = _nodeArray[node].Front;
                        portal.NumberOfLeafs++;
                        portal.Next = null;
                        portal.Previous = null;
                        frontPortalList.Add(portal);
                    }
                    else
                    {
                        frontPortalList.AddRange(ClipPortal(_nodeArray[node].Front, portal));
                    }

                    if (!frontPortalList.Any())
                    {
                        return null;
                    }

                    if (_nodeArray[node].Back == -1)
                    {
                        return frontPortalList;
                    }

                    foreach (var fp in frontPortalList)
                    {
                        backPortalList.AddRange(ClipPortal(_nodeArray[node].Back, fp));
                    }

                    portalList.AddRange(backPortalList);

                    return portalList;
                case PolygonClassification.Front:
                    if (!_nodeArray[node].IsLeaf)
                    {
                        portalList.AddRange(ClipPortal(_nodeArray[node].Front, portal));
                        return portalList;
                    }

                    portal.LeafOwnerArray[portal.NumberOfLeafs] = _nodeArray[node].Front;
                    portal.NumberOfLeafs++;
                    portal.Next = null;
                    portal.Previous = null;

                    return new List<Portal> { portal };
                case PolygonClassification.Back:
                    if (_nodeArray[node].Back != -1)
                    {
                        portalList.AddRange(ClipPortal(_nodeArray[node].Back, portal));
                        return portalList;
                    }
                    else
                    {
                        Delete(portal);
                        return null;
                    }
                case PolygonClassification.Spanning:

                    _splitter.Split(portal, _planeArray[_nodeArray[node].Plane].PointOnPlane
                        , _planeArray[_nodeArray[node].Plane].Normal, out frontSplit, out backSplit);
                    Delete(portal);
                    if (!_nodeArray[node].IsLeaf)
                    {
                        frontPortalList.AddRange(ClipPortal(_nodeArray[node].Front, frontSplit));
                    }
                    else
                    {
                        frontSplit.LeafOwnerArray[frontSplit.NumberOfLeafs] = _nodeArray[node].Front;
                        frontSplit.NumberOfLeafs++;
                        frontPortalList.Add(frontSplit);
                    }

                    if (_nodeArray[node].Back != -1)
                    {
                        backPortalList.AddRange(ClipPortal(_nodeArray[node].Back, backSplit));
                    }
                    else
                    {
                        Delete(backSplit);
                    }

                    frontPortalList.AddRange(backPortalList);

                    return frontPortalList;

                default:
                    return null;
            }            
        }

        private void Delete(Portal portal)
        {
            portal.Deleted = true;
        }

        private bool IsDuplicatePortal(Portal x, out int portalIndex)
        {
            throw new NotImplementedException();
        }
    }
}
