using System.Collections.Generic;
using Assimp;
using FunAndGamesWithSharpDX.Entities;
using DungeonHack.Builders;
using DungeonHack.DirectX;
using DungeonHack.Entities;
using SharpDX.Direct3D11;
using FunAndGamesWithSharpDX.DirectX;

namespace DungeonHack.Factories
{
    public class ModelFactory
    {
        private Device _device;
        private Shader _shader;

        public ModelFactory(Device device, Shader shader)
        {
            _device = device;
            _shader = shader;
        }

        public List<Polygon> CreateModelFromFile(string filename)
        {
            AssimpContext context = new AssimpContext();
            List<Polygon> polygons = new List<Polygon>();

            var scene = context.ImportFile(filename);
            var pBuilder = new PolygonBuilder(_device, _shader, new BufferFactory(_device));

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
