using DungeonHack.Engine;
using DungeonHack.Lights;
using FunAndGamesWithSharpDX.DirectX;
using FunAndGamesWithSharpDX.Engine;
using FunAndGamesWithSharpDX.Entities;
using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.DXGI;

namespace DungeonHack.DirectX
{
    public class Shader : IShader
    {
        private DeferredShadingRenderer _deferredShadingRenderer;
        private Renderer _renderer;
        private DeferredShader _deferredShader;
        private AmbientLightShader _ambientLightShader;
        private DirectionalLightShader _directionalLightShader;
        private PointLightShader _pointLightShader;
        private TextureShader _textureShader;
        private IShader _currentShader;
        private BlendState _blendState;

        private int _numberOfDeferredContexts;

        public void Dispose()
        {
            _pointLightShader?.Dispose();

            _textureShader?.Dispose();

            _deferredShader?.Dispose();

            _directionalLightShader?.Dispose();

            _ambientLightShader?.Dispose();

            _blendState?.Dispose();
        }

        public Shader(Renderer renderer, DeferredShadingRenderer deferredRenderer, Camera camera)
        {
            _renderer = renderer;
            _deferredShadingRenderer = deferredRenderer;
            _textureShader = new TextureShader(renderer);
            _pointLightShader = new PointLightShader(renderer, camera, deferredRenderer);
            _deferredShader = new DeferredShader(renderer);
            _ambientLightShader = new AmbientLightShader(renderer, camera, deferredRenderer);
            _directionalLightShader = new DirectionalLightShader(renderer, camera, deferredRenderer);

            _numberOfDeferredContexts = _renderer.DeferredContexts.Length;

            _deferredShader.Initialize();
            _ambientLightShader.Initialize();
            _directionalLightShader.Initialize();
            _pointLightShader.Initialize();

            _blendState = CreateBlendState();

            SetShader(ShaderTechnique.LightShader);
        }

        public void SetShader(ShaderTechnique shaderTechnique)
        {
            switch (shaderTechnique)
            {
                case ShaderTechnique.LightShader:
                   // _lightShader.Initialize();
                    //_currentShader = _pointLightShader;
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
            SetupFrameRender();

            _deferredShader.RenderFrame(camera);
        }

        public void Render(int threadNumber, int indexCount, Matrix worldMatrix, Matrix viewMatrix, Matrix viewProjMatrix, Texture texture, Vector3 cameraPosition, Material material)
        {
            _deferredShader.Render(threadNumber, indexCount, worldMatrix, viewMatrix, viewProjMatrix, texture, cameraPosition, material);
        }

        public void RenderLights(AmbientLight[] ambientLight, DirectionalLight[] directionalLight, PointLight[] pointLight, Spotlight[] spotLight)
        {
            ResetFrame();

            _renderer.ImmediateContext.ClearRenderTargetView(_renderer.RenderTarget, Colors.Black);
            _renderer.ImmediateContext.ClearDepthStencilView(_renderer.DepthStencilView, DepthStencilClearFlags.Depth, 1.0f, 0);

            _renderer.TurnZBufferOff();

            //Do render fullscreen quad?

            TurnOnAlphaBlending();

            _ambientLightShader.SwitchShader();
            _ambientLightShader.RenderLights(ambientLight);

            _directionalLightShader.SwitchShader();
            _directionalLightShader.RenderLights(directionalLight);

            _pointLightShader.SwitchShader();
            _pointLightShader.RenderLights(pointLight);

            TurnOffAlphaBlending();

            //_currentShader.RenderLights(directionalLight, pointLight, spotLight);

            _renderer.TurnZBufferOn();

            //Present
            //_renderer.SwapChain.Present(ConfigManager.VSync, PresentFlags.None);
        }

        private void TurnOnAlphaBlending()
        {
            _renderer.ImmediateContext.OutputMerger.SetBlendState(_blendState, 
               null);
        }

        private void TurnOffAlphaBlending()
        {
            _renderer.ImmediateContext.OutputMerger.SetBlendState(null);
        }

        private BlendState CreateBlendState()
        {
            RenderTargetBlendDescription rendBlendDesc = new RenderTargetBlendDescription()
            {
                IsBlendEnabled = true,
                SourceAlphaBlend = BlendOption.One,
                DestinationAlphaBlend = BlendOption.One,
                BlendOperation = BlendOperation.Add,
                SourceBlend = BlendOption.One,
                DestinationBlend = BlendOption.One,
                AlphaBlendOperation = BlendOperation.Add,
                RenderTargetWriteMask = ColorWriteMaskFlags.All
            };

            BlendStateDescription blendDesc = new BlendStateDescription()
            {
                AlphaToCoverageEnable = false,
                IndependentBlendEnable = false,
            };

            blendDesc.RenderTarget[0] = rendBlendDesc;

            return new BlendState(_renderer.Device, blendDesc);
        }

        private void SetupFrameRender()
        {
            _deferredShader.SwitchShader();

            _deferredShadingRenderer.SetRenderTargets(_renderer.ImmediateContext);
            _deferredShadingRenderer.ClearRenderTargets(_renderer.ImmediateContext, Color.Black);

            DeviceContext context;

            for (int i = 0; i < _numberOfDeferredContexts; i++)
            {
                context = _renderer.DeferredContexts[i];
                _deferredShadingRenderer.SetRenderTargets(context);
            }
        }

        public void SwitchShader()
        {
            
        }

        private void ResetFrame()
        {
            //_directionalLightShader.SwitchShader();
            _renderer.SetBackBufferRenderTarget(_renderer.DepthStencilView, _renderer.ImmediateContext);
            _renderer.ResetViewport(_renderer.ImmediateContext);

            //DeviceContext context;

            //for (int i = 0; i < _numberOfDeferredContexts; i++)
            //{
            //    context = _renderer.DeferredContexts[i];

            //    _renderer.SetBackBufferRenderTarget(context);
            //    _renderer.ResetViewport(context);
            //}
        }
    }
}
