using FunAndGamesWithSlimDX.DirectX;
using SlimDX;
using SlimDX.Direct3D11;

namespace FunAndGamesWithSlimDX.Entities
{
    public class SquareMesh : Mesh
    {
        public SquareMesh(Device device, Color4 color, IShader shader) : base(device, shader)
        {
            VertexData = null;//new Vertex[] ()
                              /*  {
                                    new Vertex() { Position = new Vector3(-0.5f, 0.5f, 0.5f), Color = color},
                                    new Vertex() { Position = new Vector3(0.5f, 0.5f, 0.5f), Color = color},
                                    new Vertex() { Position = new Vector3(0.5f, -0.5f, 0.5f), Color = color},
                                    new Vertex() { Position = new Vector3(-0.5f, -0.5f, 0.5f), Color = color},
                                }*/
            ;

            IndexData = new short[]
                {
                    //our 1 face consists of 2 triangles
                    0, 1, 2,
                    0, 2, 3
                };

            MeshRenderPrimitive = PrimitiveTopology.TriangleList;
        }
    }
}