using FunAndGamesWithSharpDX.Entities;
using SharpDX;
using SharpDX.Direct3D11;

namespace DungeonHack.DirectX
{
    public class BufferFactory
    {
        private readonly Device _device;

        public BufferFactory(Device device)
        {
            _device = device;
        }

        public Buffer GetVertexBuffer(Vertex[] vertexData)
        {
            var vertices = new DataStream(Vertex.SizeOf * vertexData.Length, true, true);
            vertices.WriteRange(vertexData);
            vertices.Position = 0;

            return new Buffer(_device, vertices, Vertex.SizeOf * vertexData.Length
                    , ResourceUsage.Default, BindFlags.VertexBuffer, CpuAccessFlags.None, ResourceOptionFlags.None, Vertex.SizeOf);
        }

        public Buffer GetIndexBuffer(short[] indexData)
        {
            var indexStream = new DataStream(sizeof(uint) * indexData.Length, true, true);
            indexStream.WriteRange(indexData);
            indexStream.Position = 0;

            return new SharpDX.Direct3D11.Buffer(_device, indexStream, sizeof(uint) * indexData.Length,
                                                           ResourceUsage.Default, BindFlags.IndexBuffer,
                                                           CpuAccessFlags.None, ResourceOptionFlags.None, 0);
        }
    }
}
