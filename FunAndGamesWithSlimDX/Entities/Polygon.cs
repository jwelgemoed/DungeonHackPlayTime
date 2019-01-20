using System;
using DungeonHack.DirectX;
using FunAndGamesWithSharpDX.Entities;
using SharpDX;
using Buffer = SharpDX.Direct3D11.Buffer;
using Device = SharpDX.Direct3D11.Device;

namespace DungeonHack.Entities
{
    public class Polygon : IDisposable
    {
        public Buffer VertexBuffer { get; set; }

        public Buffer IndexBuffer { get; set; }

        public Vertex[] VertexData { get; set; }

        public Vector3[] WorldVectors { get; set; }

        public short[] IndexData { get; set; }

        public Model[] Model { get; set; }

        public Vector3 Normal => VertexData[0].Normal;

        public AABoundingBox BoundingBox { get; set; }

        public int TextureIndex { get; set; }
        public int MaterialIndex { get; set; }

        public Matrix ScaleMatrix { get; set; }
        public Matrix TranslationMatrix { get; set; }
        public Matrix RotationMatrix { get; set; }
        public Matrix WorldMatrix { get; set; }

        public PolygonType PolygonType { get; set; }

        public Polygon(Device device, Shader shader)
        {
            ScaleMatrix = Matrix.Identity;
            TranslationMatrix = Matrix.Identity;
            RotationMatrix = Matrix.Identity;
            WorldMatrix = Matrix.Identity;
        }

        public void Dispose()
        {
            VertexBuffer?.Dispose();
            IndexBuffer?.Dispose();
            BoundingBox?.Dispose();
        }
    }
}
