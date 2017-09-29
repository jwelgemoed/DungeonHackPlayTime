using FunAndGamesWithSlimDX.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FunAndGamesWithSlimDX.DirectX;
using SlimDX.Direct3D11;
using AutoMapper;

namespace DungeonHack.BSP.LeafBsp
{
    public class Portal : Polygon
    {
        private Device _device;
        private IShader _shader;

        public Portal(Device device, IShader shader) 
            : base(device, shader)
        {
            _device = device;
            _shader = shader;
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
