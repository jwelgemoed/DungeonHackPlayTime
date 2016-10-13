using FunAndGamesWithSlimDX.DirectX;
using SlimDX;
using SlimDX.Direct3D11;

namespace FunAndGamesWithSlimDX.Entities
{
    public class TriangleMesh : Mesh
    {
        public TriangleMesh(Device device, float size, IShader shader) : base(device, shader)
        {
            VertexData = new Vertex[]
                {
                    new Vertex() { Position = new Vector4(-1, -1, 0, 1), Texture = new Vector2(0f, 1f)},
                    new Vertex() { Position = new Vector4(0f, 1f, 0f, 1), Texture = new Vector2(0.5f, 0f)},
                    new Vertex() { Position = new Vector4(1f, -1f, 0f, 1), Texture = new Vector2(1f, 1f)},
                };

            IndexData = new short[]
                {
                    //our 1 face consists of 1 triangle
                    0, 1, 2
                };

            MeshRenderPrimitive = PrimitiveTopology.TriangleList;
        }
    }
}