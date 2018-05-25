using DungeonHack.BSP.LeafBsp;
using DungeonHack.Builders;
using SharpDX;
using System;
using System.Collections.Generic;

namespace DungeonHack.BSP.PVS
{
    public class PVSCalculator
    {
        private readonly PortalBuilder _portalBuilder;
        private readonly PolygonClassifier _polyClassifier;
        private readonly PointClassifier _pointClassifier;
        private readonly PortalSplitter _splitter;
        private LeafBspMasterData _masterData;
        private int _bytesPerSet;

        public PVSCalculator(PolygonClassifier polygonClassifier, PortalSplitter portalSplitter, PointClassifier pointClassifier, 
                        LeafBspMasterData masterData)
        {
            _masterData = masterData;
            _polyClassifier = polygonClassifier;
            _splitter = portalSplitter;
            _pointClassifier = pointClassifier;

            _bytesPerSet = (_masterData.NumberOfLeaves + 7) >> 3;

            _masterData.PVSData = new Byte[_masterData.NumberOfLeaves];
        }

        public long CalculatePVS()
        {
            byte[] leafPvs = new byte[_bytesPerSet];
            int pvsMasterWriterPointer = 0;
            for (int leaf=0; leaf<_masterData.NumberOfLeaves;leaf++)
            {
                _masterData.LeafArray[leaf].PVSIndex = pvsMasterWriterPointer;
                SetPvsBit(leafPvs, leaf);

                for (int spi=0; spi<_masterData.LeafArray[leaf].NumberOfPortals; spi++)
                {
                    Portal sourcePortal = _masterData.PortalArray[_masterData.LeafArray[leaf].PortalIndexList[spi]];
                    int targetLeaf = sourcePortal.LeafOwnerArray[0];
                    if (targetLeaf == leaf)
                    {
                        targetLeaf = sourcePortal.LeafOwnerArray[1];
                    }
                    SetPvsBit(leafPvs, targetLeaf);

                    for (int dpi=0; dpi<_masterData.LeafArray[targetLeaf].NumberOfPortals; dpi++)
                    {
                        Portal targetPortal = _masterData.PortalArray[_masterData.LeafArray[targetLeaf].PortalIndexList[dpi]];

                        if ((sourcePortal != targetPortal) &&
                                (_polyClassifier.ClassifyPolygon(GetPortalPlane(sourcePortal), targetPortal) 
                                    != PolygonClassification.OnPlane))
                        {
                            RecursePvs(leaf, sourcePortal, targetPortal, targetLeaf, leafPvs);
                        }
                    }
                }

                pvsMasterWriterPointer += CompressLeafSet(leafPvs, pvsMasterWriterPointer);
            }

            return pvsMasterWriterPointer;
        }

        private int CompressLeafSet(byte[] visArray, int writePos)
        {
            int j, rep;
            byte dest = _masterData.PVSData[writePos];
            byte dest_p = dest;
            int currentPos = writePos;

            for (int i = 0; i < _bytesPerSet; i++)
            {
                currentPos++;
                _masterData.PVSData[currentPos] = visArray[i];
                if (visArray[i] > 0)
                    continue;

                rep = 1;
                for (i++; i < _bytesPerSet; i++)
                {
                    if ((visArray[i] | rep) == 255)
                    {
                        break;
                    }
                    else
                    {
                        rep++;
                    }
                }

                currentPos++;
                _masterData.PVSData[currentPos] = (byte) rep;
                i--;
            }

            return currentPos - writePos;
        }

        private void RecursePvs(int sourceLeaf, Portal srcPortal, Portal targetPortal, int targetLeaf, byte[] leafPvs)
        {
            int generatorLeaf = targetPortal.LeafOwnerArray[0];

            if (generatorLeaf == targetLeaf)
            {
                generatorLeaf = targetPortal.LeafOwnerArray[1];
            }

            SetPvsBit(leafPvs, generatorLeaf);

            Vector3 sourceLeafCenter = (_masterData.LeafArray[sourceLeaf].BoundingBox.Maximum + _masterData.LeafArray[sourceLeaf].BoundingBox.Minimum) / 2;
            Vector3 targetLeafCenter = (_masterData.LeafArray[targetLeaf].BoundingBox.Maximum + _masterData.LeafArray[targetLeaf].BoundingBox.Minimum) / 2;

            var sourceLeafLocation = _pointClassifier.ClassifyPoint(sourceLeafCenter, GetPortalPlane(srcPortal));
            var targetLeafLocation = _pointClassifier.ClassifyPoint(targetLeafCenter, GetPortalPlane(targetPortal));

            for (int gpi = 0; gpi < _masterData.LeafArray[generatorLeaf].NumberOfPortals; gpi++)
            {
                if (_masterData.PortalArray[_masterData.LeafArray[generatorLeaf].PortalIndexList[gpi]] == targetPortal)
                {
                    continue;
                }

                Portal sourcePortal = srcPortal.Copy();
                Portal generatorPortal = _masterData.PortalArray[_masterData.LeafArray[generatorLeaf].PortalIndexList[gpi]].Copy();

                var generatorLocation = _polyClassifier.ClassifyPolygon(GetPortalPlane(sourcePortal), generatorPortal);

                if (generatorLocation == PolygonClassification.OnPlane
                    || (int)generatorLocation == (int)sourceLeafLocation)
                {
                    generatorPortal.Dispose();
                    sourcePortal.Dispose();
                    continue;
                }

                generatorLocation = _polyClassifier.ClassifyPolygon(GetPortalPlane(targetPortal), generatorPortal);

                if (generatorLocation == PolygonClassification.OnPlane
                    || (int) generatorLocation == (int)targetLeafLocation)
                {
                    generatorPortal.Dispose();
                    sourcePortal.Dispose();
                    continue;
                }

                generatorPortal = ClipToAntiPenumbra(sourcePortal, targetPortal, generatorPortal);

                if (generatorPortal == null)
                {
                    sourcePortal?.Dispose();
                    continue;
                }

                sourcePortal = ClipToAntiPenumbra(generatorPortal, targetPortal, sourcePortal);

                if (sourcePortal == null)
                {
                    generatorPortal?.Dispose();
                    continue;
                }

                RecursePvs(sourceLeaf, sourcePortal, generatorPortal, generatorLeaf, leafPvs);

                generatorPortal.Dispose();
                sourcePortal.Dispose();
            }

        }

