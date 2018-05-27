using FunAndGamesWithSharpDX.Entities;
using FunAndGamesWithSharpDX.Lights;
using SharpDX;
using SharpDX.Direct3D11;

namespace DungeonHack.DirectX
{
    public interface IShader
    {
        void Dispose();
        void Initialize(Device device, DeviceContext context);
        void Render(DeviceContext context, int indexCount, Matrix worldMatrix, Matrix viewMatrix, Matrix viewProjectionMatrix, ShaderResourceView texture, Vector3 cameraPosition, Material material);
        void RenderLights(DirectionalLight directionalLight, PointLight pointLight, Spotlight spotLight);
    }
}