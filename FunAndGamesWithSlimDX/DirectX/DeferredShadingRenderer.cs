using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpDX.Mathematics.Interop;
using System;

namespace DungeonHack.DirectX
{
    public class DeferredShadingRenderer : IDisposable
    {
        public SharpDX.Direct3D11.Device Device;
        public static int Width { get; set; }
        public static int Height { get; set; }
        public static bool Use4XMSAA { get; set; }

        public DepthStencilView DepthStencilView;
        private DepthStencilViewDescription _depthStencilViewDesc;
        private Texture2D _depthStencilBuffer;
        private DepthStencilState _depthStencilState;
        private RawViewportF viewport;

        //Deferred Shading Buffers
        public RenderTargetView[] RenderTargets;
        public Texture2D[] RenderTargetBuffers;
        public ShaderResourceView[] ShaderResourceViews;

        private int _numberOfBuffers = 3;

        public void Initialize()
        {
            DepthStencilView = CreatDepthStencil();

            CreateDeferredShadingRenderTargetsAndBuffers();

            viewport = CreateViewPort();
        }

        public void SetRenderTargets(DeviceContext context)
        {
            context.OutputMerger.SetTargets(DepthStencilView, RenderTargets);
            context.Rasterizer.SetViewport(viewport);
        }

        public void ClearRenderTargets(DeviceContext context, Color color)
        {

            for (int i=0; i<_numberOfBuffers; i++)
            {
                context.ClearRenderTargetView(RenderTargets[i], color);
            }

            context.ClearDepthStencilView(DepthStencilView, DepthStencilClearFlags.Depth, 1.0f, 0);
        }

        public ShaderResourceView GetShaderResourcesView(int view)
        {
            return ShaderResourceViews[view];
        }

        private DepthStencilView CreatDepthStencil()
        {
            var sampleDesc = Use4XMSAA ?
                                       new SampleDescription(4, Device.CheckMultisampleQualityLevels(Format.R8G8B8A8_UNorm, 4))
                                       : new SampleDescription(1, 0);

            //Create a depth stencil
            var depthStencilDesc = new Texture2DDescription()
            {
                Width = Width,
                Height = Height,
                MipLevels = 1,
                ArraySize = 1,
                Format = Format.D32_Float,
                SampleDescription = sampleDesc,
                Usage = ResourceUsage.Default,
                BindFlags = BindFlags.DepthStencil,
                CpuAccessFlags = CpuAccessFlags.None,
            };

            _depthStencilBuffer = new Texture2D(Device, depthStencilDesc);

            var dsStateDesc = new DepthStencilStateDescription()
            {
                IsDepthEnabled = true,
                IsStencilEnabled = false,
                DepthWriteMask = DepthWriteMask.All,
                DepthComparison = Comparison.Less,
            };

            _depthStencilState = new DepthStencilState(Device, dsStateDesc);

            _depthStencilBuffer = new Texture2D(Device, depthStencilDesc);

            _depthStencilViewDesc = new DepthStencilViewDescription()
            {
                Dimension = DepthStencilViewDimension.Texture2D
            };

            return new DepthStencilView(Device, _depthStencilBuffer, _depthStencilViewDesc);
        }

        private void CreateDeferredShadingRenderTargetsAndBuffers()
        {
            var renderTargets = new RenderTargetView[_numberOfBuffers];
            var renderBuffers = new Texture2D[_numberOfBuffers];
            var renderShaderViews = new ShaderResourceView[_numberOfBuffers];

            var textureDescription = new Texture2DDescription()
            {
                Width = Width,
                Height = Height,
                MipLevels = 1,
                ArraySize = 1,
                Format = Format.R8G8B8A8_UNorm,
                SampleDescription = new SampleDescription() { Count = 1 },
                BindFlags = BindFlags.RenderTarget | BindFlags.ShaderResource,
                CpuAccessFlags = 0,
                Usage = ResourceUsage.Default,
                OptionFlags = ResourceOptionFlags.None
            };

            renderBuffers[0] = new Texture2D(Device, textureDescription);
            textureDescription.Format = Format.R11G11B10_Float;
            renderBuffers[1] = new Texture2D(Device, textureDescription);
            textureDescription.Format = Format.R8G8B8A8_UNorm;
            renderBuffers[2] = new Texture2D(Device, textureDescription);

            var renderTargetViewDescription = new RenderTargetViewDescription()
            {
                Format = textureDescription.Format,
                Dimension = RenderTargetViewDimension.Texture2D,
                Texture2D = new RenderTargetViewDescription.Texture2DResource() { MipSlice = 0 }
            };

            for (int i = 0; i < _numberOfBuffers; i++)
            {
                renderTargetViewDescription.Format = renderBuffers[i].Description.Format;
                renderTargets[i] = new RenderTargetView(Device, renderBuffers[i], renderTargetViewDescription);
            }

            var shaderResourceViewDescription = new ShaderResourceViewDescription()
            {
                Format = textureDescription.Format,
                Dimension = SharpDX.Direct3D.ShaderResourceViewDimension.Texture2D,
                Texture2D = new ShaderResourceViewDescription.Texture2DResource()
                {
                    MipLevels = 1,
                    MostDetailedMip = 0
                }
            };

            for (int i = 0; i < _numberOfBuffers; i++)
            {
                shaderResourceViewDescription.Format = renderBuffers[i].Description.Format;
                renderShaderViews[i] = new ShaderResourceView(Device, renderBuffers[i], shaderResourceViewDescription);
            }

            RenderTargetBuffers = renderBuffers;
            RenderTargets = renderTargets;
            ShaderResourceViews = renderShaderViews;
        }

        private static RawViewportF CreateViewPort()
        {
            return new RawViewportF()
            {
                X = 0,
                Y = 0,
                Width = Width,
                Height = Height,
                MinDepth = 0.0f,
                MaxDepth = 1.0f
            };
        }

        public void Dispose()
        {
            _depthStencilBuffer?.Dispose();

            DepthStencilView?.Dispose();

            for (int i = 0; i < _numberOfBuffers; i++)
            {
                ShaderResourceViews[i]?.Dispose();
                RenderTargets[i]?.Dispose();
                RenderTargetBuffers[i]?.Dispose();
            }

            Device?.Dispose();
        }
    }
}
