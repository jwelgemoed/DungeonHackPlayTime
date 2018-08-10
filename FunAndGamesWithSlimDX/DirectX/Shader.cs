using DungeonHack.Lights;
using FunAndGamesWithSharpDX.DirectX;
using FunAndGamesWithSharpDX.Engine;
using FunAndGamesWithSharpDX.Entities;
using SharpDX;

namespace DungeonHack.DirectX
{
    public class Shader : IShader
    {
        private LightShader _lightShader;
        private TextureShader _textureShader;
        private IShader _currentShader;

        public void Dispose()
        {
            _lightShader?.Dispose();

            _textureShader?.Dispose();
        }

        public Shader(Renderer renderer)
        {
            _textureShader = new TextureShader(renderer);
            _lightShader = new LightShader(renderer);

            SetShader(ShaderTechnique.LightShader);
        }

        public void SetShader(ShaderTechnique shaderTechnique)
        {
            switch (shaderTechnique)
            {
                case ShaderTechnique.LightShader:
                    _lightShader.Initialize();
                    _currentShader = _lightShader;
                    break;
                case ShaderTechnique.TextureShader:
                    _textureShader.Initialize();
                    _currentShader = _textureShader;
                    break;
            }
        }

        public void Initialize()
        {
            SetShader(ShaderTechnique.LightShader);
        }

        public void RenderFrame(Camera camera)
        {
            _currentShader.RenderFrame(camera);
        }

        public void Render(int threadNumber, int indexCount, Matrix worldMatrix, Matrix viewMatrix, Matrix viewProjMatrix, Texture texture, Vector3 cameraPosition, Material material)
        {
            _currentShader.Render(threadNumber, indexCount, worldMatrix, viewMatrix, viewProjMatrix, texture, cameraPosition, material);
        }

        public void RenderLights(DirectionalLight[] directionalLight, PointLight[] pointLight, Spotlight[] spotLight)
        {
            _currentShader.RenderLights(directionalLight, pointLight, spotLight);
        }
    }
}
