using System.Collections.Generic;
using Assimp;
using FunAndGamesWithSlimDX.Entities;
using DungeonHack.Builders;
using SlimDX.Direct3D11;
using FunAndGamesWithSlimDX.DirectX;

namespace DungeonHack.Factories
{
    public class ModelFactory
    {
        private Device _device;
        private IShader _shader;

        public ModelFactory(Device device, IShader shader)
        {
            _device = device;
            _shader = shader;
        }

        public List<Polygon> CreateModelFromFile(string filename)
        {
            AssimpContext context = new AssimpContext();
            List<Polygon> polygons = new List<Polygon>();

            var scene = context.ImportFile(filename);
            var pBuilder = new PolygonBuilder(_device, _shader);

            foreach (var mesh in scene.Meshes)
            {
                pBuilder.New();
                pBuilder.LoadFromMesh(mesh);
                polygons.Add(pBuilder.Build());
            }

            return polygons;
            
        }


    }
}
