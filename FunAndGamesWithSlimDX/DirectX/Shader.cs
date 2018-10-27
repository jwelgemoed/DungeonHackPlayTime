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
        private LightShader _lightShader;
        private TextureShader _textureShader;
        private IShader _currentShader;

        private int _numberOfDeferredContexts;

        public void Dispose()
        {
            _lightShader?.Dispose();

            _textureShader?.Dispose();
        }

        public Shader(Renderer renderer, DeferredShadingRenderer deferredRenderer, Camera camera)
        {
            _renderer = renderer;
            _deferredShadingRenderer = deferredRenderer;
            _textureShader = new TextureShader(renderer);
            _lightShader = new LightShader(renderer, camera, deferredRenderer);
            _deferredShader = new DeferredShader(renderer);
            _numberOfDeferredContexts = _renderer.DeferredContexts.Length;

            _deferredShader.Initialize();
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
            SetupFrameRender();

            _deferredShader.RenderFrame(camera);
        }

        public void Render(int threadNumber, int indexCount, Matrix worldMatrix, Matrix viewMatrix, Matrix viewProjMatrix, Texture texture, Vector3 cameraPosition, Material material)
        {
            _deferredShader.Render(threadNumber, indexCount, worldMatrix, viewMatrix, viewProjMatrix, texture, cameraPosition, material);
        }

        public void RenderLights(DirectionalLight[] directionalLight, PointLight[] pointLight, Spotlight[] spotLight)
        {
            ResetFrame();

            _renderer.ImmediateContext.ClearRenderTargetView(_renderer.RenderTarget, Colors.Black);
            _renderer.ImmediateContext.ClearDepthStencilView(_lightShader.DepthStencilLightShader, DepthStencilClearFlags.Depth, 1.0f, 0);

            _renderer.TurnZBufferOff();

            //Do render fullscreen quad?

            _currentShader.RenderLights(directionalLight, pointLight, spotLight);

            _renderer.TurnZBufferOn();

            //Present
            //_renderer.SwapChain.Present(ConfigManager.VSync, PresentFlags.None);
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
                _deferredShadingRenderer.ClearRenderTargets(context, Color.Black);
            }
        }

        public void SwitchShader()
        {
            
        }

        private void ResetFrame()
        {
            _currentShader.SwitchShader();
            _renderer.SetBackBufferRenderTarget(_lightShader.DepthStencilLightShader, _renderer.ImmediateContext);
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
