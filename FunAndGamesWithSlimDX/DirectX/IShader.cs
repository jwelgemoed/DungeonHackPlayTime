using DungeonHack.Lights;
using FunAndGamesWithSharpDX.DirectX;
using FunAndGamesWithSharpDX.Engine;
using FunAndGamesWithSharpDX.Entities;
using SharpDX;
using SharpDX.Direct3D11;

namespace DungeonHack.DirectX
{
    public interface IShader
    {
        void Dispose();
        void Initialize(Device device, DeviceContext context, DeviceContext[] deferredContexts);
        void Render(DeviceContext context, int indexCount, Matrix worldMatrix, Matrix viewMatrix, Matrix viewProjectionMatrix, Texture texture, Vector3 cameraPosition, Material material);
        void RenderLights(DirectionalLight[] directionalLight, PointLight[] pointLight, Spotlight[] spotLight);
        void RenderFrame(Camera camera);
    }
}