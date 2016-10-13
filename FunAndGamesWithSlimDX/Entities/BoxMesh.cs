using FunAndGamesWithSlimDX.DirectX;
using SlimDX;
using SlimDX.Direct3D11;

namespace FunAndGamesWithSlimDX.Entities
{
    public class BoxMesh : Mesh
    {
        public BoxMesh(Device device, float size, IShader shader)
            : base(device, shader)
        {
            var left = -size;
            var right = size;
            var top = size;
            var bottom = -size;
            var front = -size;
            var back = size;

            VertexData = new Vertex[]
                { 
                    //Front Face
                    new Vertex() { Position = new Vector4(left, top, front, 1), Texture = new Vector2(0f, 0f), Normal = new Vector3(0.0f, 0.0f, -1.0f)}, // Top left
                    new Vertex() { Position = new Vector4(right, top, front, 1), Texture = new Vector2(1f, 0f), Normal = new Vector3(0.0f, 0.0f, -1.0f)},  // Top Right
                    new Vertex() { Position = new Vector4(right, bottom, front, 1), Texture = new Vector2(1f, 1f), Normal = new Vector3(0.0f, 0.0f, -1.0f)}, // Bottom Right
                    new Vertex() { Position = new Vector4(left, bottom, front, 1), Texture = new Vector2(0f, 1f), Normal = new Vector3(0.0f, 0.0f, -1.0f)},  // Bottom Left

                    //Back Face
                    new Vertex() { Position = new Vector4(left, top, back, 1), Texture = new Vector2(1, 0), Normal = new Vector3(0.0f, 0.0f, 1.0f)}, 
                    new Vertex() { Position = new Vector4(right, top, back, 1), Texture = new Vector2(0f, 0f), Normal = new Vector3(0.0f, 0.0f, 1.0f)}, 
                    new Vertex() { Position = new Vector4(right, bottom, back, 1), Texture = new Vector2(0f, 1f), Normal = new Vector3(0.0f, 0.0f, 1.0f)},
                    new Vertex() { Position = new Vector4(left, bottom, back, 1), Texture = new Vector2(1f, 1f), Normal = new Vector3(0.0f, 0.0f, 1.0f)}, 

                    
                };

            IndexData = new short[]
                {
                    0, 1, 2, 0, 2, 3, //front face

                    6, 5, 4, 7, 6, 4, //w back face

                    4, 0, 3, 3, 7, 4, // left face

                    2, 1, 5, 5, 6, 2, // right face

                    5, 1, 0, 0, 4, 5, // top face

                    7, 2, 6, 2, 7, 3//bottom face 
               
                };

            MeshRenderPrimitive = PrimitiveTopology.TriangleList;
        }
    }
}
