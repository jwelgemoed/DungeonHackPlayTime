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
    public class TextureShader : IShader
    {
        private Device _device;
        private DeviceContext[] _contextPerThread;
        private InputLayout _layout;

        private SamplerState _samplerState;
        private InputElement[] _elements;

        private ConstantBufferPerObject _perObjectBuffer;
        private ShaderResourceView _currentTexture;

        private SharpDX.Direct3D11.Buffer _staticContantBuffer;
        private int _threadCount;
        
        public TextureShader(Device device, DeviceContext[] contextPerThread)
        {
            _device = device;
            _contextPerThread = contextPerThread;
            _threadCount = _contextPerThread.Length;
        }

        public void Initialize(Device device, DeviceContext[] contextPerThread)
        {
            _device = device;
            _contextPerThread = contextPerThread;
            _threadCount = _contextPerThread.Length;

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

            for (int i = 0; i < _threadCount; i++)
            {
                _contextPerThread[i].InputAssembler.InputLayout = _layout;
                _contextPerThread[i].InputAssembler.PrimitiveTopology = SharpDX.Direct3D.PrimitiveTopology.TriangleList;
                _contextPerThread[i].VertexShader.SetConstantBuffer(0, _staticContantBuffer);
                _contextPerThread[i].VertexShader.Set(vertexShader);
                _contextPerThread[i].PixelShader.Set(pixelShader);
                _contextPerThread[i].PixelShader.SetSampler(0, _samplerState);
            }
        }

               
        public void SetSelectedShaderEffect(Device device, string technique)
        {
            
        }
                
        public void Render(int contextCount, int indexCount, Matrix worldMatrix, Matrix viewMatrix,
                           Matrix projectionMatrix, ShaderResourceView texture, Vector3 cameraPosition, Material material)
        {
            _perObjectBuffer.WorldMatrix = worldMatrix;
            _perObjectBuffer.WorldMatrix.Transpose();
            _perObjectBuffer.ViewMatrix = viewMatrix;
            _perObjectBuffer.ViewMatrix.Transpose();
            _perObjectBuffer.ProjectionMatrix = projectionMatrix;
            _perObjectBuffer.ProjectionMatrix.Transpose();
            _perObjectBuffer.Material = material;

            var context = _contextPerThread[contextCount];

            context.UpdateSubresource(ref _perObjectBuffer, _staticContantBuffer);

            context.PixelShader.SetShaderResource(0, texture);
            
            context.DrawIndexed(indexCount, 0, 0);
        }

        public void RenderLights(DirectionalLight directionalLight, PointLight pointLight, Spotlight spotLight)
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
