using DungeonHack.DirectX.ConstantBuffer;
using DungeonHack.Engine;
using DungeonHack.Lights;
using FunAndGamesWithSharpDX.DirectX;
using FunAndGamesWithSharpDX.Engine;
using FunAndGamesWithSharpDX.Entities;
using SharpDX;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using System;
using Device = SharpDX.Direct3D11.Device;

namespace DungeonHack.DirectX
{
    public class PointLightShader : IDisposable
    {
        private Device _device;
        private DeviceContext _immediateContext;
        private InputLayout _layout;
        private DeferredShadingRenderer _deferredShadingRenderer;
        private Renderer _renderer;

        private SamplerState _samplerState;
        private InputElement[] _elements;
        private VertexShader _vertexShader;
        private PixelShader _pixelShader;
        private DomainShader _domainShader;
        private HullShader _hullShader;
        private Camera _camera;

        private ConstantBuffer<ConstantBufferPointLight> _pointLightConstantBuffer;
        private ConstantBuffer<ConstantBufferDeferredInfo> _deferredInfoConstantBuffer;

        private ConstantBufferPointLight _constantBufferPointLight;
        private ConstantBufferDeferredInfo _constantBufferDeferredInfo;
        private DepthStencilState DepthStencilState;
        private Texture2D _depthStencilBuffer;
        private DepthStencilViewDescription _depthStencilViewDesc;
        private bool Use4XMSAA;
        private int Width;
        private int Height;
        public DepthStencilView DepthStencilLightShader;

        public PointLightShader(Renderer renderer, Camera camera, DeferredShadingRenderer deferredShadingRenderer)
        {
            _renderer = renderer;
            _camera = camera;
            _device = renderer.Device;
            _immediateContext = renderer.ImmediateContext;
            _deferredShadingRenderer = deferredShadingRenderer;
        }

        public void Initialize()
        {
            _elements = Vertex.GetInputElements();

            Width = ConfigManager.ScreenWidth;
            Height = ConfigManager.ScreenHeight;

            var basePath = ConfigManager.ResourcePath;

            var vsShaderName = basePath + @"\Shaders\PointLightVS.hlsl";
            var psShaderName = basePath + @"\Shaders\PointLightPS.hlsl";
            var hsShaderName = basePath + @"\Shaders\PointLightHS.hlsl";
            var dsShaderName = basePath + @"\Shaders\PointLightDS.hlsl";
            
            var bytecode = ShaderBytecode.CompileFromFile(vsShaderName, "PointLightVS", "vs_5_0", ShaderFlags.Debug | ShaderFlags.SkipOptimization,
                include: FileIncludeHandler.Default);

            _vertexShader = new VertexShader(_device, bytecode);

            _layout = new InputLayout(_device, bytecode, _elements);
            bytecode.Dispose();

            bytecode = ShaderBytecode.CompileFromFile(psShaderName, "PointLightPS", "ps_5_0", ShaderFlags.Debug | ShaderFlags.SkipOptimization,
                include: FileIncludeHandler.Default);

            _pixelShader = new PixelShader(_device, bytecode);

            bytecode = ShaderBytecode.CompileFromFile(hsShaderName, "PointLightHS", "hs_5_0", ShaderFlags.Debug | ShaderFlags.SkipOptimization,
               include: FileIncludeHandler.Default);

            _hullShader = new HullShader(_device, bytecode);

            bytecode = ShaderBytecode.CompileFromFile(dsShaderName, "PointLightDS", "ds_5_0", ShaderFlags.Debug | ShaderFlags.SkipOptimization,
               include: FileIncludeHandler.Default);

            _domainShader = new DomainShader(_device, bytecode);

            bytecode.Dispose();

            _pointLightConstantBuffer = new ConstantBuffer<ConstantBufferPointLight>(_device);

            _constantBufferPointLight = new ConstantBufferPointLight();

            _deferredInfoConstantBuffer = new ConstantBuffer<ConstantBufferDeferredInfo>(_device);
            _constantBufferDeferredInfo = new ConstantBufferDeferredInfo();
            _constantBufferDeferredInfo.PerspectiveValues = new Vector4();

            SamplerStateDescription samplerDesc = CreateSamplerStateDescription();

            _samplerState = new SamplerState(_device, samplerDesc);

            _constantBufferDeferredInfo.PerspectiveValues.X = 1 / _camera.ProjectionMatrix.M11;
            _constantBufferDeferredInfo.PerspectiveValues.Y = 1 / _camera.ProjectionMatrix.M22;
            _constantBufferDeferredInfo.PerspectiveValues.Z = _camera.ProjectionMatrix.M32;
            _constantBufferDeferredInfo.PerspectiveValues.W = _camera.ProjectionMatrix.M22;

            DepthStencilLightShader = CreateDepthStencil();

            BindImmediateContext(_vertexShader, _pixelShader);
        }

        public void SwitchShader()
        {
            BindImmediateContext(_vertexShader, _pixelShader);

            _renderer.SetBackBufferRenderTarget(DepthStencilLightShader, _immediateContext);

            _renderer.ImmediateContext.ClearDepthStencilView(DepthStencilLightShader, DepthStencilClearFlags.Depth, 1.0f, 0);
        }

