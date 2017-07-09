using System;
using System.Collections.Generic;
using System.Linq;
using FunAndGamesWithSlimDX.DirectX;
using SlimDX.Direct3D11;
using DungeonHack.BSP.LeafBsp;
using FunAndGamesWithSlimDX.Entities;
using SlimDX;

namespace DungeonHack.Builders
{
    public class PortalBuilder : PolygonBuilder
    {
        public PortalBuilder(Device device, IShader shader) : base(device, shader)
        {
        }

        public new PortalBuilder New()
        {
            _mesh = new Portal(_device, _shader);

            return this;
        }

        public PortalBuilder CreateFromVectorsAndNormal(IEnumerable<Vector3> vectors, Vector3 normal)
        {
            if (vectors == null)
            {
                throw new ArgumentNullException(nameof(vectors));
            }

            _mesh.Model = new Model[vectors.Count()];

            int i = 0;

            foreach (var vector in vectors)
            {
                _mesh.Model[i].x = vector.X;
                _mesh.Model[i].y = vector.Y;
                _mesh.Model[i].z = vector.Z;
                _mesh.Model[i].tx = 0.0f;
                _mesh.Model[i].ty = 0.0f;
                _mesh.Model[i].nx = normal.X;
                _mesh.Model[i].ny = normal.Y;
                _mesh.Model[i].nz = normal.Z;
                i++;
            }

            LoadVectorsFromModel(null, null);

            return this;
        }

        public new Portal Build()
        {
            return (Portal)base.Build();
        }
    }
}
