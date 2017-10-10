using DungeonHack.BSP.LeafBsp;
using FunAndGamesWithSharpDX.Entities;
using System.Collections.Generic;

namespace DungeonHack.BSP
{
    public class LeafBspMasterData
    {
        public List<Polygon> PolygonArray;
        public List<Node> NodeArray;
        public List<Leaf> LeafArray;
        public List<Entities.Plane> PlaneArray;
        public List<Portal> PortalArray;
        public byte[] PVSData;

        public int NumberOfPolygons { get { return PolygonArray.Count; } }
        public int NumberOfNodes { get { return NodeArray.Count; } }
        public int NumberOfLeaves { get { return LeafArray.Count; } }
        public int NumberOfPlanes { get { return PlaneArray.Count; } }
        public int NumberOfPortals { get { return PortalArray.Count; } }

        public LeafBspMasterData()
        {
            PolygonArray = new List<Polygon>();
            NodeArray = new List<Node>();
            LeafArray = new List<Leaf>();
            PlaneArray = new List<Entities.Plane>();
            PortalArray = new List<Portal>();            
        }
     }
}