        private static SamplerStateDescription CreateSamplerStateDescription()
        {
            return new SamplerStateDescription
            {
                Filter = Filter.MinMagLinearMipPoint,
                AddressU = TextureAddressMode.Wrap,
                AddressV = TextureAddressMode.Wrap,
                AddressW = TextureAddressMode.Wrap,
                MipLodBias = 0.0f,
                MaximumAnisotropy = 1,
                ComparisonFunction = Comparison.Always,
                BorderColor = Colors.Black,
                MinimumLod = 0,
                MaximumLod = 0
            };
        }

        private void BindImmediateContext(VertexShader vertexShader, PixelShader pixelShader)
        {
            _immediateContext.InputAssembler.InputLayout = _layout;
            _immediateContext.InputAssembler.PrimitiveTopology = SharpDX.Direct3D.PrimitiveTopology.PatchListWith1ControlPoints;

            _immediateContext.PixelShader.SetConstantBuffer(0, _deferredInfoConstantBuffer.Buffer);
            _immediateContext.PixelShader.SetConstantBuffer(1, _pointLightConstantBuffer.Buffer);

            _immediateContext.DomainShader.SetConstantBuffer(0, _deferredInfoConstantBuffer.Buffer);
            _immediateContext.DomainShader.SetConstantBuffer(1, _pointLightConstantBuffer.Buffer);

            //_immediateContext.HullShader.SetConstantBuffer(0, _deferredInfoConstantBuffer.Buffer);
            //_immediateContext.HullShader.SetConstantBuffer(1, _pointLightConstantBuffer.Buffer);
            //_immediateContext.PixelShader.SetSampler(0, _samplerState);

            _immediateContext.HullShader.Set(_hullShader);
            _immediateContext.DomainShader.Set(_domainShader);
            _immediateContext.VertexShader.Set(vertexShader);
            _immediateContext.PixelShader.Set(pixelShader);
        }

        public void RenderLights(PointLight[] pointLight)
        {
            _immediateContext.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(null, 0, 0));
            _immediateContext.InputAssembler.SetIndexBuffer(null, SharpDX.DXGI.Format.R32_UInt, 0);

            _deferredInfoConstantBuffer.UpdateValue(_immediateContext, _constantBufferDeferredInfo);

            _immediateContext.PixelShader.SetShaderResource(0, _deferredShadingRenderer.DepthShaderResourceView);
            _immediateContext.PixelShader.SetShaderResources(1, _deferredShadingRenderer.ShaderResourceViews);

            var translationViewMatrix = new Matrix()
            {
                M11 = 1.0f,
                M22 = 1.0f,
                M33 = 1.0f,
                M44 = 1.0f,
                M41 = _camera.ViewMatrix.M41,
                M42 = _camera.ViewMatrix.M42,
                M43 = _camera.ViewMatrix.M43
            };

            for (int i = 0; i < pointLight.Length; i++)
            {
                var rangeMatrix = new Matrix()
                {
                    M11 = pointLight[i].Range * 1.1f,
                    M22 = pointLight[i].Range * 1.1f,
                    M33 = pointLight[i].Range * 1.1f,
                    M44 = 1.0f
                };

                var translationMatrix = new Matrix()
                {
                    M11 = 1.0f,
                    M22 = 1.0f,
                    M33 = 1.0f,
                    M44 = 1.0f,
                    M41 = pointLight[i].Position.X,
                    M42 = pointLight[i].Position.Y,
                    M43 = pointLight[i].Position.Z
                };

                pointLight[i].LightCalculations = rangeMatrix * translationMatrix * translationViewMatrix * _camera.ProjectionMatrix;

                _constantBufferPointLight.PointLight = pointLight[i];

                _pointLightConstantBuffer.UpdateValue(_immediateContext, _constantBufferPointLight);

                _constantBufferDeferredInfo.ViewInv = Matrix.Invert(_camera.ViewMatrix);

                _immediateContext.Draw(4, 0);
            }
        }

        private DepthStencilView CreateDepthStencil()
        {
            var sampleDesc = Use4XMSAA ?
                                       new SampleDescription(4, _device.CheckMultisampleQualityLevels(Format.R8G8B8A8_UNorm, 4))
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

            _depthStencilBuffer = new Texture2D(_device, depthStencilDesc);

            var dsStateDesc = new DepthStencilStateDescription()
            {
                IsDepthEnabled = true,
                DepthWriteMask = DepthWriteMask.Zero,
                DepthComparison = Comparison.GreaterEqual,
            };

            DepthStencilState = new DepthStencilState(_device, dsStateDesc);

            _depthStencilBuffer = new Texture2D(_device, depthStencilDesc);

            _depthStencilViewDesc = new DepthStencilViewDescription()
            {
                Dimension = DepthStencilViewDimension.Texture2D
            };

            return new DepthStencilView(_device, _depthStencilBuffer, _depthStencilViewDesc);
        }

        public void Dispose()
        {
            _layout?.Dispose();
            _samplerState?.Dispose();
            _samplerState?.Dispose();
            _pointLightConstantBuffer?.Dispose();
            _vertexShader?.Dispose();
            _pixelShader?.Dispose();
            _depthStencilBuffer?.Dispose();
            DepthStencilLightShader?.Dispose();
        }

    }
}
