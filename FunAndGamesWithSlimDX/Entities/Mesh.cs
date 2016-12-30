using FunAndGamesWithSlimDX.DirectX;
using SlimDX;
using System;
using Buffer = SlimDX.Direct3D11.Buffer;
using Device = SlimDX.Direct3D11.Device;

namespace FunAndGamesWithSlimDX.Entities
{
    public class Mesh : IDisposable
    {
        public Buffer VertexBuffer { get; set; }
        public Buffer IndexBuffer { get; set; }

        public Vertex[] VertexData { get; set; }

        public short[] IndexData { get; set; }

        public Model[] Model { get; set; }

        public BoundingBox BoundingBox { get; set; }

        public int TextureIndex { get; set; }
        public int MaterialIndex { get; set; }

        public Matrix ScaleMatrix { get; set; }
        public Matrix TranslationMatrix { get; set; }
        public Matrix RotationMatrix { get; set; }
        public Matrix WorldMatrix { get; set; }

        public Mesh(Device device, IShader shader)
        {
            ScaleMatrix = Matrix.Identity;
            TranslationMatrix = Matrix.Identity;
            RotationMatrix = Matrix.Identity;
            WorldMatrix = Matrix.Identity;
        }

        public void Dispose()
        {
            if (VertexBuffer != null)
                VertexBuffer.Dispose();
            if (IndexBuffer != null)
                IndexBuffer.Dispose();
        }
    }
}
