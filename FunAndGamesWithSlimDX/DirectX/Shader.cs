using DungeonHack.Lights;
using FunAndGamesWithSharpDX.DirectX;
using FunAndGamesWithSharpDX.Entities;
using SharpDX;
using SharpDX.Direct3D11;

namespace DungeonHack.DirectX
{
    public class Shader : IShader
    {
        private Device _device;
        private DeviceContext _context;
        private LightShader _lightShader;
        private TextureShader _textureShader;
        private IShader _currentShader;

        public void Dispose()
        {
            _lightShader?.Dispose();

            _textureShader?.Dispose();
        }

        public void SetShader(ShaderTechnique shaderTechnique)
        {
            switch (shaderTechnique)
            {
                case ShaderTechnique.LightShader:
                    _lightShader.Initialize(_device, _context);
                    _currentShader = _lightShader;
                    break;
                case ShaderTechnique.TextureShader:
                    _textureShader.Initialize(_device, _context);
                    _currentShader = _textureShader;
                    break;
            }
        }

        public void Initialize(Device device, DeviceContext context)
        {
            _device = device;
            _context = context;

            _textureShader = new TextureShader(device, context);
            _lightShader = new LightShader(device, context);

            SetShader(ShaderTechnique.LightShader);
        }

        public void Render(DeviceContext context, int indexCount, Matrix worldMatrix, Matrix viewMatrix, Matrix viewProjMatrix, Texture texture, Vector3 cameraPosition, Material material)
        {
            _currentShader.Render(context, indexCount, worldMatrix, viewMatrix, viewProjMatrix, texture, cameraPosition, material);
        }

        public void RenderLights(DirectionalLight[] directionalLight, PointLight[] pointLight, Spotlight[] spotLight)
        {
            _currentShader.RenderLights(directionalLight, pointLight, spotLight);
        }
    }
}
