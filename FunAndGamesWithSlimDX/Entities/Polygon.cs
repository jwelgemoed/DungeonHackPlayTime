using FunAndGamesWithSlimDX.Entities;
using SlimDX;
using System;

namespace DungeonHack.Entities
{
    public class Polygon : IDisposable
    {
        public SlimDX.Direct3D11.Buffer VertexBuffer { get; set; }
        public SlimDX.Direct3D11.Buffer IndexBuffer { get; set; }
        public Vertex[] VertexData { get; set; }
        public short[] IndexData { get; set; }
        public BoundingBox BoundingBox { get; set; }
        public Matrix ScaleMatrix { get; set; }
        public Matrix TranslationMatrix { get; set; }
        public Matrix RotationMatrix { get; set; }
        public Matrix WorldMatrix { get; set; }

        public int MaterialIndex { get; set; }

        public int TextureIndex { get; set; }

        public void Dispose()
        {
            if (VertexBuffer != null)
                VertexBuffer.Dispose();
            if (IndexBuffer != null)
                IndexBuffer.Dispose();
        }

    }
}
