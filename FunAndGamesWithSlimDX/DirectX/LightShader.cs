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

        public LightShader(Device device, DeviceContext immediateContext, DeviceContext[] deferredContexts)
        {
            _immediateContext = immediateContext;
            _deferredContexts = deferredContexts;
        }

        public void Initialize(Device device, DeviceContext immediateContext, DeviceContext[] deviceContexts)
        {
            _immediateContext = immediateContext;
            _deferredContexts = deviceContexts;

            _elements = Vertex.GetInputElements();

            var basePath = ConfigManager.ResourcePath;

            var fileName = basePath + @"\Shaders\LightTexture.hlsl";

            var bytecode = ShaderBytecode.CompileFromFile(fileName, "LightVertexShader", "vs_5_0", ShaderFlags.Debug | ShaderFlags.SkipOptimization);
            var vertexShader = new VertexShader(device, bytecode);

            _layout = new InputLayout(device, bytecode, _elements);
            bytecode.Dispose();

            bytecode = ShaderBytecode.CompileFromFile(fileName, "LightPixelShader", "ps_5_0", ShaderFlags.Debug | ShaderFlags.SkipOptimization);
            var pixelShader = new PixelShader(device, bytecode);
            bytecode.Dispose();

            bytecode = ShaderBytecode.CompileFromFile(fileName, "HS", "hs_5_0", ShaderFlags.Debug | ShaderFlags.SkipOptimization);
            var hullShader = new HullShader(device, bytecode);
            bytecode.Dispose();

            bytecode = ShaderBytecode.CompileFromFile(fileName, "DS", "ds_5_0", ShaderFlags.Debug | ShaderFlags.SkipOptimization);
            var domainShader = new DomainShader(device, bytecode);
            bytecode.Dispose();

            _objectConstantBuffer = new ConstantBuffer<ConstantBufferPerObject>(device);

            _frameConstantBuffer = new ConstantBuffer<ConstantBufferPerFrame>(device);

            _constantBufferPerFrame = new ConstantBufferPerFrame();
            _constantBufferPerObject = new ConstantBufferPerObject();

            var samplerDesc = new SamplerStateDescription
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

            _samplerState = new SamplerState(device, samplerDesc);

            var normalMapSamplerDesc = new SamplerStateDescription
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

            _normalMapSamplerState = new SamplerState(device, normalMapSamplerDesc);

            var displacementMapSamplerDesc = new SamplerStateDescription
            {
                Filter = Filter.MinMagLinearMipPoint,
                AddressU = TextureAddressMode.Border,
                AddressV = TextureAddressMode.Border,
                AddressW = TextureAddressMode.Wrap,
                MipLodBias = 0.0f,
                MaximumAnisotropy = 1,
                ComparisonFunction = Comparison.Always,
                BorderColor = Colors.Black,
                MinimumLod = 0,
                MaximumLod = 0
            };

            _displacementSamplerState = new SamplerState(device, displacementMapSamplerDesc);

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

            _constantBufferPerFrame.gMaxTessDistance = 100;
            _constantBufferPerFrame.gMinTessDistance = 250;
            _constantBufferPerFrame.gMinTessFactor = 3;
            _constantBufferPerFrame.gMaxTessFactor = 27;

            for (int i=0; i<_deferredContexts.Length; i++)
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
            _constantBufferPerObject.WorldMatrix = worldMatrix;
            _constantBufferPerObject.WorldMatrix.Transpose();
            _constantBufferPerObject.ViewMatrix = viewMatrix;
            _constantBufferPerObject.ViewMatrix.Transpose();
            _constantBufferPerObject.ViewProjectionMatrix = viewProjectionMatrix;
            _constantBufferPerObject.ViewProjectionMatrix.Transpose();
            _constantBufferPerObject.Material = material;

            _objectConstantBuffer.UpdateValue(_deferredContexts[threadNumber], _constantBufferPerObject);

            _deferredContexts[threadNumber].PixelShader.SetShaderResource(0, texture.TextureData);

            if (texture.NormalMapData != null)
                _deferredContexts[threadNumber].PixelShader.SetShaderResource(1, texture.NormalMapData);

            if (texture.DisplacementMapData != null)
                _deferredContexts[threadNumber].DomainShader.SetShaderResource(0, texture.DisplacementMapData);

            _deferredContexts[threadNumber].DrawIndexed(indexCount, 0, 0);
        }

        public void RenderFrame(Camera camera)
        {
            _constantBufferPerFrame.CameraPosition = camera.EyeAt;
            _frameConstantBuffer.UpdateValue(_immediateContext, _constantBufferPerFrame);
        }

        public void RenderLights(DirectionalLight[] directionalLight, PointLight[] pointLight, Spotlight[] spotLight)
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
