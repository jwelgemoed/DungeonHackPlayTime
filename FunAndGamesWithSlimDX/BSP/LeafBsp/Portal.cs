using FunAndGamesWithSlimDX.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FunAndGamesWithSlimDX.DirectX;
using SlimDX.Direct3D11;

namespace DungeonHack.BSP.LeafBsp
{
    public class Portal : Polygon
    {
        public Portal(Device device, IShader shader) 
            : base(device, shader)
        {
        }

        public int NumberOfLeafs { get; set; }

        public int[] LeafOwnerArray { get; set; }

        public Portal Next { get; set; }

        public Portal Previous { get; set; }
    }
}
