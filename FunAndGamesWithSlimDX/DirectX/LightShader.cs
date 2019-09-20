using DungeonHack.DirectX.ConstantBuffer;
using DungeonHack.Engine;
using DungeonHack.Lights;
using FunAndGamesWithSharpDX.DirectX;
using FunAndGamesWithSharpDX.Engine;
using FunAndGamesWithSharpDX.Entities;
using SharpDX;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D11;
using Device = SharpDX.Direct3D11.Device;

namespace DungeonHack.DirectX
{
    public class LightShader : IShader
    {
        private Device _device;
        private DeviceContext _immediateContext;
        private DeviceContext[] _deferredContexts;
        private InputLayout _layout;

        private SamplerState _samplerState;
        private SamplerState _normalMapSamplerState;
        private SamplerState _displacementSamplerState;
        private InputElement[] _elements;

        private ConstantBuffer<ConstantBufferPerObject> _objectConstantBuffer;
        private ConstantBuffer<ConstantBufferPerFrame> _frameConstantBuffer;

        private ConstantBufferPerFrame _constantBufferPerFrame;
        private ConstantBufferPerObject _constantBufferPerObject;
        private SamplerState _specularMapSamplerState;

        private object _lock = new object();

        public LightShader(Renderer renderer)
        {
            _device = renderer.Device;
            _immediateContext = renderer.ImmediateContext;
            _deferredContexts = renderer.DeferredContexts;
        }

