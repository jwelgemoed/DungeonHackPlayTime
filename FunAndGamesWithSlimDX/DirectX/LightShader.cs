using DungeonHack.DirectX.ConstantBuffer;
using FunAndGamesWithSharpDX.Engine;
using FunAndGamesWithSharpDX.Entities;
using FunAndGamesWithSharpDX.Lights;
using SharpDX;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D11;
using System;
using Device = SharpDX.Direct3D11.Device;

namespace FunAndGamesWithSharpDX.DirectX
{
    public class LightShader : IShader
    {
        private Device _device;
        private DeviceContext _context;
        private InputLayout _layout;

        private SamplerState _samplerState;
        private InputElement[] _elements;

        private ConstantBufferPerObject _perObjectBuffer;
        private ConstantBufferPerFrame _perFrameBuffer;
        private ShaderResourceView _currentTexture;

        private SharpDX.Direct3D11.Buffer _staticConstantBufferPerObject;
        private SharpDX.Direct3D11.Buffer _dynamicConstantBufferPerFrame;

        
        public LightShader(Device device, DeviceContext context)
        {
            _device = device;
            _context = context;
        }

        public void Initialize(Device device, DeviceContext context)
        {
            _device = device;
            _context = context;

            _elements = Vertex.GetInputElements();

            var basePath = ConfigManager.ResourcePath;

            var fileName = basePath + @"\Shaders\LightTexture.hlsl";

            var bytecode = ShaderBytecode.CompileFromFile(fileName, "LightVertexShader", "vs_4_0");
            var vertexShader = new VertexShader(device, bytecode);

            _layout = new InputLayout(device, bytecode, _elements);
            bytecode.Dispose();

            bytecode = ShaderBytecode.CompileFromFile(fileName, "LightPixelShader", "ps_4_0");
            var pixelShader = new PixelShader(device, bytecode);
            bytecode.Dispose();

            _staticConstantBufferPerObject = new SharpDX.Direct3D11.Buffer(device, Utilities.SizeOf<ConstantBufferPerObject>(), 
                ResourceUsage.Default, BindFlags.ConstantBuffer, CpuAccessFlags.None, ResourceOptionFlags.None, 0);

            _dynamicConstantBufferPerFrame = new SharpDX.Direct3D11.Buffer(device, Utilities.SizeOf<ConstantBufferPerFrame>(),
                ResourceUsage.Dynamic, BindFlags.ConstantBuffer, CpuAccessFlags.Write, ResourceOptionFlags.None, 0);

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

            _context.VertexShader.SetConstantBuffer(0, _staticConstantBufferPerObject);
            _context.VertexShader.SetConstantBuffer(1, _dynamicConstantBufferPerFrame);

            _context.PixelShader.SetConstantBuffer(0, _staticConstantBufferPerObject);
            _context.PixelShader.SetConstantBuffer(1, _dynamicConstantBufferPerFrame);
            _context.PixelShader.SetSampler(0, _samplerState);

            _context.VertexShader.Set(vertexShader);
            _context.PixelShader.Set(pixelShader);
        }
                
        public void Render(DeviceContext context, int indexCount, Matrix worldMatrix, Matrix viewMatrix,
                           Matrix projectionMatrix, ShaderResourceView texture, Vector3 cameraPosition, Material material)
        {
            _perObjectBuffer.WorldMatrix = worldMatrix;
            _perObjectBuffer.WorldMatrix.Transpose();
            _perObjectBuffer.ViewMatrix = viewMatrix;
            _perObjectBuffer.ViewMatrix.Transpose();
            _perObjectBuffer.ProjectionMatrix = projectionMatrix;
            _perObjectBuffer.ProjectionMatrix.Transpose();
            _perObjectBuffer.Material = material;

            _perFrameBuffer.CameraPosition = cameraPosition;

            context.UpdateSubresource(ref _perObjectBuffer, _staticConstantBufferPerObject);

            DataStream mappedResource;
            context.MapSubresource(_dynamicConstantBufferPerFrame, 0, MapMode.WriteDiscard, MapFlags.None, out mappedResource);

            mappedResource.Write(_perFrameBuffer);

            //var databox = context.MapSubresource(_dynamicConstantBufferPerFrame, 0, MapMode.WriteDiscard, MapFlags.None);
            //Utilities.Write(databox.DataPointer, ref _perFrameBuffer);
            context.UnmapSubresource(_dynamicConstantBufferPerFrame, 0);

            context.PixelShader.SetShaderResource(0, texture);
            
            context.DrawIndexed(indexCount, 0, 0);
        }

        public void RenderLights(DirectionalLight directionalLight, PointLight pointLight, Spotlight spotLight)
        {
            _perFrameBuffer.DirectionalLight = directionalLight;
            _perFrameBuffer.PointLight = pointLight;
            _perFrameBuffer.SpotLight = spotLight;
        }

        public void Dispose()
        {
            if (_layout != null)
                _layout.Dispose();

            if (_samplerState != null)
                _samplerState.Dispose();

            if (_staticConstantBufferPerObject != null)
                _staticConstantBufferPerObject.Dispose();

            if (_dynamicConstantBufferPerFrame != null)
                _dynamicConstantBufferPerFrame.Dispose();
        }

     }
}
