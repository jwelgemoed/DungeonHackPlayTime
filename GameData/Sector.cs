using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameData
{
    public class Sector
    {
        public static int Count;

        public int Id { get; set; }

        public List<int> SideDefinitions { get; set; }

        public int Effect { get; set; }

        public int FloorTextureId { get; set; }

        public int CeilingTextureId { get; set; }

        public float FloorHeight { get; set; }

        public float CeilingHeight { get; set; }

        public float XPlacement { get; set; }
        public float YPlacement { get; set; }
        
        public Sector()
        {
            SideDefinitions = new List<int>();
            Id = Count + 1;
            Count++;
        }

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();

            stringBuilder.Append($"{Effect} {FloorTextureId} {CeilingTextureId} {FloorHeight} {CeilingHeight} ");

            foreach (int side in SideDefinitions)
                stringBuilder.Append($"{side} ");

            return stringBuilder.ToString().Trim();
        }
    }
}
