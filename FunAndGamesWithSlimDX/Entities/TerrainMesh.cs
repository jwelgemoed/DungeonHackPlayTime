using FunAndGamesWithSlimDX.DirectX;
using SlimDX;
using SlimDX.Direct3D11;

namespace FunAndGamesWithSlimDX.Entities
{
    public class TerrainMesh : Mesh
    {
        public float Height { get; set; }

        public float GridSize { get; set; }

        public float GridDimensionSize { get; set; }

        public TerrainMesh(Device device, Color4 color, IShader shader, float height, float width, float depth, int m, int n)
            : base(device, shader)
        {
            MeshRenderPrimitive = PrimitiveTopology.TriangleList;

            int faceCount = (m - 1) * (n - 1) * 2;

            IndexData = new short[faceCount * 3];
            VertexData = new Vertex[m * n];

            float halfWidth = 0.5f * width;
            float halfDepth = 0.5f * depth;

            float dx = width / (n - 1.0f);
            float dz = depth / (m - 1.0f);

            float du = 1.0f / (n - 1.0f);
            float dv = 1.0f / (m - 1.0f);

            int k = 0;

            for (int i = 0; i < m; i++)
            {
                float z = halfDepth - i * dz;

                for (int j = 0; j < n; j++)
                {
                    float x = -halfWidth + j * dx;

                    VertexData[i * n + j] = (new Vertex()
                    {
                        Position = new Vector4(x, 1, z, 1),//(float) (height*(Math.Sin(x)*Math.Cos(z))), z, 1),
                        Normal = new Vector3(0.0f, 1.0f, 0.0f),
                        Texture = new Vector2(j * du, i * dv),
                        //TangentU = new Vector3(1.0f, 0.0f, 0.0f),
                        //Color = Colors.Green
                    });

                    if ((i < m - 1) && (j < n - 1))
                    {
                        IndexData[k] = (short)(i * n + j);
                        IndexData[k + 1] = (short)(i * n + 1 + j);
                        IndexData[k + 2] = (short)((i + 1) * n + j);
                        IndexData[k + 3] = (short)((i + 1) * n + j);
                        IndexData[k + 4] = (short)(i * n + j + 1);
                        IndexData[k + 5] = (short)((i + 1) * n + j + 1);

                        k += 6;
                    }
                }
            }
        }
    }
}