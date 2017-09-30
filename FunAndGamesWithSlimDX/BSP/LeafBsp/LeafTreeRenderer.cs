using DungeonHack.DataDictionaries;
using DungeonHack.Entities;
using FunAndGamesWithSlimDX.DirectX;
using FunAndGamesWithSlimDX.Engine;
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
    public class LeafTreeRenderer
    {
        private List<Polygon> _polygonArray ;
        private List<Node> _nodeArray ;
        private List<Leaf> _leafArray ;
        private List<Entities.Plane> _planeArray;
        //public Portal[] PortalArray;
        private List<byte> _pvsData;
        private PointClassifier _pointClassifier;
        private PolygonRenderer _polyRenderer;

        private int NumberOfLeafs { get { return _leafArray.Count; } }

        public LeafTreeRenderer(List<Polygon> polygonArray, List<Node> nodeArray, List<Leaf> leafArray, List<Entities.Plane> planeArray,
                                List<byte> pvsData, PointClassifier pointClassifier, PolygonRenderer polyRenderer)
        {
            _polygonArray = polygonArray;
            _nodeArray = nodeArray;
            _leafArray = leafArray;
            _planeArray = planeArray;
            _pvsData = pvsData;
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
                switch(_pointClassifier.ClassifyPoint(position, _planeArray[_nodeArray[node].Plane]))
                {
                    case PointClassification.OnPlane:
                    case PointClassification.Front:
                        if (_nodeArray[node].IsLeaf)
                        {
                            leaf = _nodeArray[node].Front;
                            DrawTree(leaf, frustrum, ref polycounter);
                            found = true;
                        }
                        else
                        {
                            node = _nodeArray[node].Front;
                        }
                        break;
                    case PointClassification.Back:
                        if (_nodeArray[node].Back == -1)
                        {
                            found = true;
                        }
                        else
                        {
                            node = _nodeArray[node].Back;
                        }
                        break;

                }
            }
        }

        private void DrawTree(int leaf, Frustrum frustrum, ref int counter)
        {
            Polygon currentPoly;
            int pvsoffset = _leafArray[leaf].PVSIndex;
            byte pvspointer = _pvsData[pvsoffset];
            int currentleaf = 0;

            while (currentleaf < NumberOfLeafs)
            {
                if (pvspointer != 0)
                {
                    for (int i=0; i<8; i++)
                    {
                        Byte mask = (byte) (1 << i);
                        Byte pvs = pvspointer;
                        if ((pvs & mask) != 0)
                        {
                            //Render
                            for (int j = _leafArray[currentleaf].StartPolygon; j < _leafArray[currentleaf].EndPolygon; j++)
                            {
                                _polyRenderer.Render(frustrum, _polygonArray[j], ref counter);
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

        
        private bool LeafInFrustrum(int leaf, Frustrum frustrum)
        {
            return frustrum.CheckBoundingBox(_leafArray[leaf].BoundingBox) > 0;
        }
    }
}
