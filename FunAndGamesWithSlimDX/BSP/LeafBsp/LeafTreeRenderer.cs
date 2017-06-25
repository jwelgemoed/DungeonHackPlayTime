using DungeonHack.Entities;
using FunAndGamesWithSlimDX.Entities;
using SlimDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DungeonHack.BSP.LeafBsp
{
    public class LeafTreeRenderer
    {
        private List<Polygon> _meshArray ;
        private List<Node> _nodeArray ;
        private List<Leaf> _leafArray ;
        private List<Entities.Plane> _planeArray;
        //public Portal[] PortalArray;
        private List<byte> _pvsData;
        private PointClassifier _pointClassifier;

        public LeafTreeRenderer(List<Polygon> meshArray, List<Node> nodeArray, List<Leaf> leafArray, List<Entities.Plane> planeArray,
                                List<byte> pvsData, PointClassifier pointClassifier)
        {
            _meshArray = meshArray;
            _nodeArray = nodeArray;
            _leafArray = leafArray;
            _planeArray = planeArray;
            _pvsData = pvsData;
            _pointClassifier = pointClassifier;
        }

        public void Render(Vector3 position)
        {
            int node = 0;
            int leaf = 0;
            bool found = false;

            while (!found)
            {
                switch(_pointClassifier.ClassifyPoint(position, _planeArray[_nodeArray[node].Plane]))
                {
                    case PointClassification.OnPlane:
                    case PointClassification.Front:
                        if (_nodeArray[node].IsLeaf)
                        {
                            leaf = _nodeArray[node].Front;
                            DrawTree(leaf);
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

        private void DrawTree(int leaf)
        {
            Polygon currentMesh;
            int i;
            int pvsoffset = _leafArray[leaf].PVSIndex;
            byte pvspointer = _pvsData[pvsoffset];
            int currentleaf = 0;

        }
    }
}
