using DungeonHack.Engine;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpDX.Mathematics.Interop;
using System;
using Device = SharpDX.Direct3D11.Device;
using Resource = SharpDX.Direct3D11.Resource;

namespace FunAndGamesWithSharpDX.DirectX
{
    /// <summary>
    /// Responsible for creating DirectX stuff and rendering stuff.
    /// </summary>
    public class Renderer : IDisposable
    {
        public RenderTargetView RenderTarget;
        public SwapChain SwapChain;
        public Device Device;
        public SharpDX.DXGI.Device DXGIDevice;
        public SharpDX.DXGI.SwapChain DXGISwapChain;
        public DeviceContext ImmediateContext;
        public DeviceContext[] DeferredContexts;
        private int _numberOfRenderingThreads;
        public CommandList[] CommandLists;
        private Texture2D _depthStencilBuffer;
        public DepthStencilView DepthStencilView;
        private DepthStencilViewDescription _depthStencilViewDesc;
        private DepthStencilState _depthStencilState;
        private DepthStencilState _depthStencilDisabledState;

        private RawViewportF _viewport;

        ////Deferred Shading Buffers
        //public RenderTargetView[] RenderTargets;
        //public Texture2D[] RenderTargetBuffers;
        //public ShaderResourceView[] ShaderResourceViews;

        public static int Width { get; set; }
        public static int Height { get; set; }
        public static bool Use4XMSAA { get; set; }
        public static bool FullScreen { get; set; }

        public static float ScreenNear { get; set; }
        public static float ScreenFar { get; set; }

        //private int _numberOfBuffers = 3;

        public bool Check4XMSAAQualitySupport()
        {
            if (Device == null)
                return false;

            return Device.CheckMultisampleQualityLevels(Format.R8G8B8A8_UNorm, 4) > 0;
        }

        public float AspectRatio()
        {
            return (float) Width/Height;
        }

        public void TurnZBufferOff()
        {
            ImmediateContext.OutputMerger.DepthStencilState = _depthStencilDisabledState;

            for (int i=0; i<DeferredContexts.Length; i++)
            {
                DeferredContexts[i].OutputMerger.DepthStencilState = _depthStencilDisabledState;
            }
        }

        public void TurnZBufferOn()
        {
            ImmediateContext.OutputMerger.DepthStencilState = _depthStencilState;

            for (int i = 0; i < DeferredContexts.Length; i++)
            {
                DeferredContexts[i].OutputMerger.DepthStencilState = _depthStencilState;
            }
        }

        public void Initialize(IntPtr formHandle, int numberOfRenderingThreads)
        {
            _numberOfRenderingThreads = numberOfRenderingThreads;

            CreateSwapChainDevice(formHandle);

            OnResize();

            DXGIDevice = Device.QueryInterface<SharpDX.DXGI.Device>();
            DXGISwapChain = SwapChain.QueryInterface<SharpDX.DXGI.SwapChain1>();
        }

        private void CreateSwapChainDevice(IntPtr formHandle)
        {
            var sampleDesc = new SampleDescription(1, 0);

            var description = new SwapChainDescription()
            {
                BufferCount = 2,
                Usage = Usage.RenderTargetOutput,
                OutputHandle = formHandle,
                IsWindowed = !FullScreen,
                ModeDescription = new ModeDescription(Width, Height, new Rational(60, 1), Format.R8G8B8A8_UNorm),
                SampleDescription = sampleDesc,
                Flags = SwapChainFlags.AllowModeSwitch,
                SwapEffect = SwapEffect.Discard,
            };

            SharpDX.DXGI.Factory f = new SharpDX.DXGI.Factory1();

            var adapters = f.Adapters;

            Device.CreateWithSwapChain(adapters[1], DeviceCreationFlags.Debug | DeviceCreationFlags.BgraSupport
                , description, out Device, out SwapChain);
        }

        public void OnResize()
        {
            if (RenderTarget != null)
                RenderTarget.Dispose();

            if (DepthStencilView != null)
                DepthStencilView.Dispose();

            if (_depthStencilBuffer != null)
                _depthStencilBuffer.Dispose();

            SwapChain.ResizeBuffers(1, Width, Height, Format.R8G8B8A8_UNorm, 0);

            RenderTarget = CreateRenderTarget();
            DepthStencilView = CreatDepthStencil();

            DepthStencilOperationDescription frontFace = CreateFrontFaceDepthStencilDescription();
            DepthStencilOperationDescription backFace = CreateBackFaceDepthStencilDescription();

            _depthStencilDisabledState = CreateDisableDepthStencil(frontFace, backFace);

            _viewport = CreateViewPort();

            BindImmediateContext();

            //SetRenderState();

            CreateDeferredContexts();

            SetRasterizerState(FillMode.Solid, CullMode.Back);
        }

