using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameData
{
    public class LineSegment
    {
        public static int Count;

        public int Id { get; set; }
        public Vertex Start { get; set; }
        public Vertex End { get; set; }
        public int TextureId { get; set; }
        public bool IsSolid { get; set; }
        public float FloorHeight { get; set; }
        public float CeilingHeight { get; set; }

        public LineSegment()
        {
            //Id = Count + 1;
            Count++;
        }

        public override string ToString()
        {
            return $"{Start} {End} {TextureId} {IsSolid} {FloorHeight} {CeilingHeight}";
        }
    }
}