        public void Initialize()
        {
            _elements = Vertex.GetInputElements();

            var basePath = ConfigManager.ResourcePath;

            var fileName = basePath + @"\Shaders\LightTexture.hlsl";

            var bytecode = ShaderBytecode.CompileFromFile(fileName, "LightVertexShader", "vs_5_0", ShaderFlags.Debug | ShaderFlags.SkipOptimization);
            var vertexShader = new VertexShader(_device, bytecode);

            _layout = new InputLayout(_device, bytecode, _elements);
            bytecode.Dispose();

            bytecode = ShaderBytecode.CompileFromFile(fileName, "LightPixelShader", "ps_5_0", ShaderFlags.Debug | ShaderFlags.SkipOptimization);
            var pixelShader = new PixelShader(_device, bytecode);
            bytecode.Dispose();

            bytecode = ShaderBytecode.CompileFromFile(fileName, "HS", "hs_5_0", ShaderFlags.Debug | ShaderFlags.SkipOptimization);
            var hullShader = new HullShader(_device, bytecode);
            bytecode.Dispose();

            bytecode = ShaderBytecode.CompileFromFile(fileName, "DS", "ds_5_0", ShaderFlags.Debug | ShaderFlags.SkipOptimization);
            var domainShader = new DomainShader(_device, bytecode);
            bytecode.Dispose();

            _objectConstantBuffer = new ConstantBuffer<ConstantBufferPerObject>(_device);

            _frameConstantBuffer = new ConstantBuffer<ConstantBufferPerFrame>(_device);

            _constantBufferPerFrame = new ConstantBufferPerFrame();
            _constantBufferPerObject = new ConstantBufferPerObject();

            SamplerStateDescription samplerDesc = CreateSamplerStateDescription();

            _samplerState = new SamplerState(_device, samplerDesc);

            var normalMapSamplerDesc = CreateSamplerStateDescription();

            _normalMapSamplerState = new SamplerState(_device, normalMapSamplerDesc);

            var specularMapSamplerDesc = CreateSamplerStateDescription();

            _specularMapSamplerState = new SamplerState(_device, specularMapSamplerDesc);

            var displacementMapSamplerDesc = CreateSamplerStateDescription();

            _displacementSamplerState = new SamplerState(_device, displacementMapSamplerDesc);

            BindImmediateContext(vertexShader, pixelShader, hullShader, domainShader);

            _constantBufferPerFrame.gMaxTessDistance = 100;
            _constantBufferPerFrame.gMinTessDistance = 250;
            _constantBufferPerFrame.gMinTessFactor = 3;
            _constantBufferPerFrame.gMaxTessFactor = 27;

            BindDeferredContexts(vertexShader, pixelShader, hullShader, domainShader);
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

        private void BindImmediateContext(VertexShader vertexShader, PixelShader pixelShader, HullShader hullShader, DomainShader domainShader)
        {
            _immediateContext.InputAssembler.InputLayout = _layout;
            _immediateContext.InputAssembler.PrimitiveTopology = SharpDX.Direct3D.PrimitiveTopology.PatchListWith3ControlPoints;

            _immediateContext.VertexShader.SetConstantBuffer(0, _objectConstantBuffer.Buffer);
            _immediateContext.VertexShader.SetConstantBuffer(1, _frameConstantBuffer.Buffer);

            _immediateContext.PixelShader.SetConstantBuffer(0, _objectConstantBuffer.Buffer);
            _immediateContext.PixelShader.SetConstantBuffer(1, _frameConstantBuffer.Buffer);
            _immediateContext.PixelShader.SetSampler(0, _samplerState);
            _immediateContext.PixelShader.SetSampler(1, _normalMapSamplerState);

            _immediateContext.DomainShader.SetConstantBuffer(0, _objectConstantBuffer.Buffer);
            _immediateContext.DomainShader.SetConstantBuffer(1, _frameConstantBuffer.Buffer);
            _immediateContext.DomainShader.SetSampler(0, _displacementSamplerState);

            _immediateContext.HullShader.SetConstantBuffer(0, _objectConstantBuffer.Buffer);
            _immediateContext.HullShader.SetConstantBuffer(1, _frameConstantBuffer.Buffer);

            _immediateContext.VertexShader.Set(vertexShader);
            _immediateContext.PixelShader.Set(pixelShader);
            _immediateContext.HullShader.Set(hullShader);
            _immediateContext.DomainShader.Set(domainShader);
        }

        private void BindDeferredContexts(VertexShader vertexShader, PixelShader pixelShader, HullShader hullShader, DomainShader domainShader)
        {
            for (int i = 0; i < _deferredContexts.Length; i++)
            {
                _deferredContexts[i].InputAssembler.InputLayout = _layout;
                _deferredContexts[i].InputAssembler.PrimitiveTopology = SharpDX.Direct3D.PrimitiveTopology.PatchListWith3ControlPoints;
                _deferredContexts[i].VertexShader.SetConstantBuffer(0, _objectConstantBuffer.Buffer);
                _deferredContexts[i].VertexShader.SetConstantBuffer(1, _frameConstantBuffer.Buffer);
                _deferredContexts[i].PixelShader.SetConstantBuffer(0, _objectConstantBuffer.Buffer);
                _deferredContexts[i].PixelShader.SetConstantBuffer(1, _frameConstantBuffer.Buffer);
                _deferredContexts[i].PixelShader.SetSampler(0, _samplerState);
                _deferredContexts[i].PixelShader.SetSampler(1, _normalMapSamplerState);
                _deferredContexts[i].DomainShader.SetConstantBuffer(0, _objectConstantBuffer.Buffer);
                _deferredContexts[i].DomainShader.SetConstantBuffer(1, _frameConstantBuffer.Buffer);
                _deferredContexts[i].DomainShader.SetSampler(0, _displacementSamplerState);
                _deferredContexts[i].HullShader.SetConstantBuffer(0, _objectConstantBuffer.Buffer);
                _deferredContexts[i].HullShader.SetConstantBuffer(1, _frameConstantBuffer.Buffer);
                _deferredContexts[i].VertexShader.Set(vertexShader);
                _deferredContexts[i].PixelShader.Set(pixelShader);
                _deferredContexts[i].HullShader.Set(hullShader);
                _deferredContexts[i].DomainShader.Set(domainShader);
            }
        }

        public void Render(int threadNumber, 
                            int indexCount, 
                            Matrix worldMatrix, 
                            Matrix viewMatrix, 
                            Matrix viewProjectionMatrix, 
                            Texture texture, 
                            Vector3 cameraPosition, 
                            Material material)
        {
            lock (_lock)
            {
                _constantBufferPerObject.WorldMatrix = worldMatrix;
                _constantBufferPerObject.WorldMatrix.Transpose();
                _constantBufferPerObject.ViewMatrix = viewMatrix;
                _constantBufferPerObject.ViewMatrix.Transpose();
                _constantBufferPerObject.ViewProjectionMatrix = viewProjectionMatrix;
                _constantBufferPerObject.ViewProjectionMatrix.Transpose();
                _constantBufferPerObject.Material = material;

                _objectConstantBuffer.UpdateValue(_deferredContexts[threadNumber], _constantBufferPerObject);
            }

            _deferredContexts[threadNumber].PixelShader.SetShaderResource(0, texture.TextureData);

            if (texture.NormalMapData != null)
                _deferredContexts[threadNumber].PixelShader.SetShaderResource(1, texture.NormalMapData);

            if (texture.SpecularMapData != null)
                _deferredContexts[threadNumber].PixelShader.SetShaderResource(2, texture.SpecularMapData);

            if (texture.DisplacementMapData != null)
                _deferredContexts[threadNumber].DomainShader.SetShaderResource(0, texture.DisplacementMapData);

            _deferredContexts[threadNumber].DrawIndexed(indexCount, 0, 0);
        }

        public void UpdateFrameConstantBuffers(Camera camera)
        {
            _constantBufferPerFrame.CameraPosition = camera.EyeAt;
            _frameConstantBuffer.UpdateValue(_immediateContext, _constantBufferPerFrame);

            for (int i=0; i< _deferredContexts.Length; i++)
            {
                _frameConstantBuffer.UpdateValue(_deferredContexts[i], _constantBufferPerFrame);
            }
        }

        public void UpdateLightsFrameConstantBuffers(DirectionalLight[] directionalLight, PointLight[] pointLight, Spotlight[] spotLight)
        {
            _constantBufferPerFrame.DirectionalLight = directionalLight;
            _constantBufferPerFrame.PointLight = pointLight;
            _constantBufferPerFrame.SpotLight = spotLight;
            _constantBufferPerFrame.FogStart = ConfigManager.FogStart;
            _constantBufferPerFrame.FogEnd = ConfigManager.FogEnd;
            _constantBufferPerFrame.UseNormalMap = ConfigManager.UseNormalMap;
        }

        public void Dispose()
        {
            if (_layout != null)
                _layout.Dispose();

            if (_samplerState != null)
                _samplerState.Dispose();

            if (_normalMapSamplerState != null)
                _normalMapSamplerState.Dispose();

            if (_frameConstantBuffer != null)
                _frameConstantBuffer.Dispose();

            if (_objectConstantBuffer != null)
                _objectConstantBuffer.Dispose();
        }

     }
}