        public void SetBackBufferRenderTarget(DeviceContext context)
        {
            context.OutputMerger.SetRenderTargets(DepthStencilView, RenderTarget);
        }

        public void SetBackBufferRenderTarget(DepthStencilView depthStencilView, DeviceContext context)
        {
            context.OutputMerger.SetRenderTargets(DepthStencilView, RenderTarget);
        }

        public void ResetViewport(DeviceContext context)
        {
            context.Rasterizer.SetViewports(new[] { _viewport });
        }

        private void CreateDeferredContexts()
        {
            DeferredContexts = new DeviceContext[_numberOfRenderingThreads];
            CommandLists = new CommandList[_numberOfRenderingThreads];

            for (int i = 0; i < _numberOfRenderingThreads; i++)
            {
                DeferredContexts[i] = new DeviceContext(Device);
                DeferredContexts[i].OutputMerger.DepthStencilState = _depthStencilState;
                DeferredContexts[i].OutputMerger.SetTargets(DepthStencilView, RenderTarget);
                DeferredContexts[i].Rasterizer.SetViewports(new[] { _viewport });
            }
        }

        private void BindImmediateContext()
        {
            ImmediateContext = Device.ImmediateContext;
            ImmediateContext.OutputMerger.DepthStencilState = _depthStencilState;
            ImmediateContext.OutputMerger.SetTargets(DepthStencilView, RenderTarget);
            ImmediateContext.Rasterizer.SetViewports(new[] { _viewport });
        }

        private static DepthStencilOperationDescription CreateBackFaceDepthStencilDescription()
        {
            return new DepthStencilOperationDescription()
            {
                FailOperation = StencilOperation.Keep,
                DepthFailOperation = StencilOperation.Decrement,
                PassOperation = StencilOperation.Keep,
                Comparison = Comparison.Always
            };
        }

        private static DepthStencilOperationDescription CreateFrontFaceDepthStencilDescription()
        {
            return new DepthStencilOperationDescription()
            {
                DepthFailOperation = StencilOperation.Increment,
                FailOperation = StencilOperation.Keep,
                PassOperation = StencilOperation.Keep,
                Comparison = Comparison.Always
            };
        }

        private RenderTargetView CreateRenderTarget()
        {
            using (var resource = Resource.FromSwapChain<Texture2D>(SwapChain, 0))
                return new RenderTargetView(Device, resource);
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

        private DepthStencilState CreateDisableDepthStencil(DepthStencilOperationDescription frontFace, DepthStencilOperationDescription backFace)
        {
            var dsStateDisabledDesc = new DepthStencilStateDescription()
            {
                IsDepthEnabled = false,
                IsStencilEnabled = true,
                DepthWriteMask = DepthWriteMask.All,
                DepthComparison = Comparison.Less,
                StencilReadMask = 0xff,
                StencilWriteMask = 0xff,
                FrontFace = frontFace,
                BackFace = backFace
            };

            return new DepthStencilState(Device, dsStateDisabledDesc);
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

        public void SetRasterizerState(FillMode fillMode, CullMode cullMode)
        {
            RasterizerStateDescription rsd = new RasterizerStateDescription()
                {
                    CullMode = cullMode,
                    DepthBias = 0,
                    DepthBiasClamp = 0.0f,
                    FillMode = fillMode,
                    IsAntialiasedLineEnabled = ConfigManager.AntiAliasedEnabled,
                    IsDepthClipEnabled = false,
                    IsFrontCounterClockwise = false,
                    IsMultisampleEnabled = ConfigManager.MultiSampleEnabled,
                    IsScissorEnabled = false,
                    SlopeScaledDepthBias = 0.0f
                };

            RasterizerState rs = new RasterizerState(Device, rsd);

            for (int i=0; i<DeferredContexts.Length; i++)
            {
                DeferredContexts[i].Rasterizer.State = rs;
            }
        }

        public void Dispose()
        {
            _depthStencilBuffer?.Dispose();
            _depthStencilDisabledState?.Dispose();
            
            DepthStencilView?.Dispose();
           
            RenderTarget?.Dispose();

            SwapChain.IsFullScreen = false;
            SwapChain?.Dispose();
            Device?.Dispose();
        }
    }
}
