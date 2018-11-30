﻿using DungeonHack.DirectX.ConstantBuffer;
using DungeonHack.Engine;
using DungeonHack.Lights;
using FunAndGamesWithSharpDX.DirectX;
using FunAndGamesWithSharpDX.Engine;
using FunAndGamesWithSharpDX.Entities;
using SharpDX;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using Device = SharpDX.Direct3D11.Device;

namespace DungeonHack.DirectX
{
    public class LightShader : IShader
    {
        private Device _device;
        private DeviceContext _immediateContext;
        private InputLayout _layout;
        private DeferredShadingRenderer _deferredShadingRenderer;

        private SamplerState _samplerState;
        private InputElement[] _elements;
        private VertexShader _vertexShader;
        private PixelShader _pixelShader;
        private DomainShader _domainShader;
        private HullShader _hullShader;
        private Camera _camera;

        private ConstantBuffer<ConstantBufferPerFrame> _frameConstantBuffer;
        private ConstantBuffer<ConstantBufferDeferredInfo> _deferredInfoConstantBuffer;

        private ConstantBufferPerFrame _constantBufferPerFrame;
        private ConstantBufferDeferredInfo _constantBufferDeferredInfo;
        private DepthStencilState DepthStencilState;
        private Texture2D _depthStencilBuffer;
        private DepthStencilViewDescription _depthStencilViewDesc;
        private bool Use4XMSAA;
        private int Width;
        private int Height;
        public DepthStencilView DepthStencilLightShader;

        public LightShader(Renderer renderer, Camera camera, DeferredShadingRenderer deferredShadingRenderer)
        {
            _camera = camera;
            _device = renderer.Device;
            _immediateContext = renderer.ImmediateContext;
            _deferredShadingRenderer = deferredShadingRenderer;
        }

        public void Initialize()
        {
            _elements = Vertex.GetInputElements();

            var basePath = ConfigManager.ResourcePath;

            var vsShaderName = basePath + @"\Shaders\LightTextureVS.hlsl";
            var psShaderName = basePath + @"\Shaders\LightTexturePS.hlsl";
            var hsShaderName = basePath + @"\Shaders\LightTextureHS.hlsl";
            var dsShaderName = basePath + @"\Shaders\LightTextureDS.hlsl";
            
            var bytecode = ShaderBytecode.CompileFromFile(vsShaderName, "LightVertexShader", "vs_5_0", ShaderFlags.Debug | ShaderFlags.SkipOptimization,
                include: FileIncludeHandler.Default);

            _vertexShader = new VertexShader(_device, bytecode);

            _layout = new InputLayout(_device, bytecode, _elements);
            bytecode.Dispose();

            bytecode = ShaderBytecode.CompileFromFile(psShaderName, "LightPixelShader", "ps_5_0", ShaderFlags.Debug | ShaderFlags.SkipOptimization,
                include: FileIncludeHandler.Default);

            _pixelShader = new PixelShader(_device, bytecode);

            bytecode = ShaderBytecode.CompileFromFile(hsShaderName, "PointLightHS", "ps_5_0", ShaderFlags.Debug | ShaderFlags.SkipOptimization,
               include: FileIncludeHandler.Default);

            _hullShader = new HullShader(_device, bytecode);

            bytecode = ShaderBytecode.CompileFromFile(dsShaderName, "PointLightDS", "ps_5_0", ShaderFlags.Debug | ShaderFlags.SkipOptimization,
               include: FileIncludeHandler.Default);

            _domainShader = new DomainShader(_device, bytecode);

            bytecode.Dispose();

            _frameConstantBuffer = new ConstantBuffer<ConstantBufferPerFrame>(_device);

            _constantBufferPerFrame = new ConstantBufferPerFrame();

            _deferredInfoConstantBuffer = new ConstantBuffer<ConstantBufferDeferredInfo>(_device);
            _constantBufferDeferredInfo = new ConstantBufferDeferredInfo();
            _constantBufferDeferredInfo.PerspectiveValues = new Vector4();

            SamplerStateDescription samplerDesc = CreateSamplerStateDescription();

            _samplerState = new SamplerState(_device, samplerDesc);

            _constantBufferPerFrame.gMaxTessDistance = 100;
            _constantBufferPerFrame.gMinTessDistance = 250;
            _constantBufferPerFrame.gMinTessFactor = 3;
            _constantBufferPerFrame.gMaxTessFactor = 27;

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
            _immediateContext.PixelShader.SetConstantBuffer(1, _frameConstantBuffer.Buffer);
            //_immediateContext.PixelShader.SetSampler(0, _samplerState);

            _immediateContext.VertexShader.Set(vertexShader);
            _immediateContext.PixelShader.Set(pixelShader);
        }

        public void RenderLights(DirectionalLight[] directionalLight, PointLight[] pointLight, Spotlight[] spotLight)
        {
            _immediateContext.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(null, 0, 0));
            _immediateContext.InputAssembler.SetIndexBuffer(null, SharpDX.DXGI.Format.R32_UInt, 0);

            CreatePointLightTransformMatrix(ref pointLight);

            _constantBufferPerFrame.DirectionalLight = directionalLight;
            _constantBufferPerFrame.PointLight = pointLight;
            _constantBufferPerFrame.SpotLight = spotLight;
            _constantBufferPerFrame.FogStart = ConfigManager.FogStart;
            _constantBufferPerFrame.FogEnd = ConfigManager.FogEnd;
            _constantBufferPerFrame.UseNormalMap = ConfigManager.UseNormalMap;

            _frameConstantBuffer.UpdateValue(_immediateContext, _constantBufferPerFrame);

            _constantBufferDeferredInfo.ViewInv = Matrix.Invert(_camera.ViewMatrix);

            _deferredInfoConstantBuffer.UpdateValue(_immediateContext, _constantBufferDeferredInfo);

            _immediateContext.PixelShader.SetShaderResource(0, _deferredShadingRenderer.DepthShaderResourceView);
            _immediateContext.PixelShader.SetShaderResources(1, _deferredShadingRenderer.ShaderResourceViews);

            _immediateContext.Draw(4, 0);
        }

        private void CreatePointLightTransformMatrix(ref PointLight[] pointlights)
        {
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

            for (int i=0; i<pointlights.Length; i++)
            {
                var rangeMatrix = new Matrix()
                {
                    M11 = pointlights[i].Range,
                    M22 = pointlights[i].Range,
                    M33 = pointlights[i].Range,
                    M44 = 1.0f
                };

                var translationMatrix = new Matrix()
                {
                    M11 = 1.0f,
                    M22 = 1.0f,
                    M33 = 1.0f,
                    M44 = 1.0f,
                    M41 = pointlights[i].Position.X,
                    M42 = pointlights[i].Position.Y,
                    M43 = pointlights[i].Position.Z
                };
                                
                pointlights[i].LightCalculations = rangeMatrix * translationMatrix * translationViewMatrix * _camera.ProjectionMatrix;
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
                IsStencilEnabled = false,
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
            _frameConstantBuffer?.Dispose();
            _vertexShader?.Dispose();
            _pixelShader?.Dispose();
            _depthStencilBuffer?.Dispose();
            DepthStencilLightShader?.Dispose();
        }

        public void Render(int threadNumber, int indexCount, Matrix worldMatrix, Matrix viewMatrix, Matrix viewProjectionMatrix, Texture texture, Vector3 cameraPosition, Material material)
        {
            throw new System.NotImplementedException();
        }

        public void RenderFrame(Camera camera)
        {
            throw new System.NotImplementedException();
        }
    }
}
