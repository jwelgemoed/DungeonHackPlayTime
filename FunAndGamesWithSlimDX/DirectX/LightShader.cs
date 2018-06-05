using DungeonHack.DirectX.ConstantBuffer;
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
        private DeviceContext _context;
        private InputLayout _layout;

        private SamplerState _samplerState;
        private InputElement[] _elements;

        private ConstantBufferPerObject _perObjectBuffer;
        private ConstantBufferPerFrame _perFrameBuffer;
        private ShaderResourceView _currentTexture;

        private SharpDX.Direct3D11.Buffer _constantBufferPerObject;
        private SharpDX.Direct3D11.Buffer _constantBufferPerFrame;
        
        public LightShader(Device device, DeviceContext context)
        {
            _context = context;
        }

        public void Initialize(Device device, DeviceContext context)
        {
            _context = context;

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

            _constantBufferPerObject = new SharpDX.Direct3D11.Buffer(device, Utilities.SizeOf<ConstantBufferPerObject>(), 
                ResourceUsage.Dynamic, BindFlags.ConstantBuffer, CpuAccessFlags.Write, ResourceOptionFlags.None, 0);

            _constantBufferPerFrame = new SharpDX.Direct3D11.Buffer(device, Utilities.SizeOf<ConstantBufferPerFrame>(),
                ResourceUsage.Dynamic, BindFlags.ConstantBuffer, CpuAccessFlags.Write, ResourceOptionFlags.None, 0);// ConstantBufferPerFrame.Stride);

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

            _context.InputAssembler.InputLayout = _layout;
            _context.InputAssembler.PrimitiveTopology = SharpDX.Direct3D.PrimitiveTopology.TriangleList;

            _context.VertexShader.SetConstantBuffer(0, _constantBufferPerObject);
            _context.VertexShader.SetConstantBuffer(1, _constantBufferPerFrame);

            _context.PixelShader.SetConstantBuffer(0, _constantBufferPerObject);
            _context.PixelShader.SetConstantBuffer(1, _constantBufferPerFrame);
            _context.PixelShader.SetSampler(0, _samplerState);

            _context.VertexShader.Set(vertexShader);
            _context.PixelShader.Set(pixelShader);
        }
                
        public void Render(DeviceContext context, int indexCount, Matrix worldMatrix, Matrix viewMatrix, Matrix viewProjectionMatrix, ShaderResourceView texture, Vector3 cameraPosition, Material material)
        {
            _perObjectBuffer.WorldMatrix = worldMatrix;
            _perObjectBuffer.WorldMatrix.Transpose();
            _perObjectBuffer.ViewMatrix = viewMatrix;
            _perObjectBuffer.ViewMatrix.Transpose();
            _perObjectBuffer.ViewProjectionMatrix = viewProjectionMatrix;
            _perObjectBuffer.ViewProjectionMatrix.Transpose();
            _perObjectBuffer.Material = material;

            _perFrameBuffer.CameraPosition = cameraPosition;

            //context.UpdateSubresource(ref _perObjectBuffer, _constantBufferPerObject);
            DataStream mappedResource;
            context.MapSubresource(_constantBufferPerObject, 0, MapMode.WriteDiscard, MapFlags.None, out mappedResource);

            mappedResource.Write(_perObjectBuffer);
            context.UnmapSubresource(_constantBufferPerObject, 0);

            DataStream mappedResource2;
            context.MapSubresource(_constantBufferPerFrame, 0, MapMode.WriteDiscard, MapFlags.None, out mappedResource2);

            mappedResource2.Write(_perFrameBuffer);

            //var databox = context.MapSubresource(_dynamicConstantBufferPerFrame, 0, MapMode.WriteDiscard, MapFlags.None);
            //Utilities.Write(databox.DataPointer, ref _perFrameBuffer);
            context.UnmapSubresource(_constantBufferPerFrame, 0);

            context.PixelShader.SetShaderResource(0, texture);
            
            context.DrawIndexed(indexCount, 0, 0);
        }

        public void RenderLights(DirectionalLight directionalLight, PointLight pointLight, Spotlight spotLight)
        {
            _perFrameBuffer.DirectionalLight = directionalLight;
            _perFrameBuffer.PointLight = pointLight;
            _perFrameBuffer.SpotLight = spotLight;
            _perFrameBuffer.FogStart = ConfigManager.FogStart;
            _perFrameBuffer.FogEnd = ConfigManager.FogEnd;
        }

        public void Dispose()
        {
            if (_layout != null)
                _layout.Dispose();

            if (_samplerState != null)
                _samplerState.Dispose();

            if (_constantBufferPerObject != null)
                _constantBufferPerObject.Dispose();

            if (_constantBufferPerFrame != null)
                _constantBufferPerFrame.Dispose();
        }

     }
}