        private Portal ClipToAntiPenumbra(Portal sourcePortal, Portal targetPortal, Portal generatorPortal)
        {
            Vector4 edge1, edge2;
            Vector3 normal;
            int portalLocation, nextVertex;

            Portal frontSplit, backSplit, tempSource, tempTarget;

            ClipPlanes clipPlanes = new ClipPlanes()
            {
                Planes = new List<Entities.Plane>()
            };

            Entities.Plane tempPlane;

            for (int i=0; i<2; i++)
            {
                if (i == 0)
                {
                    tempSource = sourcePortal;
                    tempTarget = targetPortal;
                }
                else
                {
                    tempSource = targetPortal;
                    tempTarget = sourcePortal;
                }

                for (int sv=0; sv<tempSource.VertexData.Length; sv++)
                {
                    portalLocation = (int) _pointClassifier.ClassifyPoint(
                        new Vector3(tempSource.VertexData[sv].Position.X, tempSource.VertexData[sv].Position.Y, tempSource.VertexData[sv].Position.Z)
                        , GetPortalPlane(tempSource));

                    if (portalLocation == (int)PointClassification.OnPlane)
                        continue;

                    for (int tp = 0; tp < tempTarget.VertexData.Length; tp++)
                    {
                        if (tp == tempTarget.VertexData.Length - 1)
                        {
                            nextVertex = 0;
                        }
                        else
                        {
                            nextVertex = tp + 1;
                        }

                        edge1 = tempSource.VertexData[sv].Position - tempTarget.VertexData[tp].Position;
                        edge2 = tempTarget.VertexData[nextVertex].Position - tempTarget.VertexData[tp].Position;

                        normal = Vector3.Cross(new Vector3(edge1.X, edge1.Y, edge1.Z), new Vector3(edge2.X, edge2.Y, edge2.Z));
                        normal.Normalize();

                        tempPlane = new Entities.Plane();
                        tempPlane.Normal = normal;
                        tempPlane.PointOnPlane = new Vector3(tempSource.VertexData[sv].Position.X, tempSource.VertexData[sv].Position.Y, tempSource.VertexData[sv].Position.Z);

                        if (_polyClassifier.ClassifyPolygon(tempPlane, tempSource) == PolygonClassification.Front)
                        {
                            if (_polyClassifier.ClassifyPolygon(tempPlane, tempTarget) == PolygonClassification.Back)
                            {
                                clipPlanes.Planes.Add(tempPlane);
                            }
                        }
                        else
                        {
                            if (_polyClassifier.ClassifyPolygon(tempPlane, tempSource) == PolygonClassification.Back)
                            {
                                if (_polyClassifier.ClassifyPolygon(tempPlane, tempTarget) == PolygonClassification.Front)
                                {
                                    clipPlanes.Planes.Add(tempPlane);
                                }
                            }
                        }
                    }
                }
            }

            for (int i =0; i< clipPlanes.NumberOfPlanes; i++)
            {
                portalLocation = (int) _polyClassifier.ClassifyPolygon(clipPlanes.Planes[i], generatorPortal);
                int sourcePortalLocation = (int) _polyClassifier.ClassifyPolygon(clipPlanes.Planes[i], sourcePortal);

                if (portalLocation == sourcePortalLocation || portalLocation == (int) PolygonClassification.OnPlane)
                {
                    generatorPortal?.Dispose();
                    return null;
                }

                if ((portalLocation == (int) PolygonClassification.Back && sourcePortalLocation == (int) PolygonClassification.Front) ||
                        (portalLocation == (int) PolygonClassification.Front && sourcePortalLocation == (int) PolygonClassification.Back))
                {
                    continue;
                }

                if (portalLocation == (int) PolygonClassification.Spanning)
                {
                    _splitter.Split(generatorPortal, clipPlanes.Planes[i], out frontSplit, out backSplit);

                    if (sourcePortalLocation == (int) PolygonClassification.Front)
                    {
                        frontSplit?.Dispose();
                        generatorPortal?.Dispose();
                        generatorPortal = backSplit;
                    }
                    else if (sourcePortalLocation == (int) PolygonClassification.Back)
                    {
                        backSplit?.Dispose();
                        generatorPortal?.Dispose();
                        generatorPortal = frontSplit;
                    }
                }
            }

            return generatorPortal;
        }

        private Entities.Plane GetPortalPlane(Portal sourcePortal)
        {
            return new Entities.Plane
            {
                Normal = sourcePortal.Normal,
                PointOnPlane = new Vector3(sourcePortal.VertexData[0].Position.X,
                                            sourcePortal.VertexData[0].Position.Y,
                                            sourcePortal.VertexData[0].Position.Z)
            };
        }

        private void SetPvsBit(byte[] leafPvs, int leaf)
        {
            long byteToSet = leaf >> 3;
            byte bitToSet = (byte)(leaf - (byteToSet << 3));
            leafPvs[bitToSet] |= Convert.ToByte(1 << bitToSet);
        }
    }
}
