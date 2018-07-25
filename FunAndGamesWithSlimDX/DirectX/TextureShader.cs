using DungeonHack.DirectX.ConstantBuffer;
using FunAndGamesWithSharpDX.Engine;
using FunAndGamesWithSharpDX.Entities;
using SharpDX;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D11;
using System;
using DungeonHack.DirectX;
using DungeonHack.Engine;
using DungeonHack.Lights;
using Device = SharpDX.Direct3D11.Device;

namespace FunAndGamesWithSharpDX.DirectX
{
    public class TextureShader : IShader
    {
        private Device _device;
        private DeviceContext _context;
        private InputLayout _layout;

        private SamplerState _samplerState;
        private InputElement[] _elements;

        private ConstantBufferPerObject _perObjectBuffer;
        private ShaderResourceView _currentTexture;

        private SharpDX.Direct3D11.Buffer _staticContantBuffer;

        
        public TextureShader(Device device, DeviceContext context, DeviceContext[] deferredContexts)
        {
            _device = device;
            _context = context;
        }

        public void Initialize(Device device, DeviceContext context, DeviceContext[] deferredContexts)
        {
            _device = device;
            _context = context;

            _elements = Vertex.GetInputElements();

            var basePath = ConfigManager.ResourcePath;

            var fileName = basePath + @"\Shaders\Texture.hlsl";

            var bytecode = ShaderBytecode.CompileFromFile(fileName, "TextureVertexShader", "vs_4_0");
            var vertexShader = new VertexShader(device, bytecode);

            _layout = new InputLayout(device, bytecode, _elements);
            bytecode.Dispose();

            bytecode = ShaderBytecode.CompileFromFile(fileName, "TexturePixelShader", "ps_4_0");
            var pixelShader = new PixelShader(device, bytecode);
            bytecode.Dispose();

            _staticContantBuffer = new SharpDX.Direct3D11.Buffer(device, Utilities.SizeOf<ConstantBufferPerObject>(), 
                ResourceUsage.Default, BindFlags.ConstantBuffer, CpuAccessFlags.None, ResourceOptionFlags.None, ConstantBufferPerObject.Stride);

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
            _context.VertexShader.SetConstantBuffer(0, _staticContantBuffer);
            _context.VertexShader.Set(vertexShader);
            _context.PixelShader.Set(pixelShader);
            _context.PixelShader.SetSampler(0, _samplerState);
            _context.HullShader.Set(null);
            _context.DomainShader.Set(null);
        }

               
        public void SetSelectedShaderEffect(Device device, string technique)
        {
            
        }

        public void RenderFrame(Camera camera)
        {

        }
                
        public void Render(DeviceContext context, int indexCount, Matrix worldMatrix, Matrix viewMatrix, Matrix viewProjectionMatrix,
                           Texture texture, Vector3 cameraPosition, Material material)
        {
            _perObjectBuffer.WorldMatrix = worldMatrix;
            _perObjectBuffer.WorldMatrix.Transpose();
            _perObjectBuffer.ViewMatrix = viewMatrix;
            _perObjectBuffer.ViewMatrix.Transpose();
            _perObjectBuffer.ViewProjectionMatrix = viewProjectionMatrix;
            _perObjectBuffer.ViewProjectionMatrix.Transpose();
            _perObjectBuffer.Material = material;

            context.UpdateSubresource(ref _perObjectBuffer, _staticContantBuffer);

            context.PixelShader.SetShaderResource(0, texture.TextureData);

            if (texture.NormalMapData != null)
                context.PixelShader.SetShaderResource(1, texture.NormalMapData);
            
            context.DrawIndexed(indexCount, 0, 0);
        }

        public void RenderLights(DirectionalLight[] directionalLight, PointLight[] pointLight, Spotlight[] spotLight)
        {
            
        }

        public void Dispose()
        {
            if (_layout != null)
                _layout.Dispose();

            if (_samplerState != null)
                _samplerState.Dispose();

            if (_staticContantBuffer != null)
                _staticContantBuffer.Dispose();
        
        }

     }
}
