using FunAndGamesWithSharpDX.DirectX;
using FunAndGamesWithSharpDX.Entities;
using FunAndGamesWithSharpDX.Lights;
using SharpDX;
using SharpDX.Direct3D11;

namespace FunAndGamesWithSharpDX.DirectX
{
    public class Shader : IShader
    {
        private Device _device;
        private DeviceContext _context;
        private LightShader _lightShader;
        private TextureShader _textureShader;
        private EffectsShader _effectsShader;
        private IShader _currentShader;

        public void Dispose()
        {
            if (_lightShader != null)
                _lightShader.Dispose();

            if (_textureShader != null)
                _textureShader.Dispose();
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
                case ShaderTechnique.EffectsShader:
                    _effectsShader.Initialize(_device, _context);
                    _currentShader = _effectsShader;
                    break;
            }
        }

        public void Initialize(Device device, DeviceContext context)
        {
            _device = device;
            _context = context;

            _textureShader = new TextureShader(device, context);
            _lightShader = new LightShader(device, context);
            _effectsShader = new EffectsShader(device);

            SetShader(ShaderTechnique.LightShader);
        }

        public void Render(DeviceContext context, int indexCount, Matrix worldMatrix, Matrix viewMatrix, Matrix projectionMatrix, ShaderResourceView texture, Vector3 cameraPosition, Material material)
        {
            _currentShader.Render(context, indexCount, worldMatrix, viewMatrix, projectionMatrix, texture, cameraPosition, material);
        }

        public void RenderLights(DirectionalLight directionalLight, PointLight pointLight, Spotlight spotLight)
        {
            _currentShader.RenderLights(directionalLight, pointLight, spotLight);
        }
    }
}
