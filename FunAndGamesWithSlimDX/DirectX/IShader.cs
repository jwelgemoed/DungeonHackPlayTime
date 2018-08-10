using DungeonHack.Lights;
using FunAndGamesWithSharpDX.DirectX;
using FunAndGamesWithSharpDX.Engine;
using FunAndGamesWithSharpDX.Entities;
using SharpDX;

namespace DungeonHack.DirectX
{
    public interface IShader
    {
        void Dispose();
        void Initialize();
        void Render(int threadNumber, int indexCount, Matrix worldMatrix, Matrix viewMatrix, Matrix viewProjectionMatrix, Texture texture, Vector3 cameraPosition, Material material);
        void RenderLights(DirectionalLight[] directionalLight, PointLight[] pointLight, Spotlight[] spotLight);
        void RenderFrame(Camera camera);
    }
}