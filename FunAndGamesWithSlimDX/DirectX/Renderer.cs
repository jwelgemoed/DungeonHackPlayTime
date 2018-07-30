using FunAndGamesWithSharpDX.Engine;
using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpDX.Mathematics.Interop;
using System;
using DungeonHack.Engine;
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
        public DeviceContext ImmediateContext;
        public DeviceContext[] DeferredContexts;
        private int _maxNumberOfThreads = 4;
        public CommandList[] CommandLists;
        private Texture2D _depthStencilBuffer;
        public DepthStencilView DepthStencilView;
        private DepthStencilViewDescription _depthStencilViewDesc;
        private DepthStencilState _depthStencilState;
        private DepthStencilState _depthStencilDisabledState;

        private Matrix? _worldMatrix;
        private Matrix? _projectionMatrix;
        private Matrix? _orthMatrix;

        public Matrix ProjectionMatrix
        {
            get
            {
                if (!_projectionMatrix.HasValue)
                {
                    const float fieldOfView = (float) Math.PI/4.0f;
                    _projectionMatrix = Matrix.PerspectiveFovLH(fieldOfView, AspectRatio(), ScreenNear, ScreenFar);
                }

                return _projectionMatrix.Value;
            }
        }

        public Matrix WorldMatrix
        {
            get
            {
                if (!_worldMatrix.HasValue)
                {
                    _worldMatrix = Matrix.Identity;
                }

                return _worldMatrix.Value;
            }
        }

        public Matrix OrthoMatrix
        {
            get
            {
                if (!_orthMatrix.HasValue)
                {
                    _orthMatrix = Matrix.OrthoLH(Width, Height, ScreenNear, ScreenFar);
                }

                return _orthMatrix.Value;
            }
        }
        
        public static int Width { get; set; }
        public static int Height { get; set; }
        public static bool Use4XMSAA { get; set; }
        public static bool FullScreen { get; set; }

        public static float ScreenNear { get; set; }
        public static float ScreenFar { get; set; }

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
        }

        public void TurnZBufferOn()
        {
            ImmediateContext.OutputMerger.DepthStencilState = _depthStencilState;
        }

        public void Initialize(IntPtr formHandle)
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

            Device.CreateWithSwapChain(SharpDX.Direct3D.DriverType.Hardware, DeviceCreationFlags.Debug, description, out Device, out SwapChain);

            OnResize();
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
            
            using (var resource = Resource.FromSwapChain<Texture2D>(SwapChain, 0))
                RenderTarget = new RenderTargetView(Device, resource);

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

            _depthStencilViewDesc = new DepthStencilViewDescription()
                {
                    Dimension = DepthStencilViewDimension.Texture2D
                };

            DepthStencilView = new DepthStencilView(Device, _depthStencilBuffer, _depthStencilViewDesc);

            var dsStateDesc = new DepthStencilStateDescription()
            {
                IsDepthEnabled = true,
                IsStencilEnabled = false,
                DepthWriteMask = DepthWriteMask.All,
                DepthComparison = Comparison.Less,
            };

            _depthStencilState = new DepthStencilState(Device, dsStateDesc);

            //Create the depth stencil with zbuffering disabled.
            var depthStencilDisabledDesc = new Texture2DDescription()
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

            _depthStencilViewDesc = new DepthStencilViewDescription()
            {
                Dimension = DepthStencilViewDimension.Texture2D
            };

            DepthStencilView = new DepthStencilView(Device, _depthStencilBuffer, _depthStencilViewDesc);
            var frontFace = new DepthStencilOperationDescription()
            {
                DepthFailOperation = StencilOperation.Increment,
                FailOperation = StencilOperation.Keep,
                PassOperation = StencilOperation.Keep,
                Comparison = Comparison.Always
            };

            var backFace = new DepthStencilOperationDescription()
            {
                FailOperation = StencilOperation.Keep,
                DepthFailOperation = StencilOperation.Decrement,
                PassOperation = StencilOperation.Keep,
                Comparison = Comparison.Always
            };

            var dsStateDisabledDesc = new DepthStencilStateDescription()
            {
                IsDepthEnabled = true,
                IsStencilEnabled = true,
                DepthWriteMask = DepthWriteMask.All,
                DepthComparison = Comparison.Less,
                StencilReadMask = 0xff,
                StencilWriteMask = 0xff,
                FrontFace = frontFace,
                BackFace = backFace
            };

            _depthStencilDisabledState = new DepthStencilState(Device, dsStateDisabledDesc);

            ImmediateContext = Device.ImmediateContext;
            var viewport = new RawViewportF()
            {
                X = 0,
                Y = 0,
                Width = Width,
                Height = Height,
                MinDepth = 0.0f,
                MaxDepth = 1.0f
            };

            ImmediateContext.OutputMerger.DepthStencilState = _depthStencilState;
            ImmediateContext.OutputMerger.SetTargets(DepthStencilView, RenderTarget);
            //Context.OutputMerger.SetTargets(RenderTarget);
            ImmediateContext.Rasterizer.SetViewports(new[] { viewport });

            //SetRenderState();

            DeferredContexts = new DeviceContext[_maxNumberOfThreads];
            CommandLists = new CommandList[_maxNumberOfThreads];

            for (int i=0; i<_maxNumberOfThreads; i++)
            {
                DeferredContexts[i] = new DeviceContext(Device);
                DeferredContexts[i].OutputMerger.DepthStencilState = _depthStencilState;
                DeferredContexts[i].OutputMerger.SetTargets(DepthStencilView, RenderTarget);
                DeferredContexts[i].Rasterizer.SetViewports(new[] { viewport });
            }

            SetRasterizerState(FillMode.Solid, CullMode.Back);
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
            //Device.ImmediateContext.Rasterizer.State = rs;
        }

        public void SetupOcclusionQuery()
        {
            SharpDX.Direct3D11.BlendStateDescription blendDescription = new BlendStateDescription();
            blendDescription.RenderTarget[0] = new RenderTargetBlendDescription(true, BlendOption.SourceAlpha, BlendOption.InverseSourceAlpha,
                BlendOperation.Add, BlendOption.One, BlendOption.Zero, BlendOperation.Add, ColorWriteMaskFlags.All);



        }


        public void Dispose()
        {
            _depthStencilBuffer.Dispose();
            _depthStencilDisabledState.Dispose();
            
            DepthStencilView.Dispose();
           
            RenderTarget.Dispose();
            //Always ensure we are windows before releasing the swap chain.
            SwapChain.IsFullScreen = false;
            SwapChain.Dispose();
            Device.Dispose();
        }
    }
}
