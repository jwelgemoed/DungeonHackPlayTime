using DungeonHack.DirectX;
using FunAndGamesWithSharpDX.DirectX;
using SharpDX;
using System;
using Buffer = SharpDX.Direct3D11.Buffer;
using Device = SharpDX.Direct3D11.Device;

namespace FunAndGamesWithSharpDX.Entities
{
    public class Polygon : IDisposable
    {
        public Buffer VertexBuffer { get; set; }
        public Buffer IndexBuffer { get; set; }

        public Vertex[] VertexData { get; set; }

        public Vector3[] WorldVectors { get; set; }

        public short[] IndexData { get; set; }

        public Model[] Model { get; set; }

        public Vector3 Normal { get { return VertexData[0].Normal; } }

        public BoundingBox BoundingBox { get; set; }

        public int TextureIndex { get; set; }
        public int MaterialIndex { get; set; }

        public Matrix ScaleMatrix { get; set; }
        public Matrix TranslationMatrix { get; set; }
        public Matrix RotationMatrix { get; set; }
        public Matrix WorldMatrix { get; set; }

        public bool HasBeenUsedAsSplitPlane { get; set; }

        public OcclusionQuery OcclusionQuery { get; private set; }

        public Polygon(Device device, Shader shader)
        {
            ScaleMatrix = Matrix.Identity;
            TranslationMatrix = Matrix.Identity;
            RotationMatrix = Matrix.Identity;
            WorldMatrix = Matrix.Identity;
            OcclusionQuery = new OcclusionQuery(device);
        }

        public void Dispose()
        {
            if (VertexBuffer != null)
                VertexBuffer.Dispose();
            if (IndexBuffer != null)
                IndexBuffer.Dispose();
            if (OcclusionQuery != null)
                OcclusionQuery.Dispose();
        }
    }
}
