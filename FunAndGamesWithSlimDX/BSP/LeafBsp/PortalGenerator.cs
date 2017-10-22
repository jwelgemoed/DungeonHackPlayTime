﻿using DungeonHack.Builders;
using FunAndGamesWithSharpDX.DirectX;
using FunAndGamesWithSharpDX.Entities;
using SharpDX;
using SharpDX.Direct3D11;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DungeonHack.BSP.LeafBsp
{
    public class PortalGenerator
    {
        private readonly LeafBspMasterData _masterData;
        private readonly PortalBuilder _portalBuilder;
        private readonly PolygonClassifier _polyClassifier;
        private readonly PortalSplitter _splitter;

        public PortalGenerator(LeafBspMasterData masterData, Device device, Shader shader)
        {
            _masterData = masterData;
            _portalBuilder = new PortalBuilder(device, shader);
            _polyClassifier = new PolygonClassifier();
            _splitter = new PortalSplitter(new PointClassifier(), device, shader);
        }

        public void BuildPortals()
        {
            int stackPointer = 0;
            PortalStackItem[] nodeStack = new PortalStackItem[(_masterData.NumberOfNodes+1)];
            int portalIndex = 0;

            nodeStack[stackPointer].Node = 0;
            nodeStack[stackPointer].JumpBackPoint = 0;

        START:

            Portal initialPortal = CalculateInitialPortal(nodeStack[stackPointer].Node);
            List<Portal> portalList = ClipPortal(0, initialPortal);

            for (int i = 0; i < portalList.Count; i++)
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
                    _masterData.PortalArray[portalIndex] = portalList[i];
                    if (portalIndex == _masterData.NumberOfPortals)
                    {
                        for (int j = 0; i < portalList[i].NumberOfLeafs; j++)
                        {
                            int index = portalList[i].LeafOwnerArray[j];
                            _masterData.LeafArray[index].PortalIndexList[_masterData.LeafArray[index].NumberOfPortals] = _masterData.NumberOfPortals;
                            _masterData.LeafArray[index].NumberOfPortals++;
                        }
                    }
                }
            }

            if (!_masterData.NodeArray[nodeStack[stackPointer].Node].IsLeaf)
            {
                nodeStack[stackPointer + 1].Node = _masterData.NodeArray[nodeStack[stackPointer].Node].Front;
                nodeStack[stackPointer + 1].JumpBackPoint = 1;
                stackPointer++;
                goto START;
            }

            BACK:
            if (_masterData.NodeArray[nodeStack[stackPointer].Node].Back != -1)
            {
                nodeStack[stackPointer + 1].Node = _masterData.NodeArray[nodeStack[stackPointer].Node].Back;
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
            maxP = _masterData.NodeArray[node].BoundingBox.Maximum;
            minP = _masterData.NodeArray[node].BoundingBox.Minimum;
            planeNormal = _masterData.PlaneArray[_masterData.NodeArray[node].Plane].Normal;

            CB = (maxP + minP) / 2;
            float distanceToPlane = Vector3.Dot(_masterData.PlaneArray[_masterData.NodeArray[node].Plane].PointOnPlane - CB, planeNormal);
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

            switch (_polyClassifier.ClassifyPolygon(_masterData.PlaneArray[_masterData.NodeArray[node].Plane], (Polygon)portal))
            {
                case PolygonClassification.OnPlane:
                    if (_masterData.NodeArray[node].IsLeaf)
                    {
                        int index = portal.NumberOfLeafs - 1;
                        if (index < 0)
                            index = 0;
                        if (index > 1)
                            index = 1;
                        portal.LeafOwnerArray[index] = _masterData.NodeArray[node].Front;
                        portal.NumberOfLeafs++;
                        portal.Next = null;
                        portal.Previous = null;
                        frontPortalList.Add(portal);
                    }
                    else
                    {
                        frontPortalList.AddRange(ClipPortal(_masterData.NodeArray[node].Front, portal));
                    }

                    if (!frontPortalList.Any())
                    {
                        return new List<Portal>();
                    }

                    if (_masterData.NodeArray[node].Back == -1)
                    {
                        return frontPortalList;
                    }

                    foreach (var fp in frontPortalList)
                    {
                        backPortalList.AddRange(ClipPortal(_masterData.NodeArray[node].Back, fp));
                    }

                    portalList.AddRange(backPortalList);

                    return portalList;
                case PolygonClassification.Front:
                    if (!_masterData.NodeArray[node].IsLeaf)
                    {
                        portalList.AddRange(ClipPortal(_masterData.NodeArray[node].Front, portal));
                        return portalList;
                    }

                    portal.LeafOwnerArray[portal.NumberOfLeafs] = _masterData.NodeArray[node].Front;
                    portal.NumberOfLeafs++;
                    portal.Next = null;
                    portal.Previous = null;

                    return new List<Portal> { portal };
                case PolygonClassification.Back:
                    if (_masterData.NodeArray[node].Back != -1)
                    {
                        portalList.AddRange(ClipPortal(_masterData.NodeArray[node].Back, portal));
                        return portalList;
                    }
                    else
                    {
                        Delete(portal);
                        return new List<Portal>();
                    }
                case PolygonClassification.Spanning:

                    _splitter.Split(portal, _masterData.PlaneArray[_masterData.NodeArray[node].Plane].PointOnPlane
                        , _masterData.PlaneArray[_masterData.NodeArray[node].Plane].Normal, out frontSplit, out backSplit);
                    Delete(portal);
                    if (!_masterData.NodeArray[node].IsLeaf)
                    {
                        frontPortalList.AddRange(ClipPortal(_masterData.NodeArray[node].Front, frontSplit));
                    }
                    else
                    {
                        frontSplit.LeafOwnerArray[frontSplit.NumberOfLeafs] = _masterData.NodeArray[node].Front;
                        frontSplit.NumberOfLeafs++;
                        frontPortalList.Add(frontSplit);
                    }

                    if (_masterData.NodeArray[node].Back != -1)
                    {
                        backPortalList.AddRange(ClipPortal(_masterData.NodeArray[node].Back, backSplit));
                    }
                    else
                    {
                        Delete(backSplit);
                    }

                    frontPortalList.AddRange(backPortalList);

                    return frontPortalList;

                default:
                    return new List<Portal>();
            }            
        }

        private void Delete(Portal portal)
        {
            portal.Deleted = true;
        }

        private bool IsDuplicatePortal(Portal checkPortal, out int portalIndex)
        {
            int checkPortalLeaf1 = checkPortal.LeafOwnerArray[0];
            int checkPortalLeaf2 = checkPortal.LeafOwnerArray[1];
            int PALeaf1 = 0;
            int PALeaf2 = 0;

            for (int i=0; i<_masterData.NumberOfPortals; i++)
            {
                PALeaf1 = _masterData.PortalArray[i].LeafOwnerArray[0];
                PALeaf2 = _masterData.PortalArray[i].LeafOwnerArray[1];

                if ((checkPortalLeaf1 == PALeaf1 && checkPortalLeaf2 == PALeaf2) ||
                        (checkPortalLeaf1 == PALeaf2 && checkPortalLeaf2 == PALeaf1))
                {
                    var min1 = checkPortal.VertexData.Select(x => new Vector3(x.Position.X, x.Position.Y, x.Position.Z)).Min();
                    var max1 = checkPortal.VertexData.Select(x => new Vector3(x.Position.X, x.Position.Y, x.Position.Z)).Max();
                    var min2 = _masterData.PortalArray[i].VertexData.Select(x => new Vector3(x.Position.X, x.Position.Y, x.Position.Z)).Min();
                    var max2 = _masterData.PortalArray[i].VertexData.Select(x => new Vector3(x.Position.X, x.Position.Y, x.Position.Z)).Max();

                    float newSize = (max1 - min1).Length();
                    float oldSize = (max2 - min2).Length();

                    if (Math.Abs(newSize) > Math.Abs(oldSize))
                    {
                        _masterData.PortalArray.RemoveAt(i);
                        portalIndex = i;
                        return false;
                    }
                    else
                    {
                        portalIndex = -1;
                        return true;
                    }

                }
            }
            portalIndex = _masterData.NumberOfPortals;
            return false;
        }
    }
}