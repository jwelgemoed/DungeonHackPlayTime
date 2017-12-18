using FunAndGamesWithSharpDX.Entities;
using FunAndGamesWithSharpDX.Lights;
using SharpDX;
using SharpDX.Direct3D11;

namespace FunAndGamesWithSharpDX.DirectX
{
    public interface IShader
    {
        void Dispose();
        void Initialize(Device device, DeviceContext[] contextPerThread);
        void Render(int contextCount, int indexCount, Matrix worldMatrix, Matrix viewMatrix, Matrix projectionMatrix, ShaderResourceView texture, Vector3 cameraPosition, Material material);
        void RenderLights(DirectionalLight directionalLight, PointLight pointLight, Spotlight spotLight);
    }
}