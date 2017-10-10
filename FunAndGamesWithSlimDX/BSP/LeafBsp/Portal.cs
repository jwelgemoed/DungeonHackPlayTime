using FunAndGamesWithSharpDX.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FunAndGamesWithSharpDX.DirectX;
using SharpDX.Direct3D11;
using AutoMapper;

namespace DungeonHack.BSP.LeafBsp
{
    public class Portal : Polygon
    {
        private Device _device;
        private Shader _shader;

        public Portal(Device device, Shader shader) 
            : base(device, shader)
        {
            _device = device;
            _shader = shader;
            LeafOwnerArray = new int[2];
        }

        public int NumberOfLeafs { get; set; }

        public int[] LeafOwnerArray { get; set; }

        public Portal Next { get; set; }

        public Portal Previous { get; set; }

        public bool Deleted { get; set; }

        public Portal Copy()
        {
            Portal copy = new Portal(_device, _shader);

            Mapper.Map<Portal, Portal>(this, copy);

            return copy;
        }
    }
}
