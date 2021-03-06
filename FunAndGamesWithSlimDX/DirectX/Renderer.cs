﻿using FunAndGamesWithSlimDX.Engine;
using SlimDX;
using SlimDX.Direct3D11;
using SlimDX.DXGI;
using System;
using Device = SlimDX.Direct3D11.Device;
using Resource = SlimDX.Direct3D11.Resource;

namespace FunAndGamesWithSlimDX.DirectX
{
    /// <summary>
    /// Responsible for creating DirectX stuff and rendering stuff.
    /// </summary>
    public class Renderer : IDisposable
    {
        public RenderTargetView RenderTarget;
        public SwapChain SwapChain;
        public Device Device;
        public DeviceContext Context;
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
            Context.OutputMerger.DepthStencilState = _depthStencilDisabledState;
        }

        public void TurnZBufferOn()
        {
            Context.OutputMerger.DepthStencilState = _depthStencilState;
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

            Device.CreateWithSwapChain(DriverType.Hardware, DeviceCreationFlags.Debug, description, out Device, out SwapChain);

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

            SetRasterizerState(FillMode.Solid, CullMode.Back);

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
                    ArraySize = 0,
                    Dimension = DepthStencilViewDimension.Texture2D,
                    MipSlice = 0,
                    Flags = 0,
                    FirstArraySlice = 0
                };

            DepthStencilView = new DepthStencilView(Device, _depthStencilBuffer, _depthStencilViewDesc);

            var dsStateDesc = new DepthStencilStateDescription()
            {
                IsDepthEnabled = true,
                IsStencilEnabled = false,
                DepthWriteMask = DepthWriteMask.All,
                DepthComparison = Comparison.Less,
            };

            _depthStencilState = DepthStencilState.FromDescription(Device, dsStateDesc);

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
                ArraySize = 0,
                Dimension = DepthStencilViewDimension.Texture2D,
                MipSlice = 0,
                Flags = 0,
                FirstArraySlice = 0
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
                IsDepthEnabled = false,
                IsStencilEnabled = true,
                DepthWriteMask = DepthWriteMask.All,
                DepthComparison = Comparison.Less,
                StencilReadMask = 0xff,
                StencilWriteMask = 0xff,
                FrontFace = frontFace,
                BackFace = backFace
            };

            _depthStencilDisabledState = DepthStencilState.FromDescription(Device, dsStateDisabledDesc);

            Context = Device.ImmediateContext;
            var viewport = new Viewport(0.0f, 0.0f, Width, Height, 0.0f, 1.0f);

            Context.OutputMerger.DepthStencilState = _depthStencilState;
            Context.OutputMerger.SetTargets(DepthStencilView, RenderTarget);
            //Context.OutputMerger.SetTargets(RenderTarget);
            Context.Rasterizer.SetViewports(viewport);
            
            //SetRenderState();
            
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
                    IsFrontCounterclockwise = false,
                    IsMultisampleEnabled = ConfigManager.MultiSampleEnabled,
                    IsScissorEnabled = false,
                    SlopeScaledDepthBias = 0.0f
                };

            RasterizerState rs = RasterizerState.FromDescription(Device, rsd);
            Device.ImmediateContext.Rasterizer.State = rs;
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
