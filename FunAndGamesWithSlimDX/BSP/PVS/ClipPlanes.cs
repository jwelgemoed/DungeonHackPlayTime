using DungeonHack.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DungeonHack.BSP.PVS
{
    public class ClipPlanes
    {
        public int NumberOfPlanes { get { return Planes == null ? 0 : Planes.Count; } }

        public List<Plane> Planes { get; set; }
    }
}
