using FunAndGamesWithSharpDX.Engine;
using FunAndGamesWithSharpDX.Entities;
using SharpDX;
using System;

namespace DungeonHack.BSP.LeafBsp
{
    public class LeafTreeRenderer
    {
        private PointClassifier _pointClassifier;
        private PolygonRenderer _polyRenderer;

        private LeafBspMasterData _masterData;

        public LeafTreeRenderer(LeafBspMasterData masterData, PointClassifier pointClassifier, PolygonRenderer polyRenderer)
        {
            _masterData = masterData;
            _pointClassifier = pointClassifier;
            _polyRenderer = polyRenderer;
        }

        public void Render(Vector3 position, Frustrum frustrum)
        {
            int node = 0;
            int leaf = 0;
            bool found = false;
            int polycounter = 0;

            while (!found)
            {
                switch(_pointClassifier.ClassifyPoint(position, _masterData.PlaneArray[_masterData.NodeArray[node].Plane]))
                {
                    case PointClassification.OnPlane:
                    case PointClassification.Front:
                        if (_masterData.NodeArray[node].IsLeaf)
                        {
                            leaf = _masterData.NodeArray[node].Front;
                            DrawTree(leaf, frustrum, ref polycounter);
                            found = true;
                        }
                        else
                        {
                            node = _masterData.NodeArray[node].Front;
                        }
                        break;
                    case PointClassification.Back:
                        if (_masterData.NodeArray[node].Back == -1)
                        {
                            found = true;
                        }
                        else
                        {
                            node = _masterData.NodeArray[node].Back;
                        }
                        break;

                }
            }
        }

        private void DrawTree(int leaf, Frustrum frustrum, ref int counter)
        {
            int pvsoffset = _masterData.LeafArray[leaf].PVSIndex;
            byte pvspointer = _masterData.PVSData[pvsoffset];
            int currentleaf = 0;

            while (currentleaf < _masterData.NumberOfLeaves)
            {
                if (pvspointer != 0)
                {
                    for (int i=0; i<8; i++)
                    {
                        Byte mask = (byte) (1 << i);
                        Byte pvs = pvspointer;
                        if ((pvs & mask) != 0)
                        {
                            if (ConfigManager.FrustrumCullingEnabled &&
                                frustrum.CheckBoundingBox(_masterData.LeafArray[currentleaf].BoundingBox) == 0)
                            {
                                continue;
                            }

                            for (int j = _masterData.LeafArray[currentleaf].StartPolygon; j < _masterData.LeafArray[currentleaf].EndPolygon; j++)
                            {
                                _polyRenderer.Render(0, frustrum, _masterData.PolygonArray[j], ref counter);
                            }
                        }
                        currentleaf++;
                    }
                    pvspointer++;
                }
                else
                {
                    pvspointer++;
                    Byte runLength = pvspointer;
                    pvspointer++;
                    currentleaf += runLength * 8;
                }
            }

        }
    }
}
