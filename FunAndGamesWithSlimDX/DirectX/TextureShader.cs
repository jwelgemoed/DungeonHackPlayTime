using DungeonHack.DirectX;
using DungeonHack.DirectX.ConstantBuffer;
using DungeonHack.Engine;
using DungeonHack.Lights;
using FunAndGamesWithSharpDX.Engine;
using FunAndGamesWithSharpDX.Entities;
using SharpDX;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D11;
using Device = SharpDX.Direct3D11.Device;

namespace FunAndGamesWithSharpDX.DirectX
{
    public class TextureShader : IShader
    {
        private readonly Device _device;
        private readonly DeviceContext _immediateContext;
        private readonly DeviceContext[] _deferredContexts;
        private InputLayout _layout;

        private SamplerState _samplerState;
        private InputElement[] _elements;

        private ConstantBufferPerObject _perObjectBuffer;
        private ShaderResourceView _currentTexture;

        private SharpDX.Direct3D11.Buffer _staticContantBuffer;

        private object _lock = new object();
        
        public TextureShader(Renderer renderer)
        {
            _device = renderer.Device;
            _immediateContext = renderer.ImmediateContext;
            _deferredContexts = renderer.DeferredContexts;
        }

        public void Initialize()
        {
            _elements = Vertex.GetInputElements();

            var basePath = ConfigManager.ResourcePath;

            var fileName = basePath + @"\Shaders\Texture.hlsl";

            var bytecode = ShaderBytecode.CompileFromFile(fileName, "TextureVertexShader", "vs_4_0");
            var vertexShader = new VertexShader(_device, bytecode);

            _layout = new InputLayout(_device, bytecode, _elements);
            bytecode.Dispose();

            bytecode = ShaderBytecode.CompileFromFile(fileName, "TexturePixelShader", "ps_4_0");
            var pixelShader = new PixelShader(_device, bytecode);
            bytecode.Dispose();

            _staticContantBuffer = new SharpDX.Direct3D11.Buffer(_device, Utilities.SizeOf<ConstantBufferPerObject>(), 
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

            _samplerState = new SamplerState(_device, samplerDesc);

            _immediateContext.InputAssembler.InputLayout = _layout;
            _immediateContext.InputAssembler.PrimitiveTopology = SharpDX.Direct3D.PrimitiveTopology.TriangleList;
            _immediateContext.VertexShader.SetConstantBuffer(0, _staticContantBuffer);
            _immediateContext.VertexShader.Set(vertexShader);
            _immediateContext.PixelShader.Set(pixelShader);
            _immediateContext.PixelShader.SetSampler(0, _samplerState);
            _immediateContext.HullShader.Set(null);
            _immediateContext.DomainShader.Set(null);

            for (int i=0; i<_deferredContexts.Length; i++)
            {
                _deferredContexts[i].InputAssembler.InputLayout = _layout;
                _deferredContexts[i].InputAssembler.PrimitiveTopology = SharpDX.Direct3D.PrimitiveTopology.TriangleList;
                _deferredContexts[i].VertexShader.SetConstantBuffer(0, _staticContantBuffer);
                _deferredContexts[i].VertexShader.Set(vertexShader);
                _deferredContexts[i].PixelShader.Set(pixelShader);
                _deferredContexts[i].PixelShader.SetSampler(0, _samplerState);
                _deferredContexts[i].HullShader.Set(null);
                _deferredContexts[i].DomainShader.Set(null);
            }
        }

               
        public void SetSelectedShaderEffect(Device device, string technique)
        {
            
        }

        public void RenderFrame(Camera camera)
        {

        }
                
        public void Render(int threadNumber, int indexCount, Matrix worldMatrix, Matrix viewMatrix, Matrix viewProjectionMatrix,
                           Texture texture, Vector3 cameraPosition, Material material)
        {
            lock (_lock)
            {
                _perObjectBuffer.WorldMatrix = worldMatrix;
                _perObjectBuffer.WorldMatrix.Transpose();
                _perObjectBuffer.ViewMatrix = viewMatrix;
                _perObjectBuffer.ViewMatrix.Transpose();
                _perObjectBuffer.ViewProjectionMatrix = viewProjectionMatrix;
                _perObjectBuffer.ViewProjectionMatrix.Transpose();
                _perObjectBuffer.Material = material;
            }

            _deferredContexts[threadNumber].UpdateSubresource(ref _perObjectBuffer, _staticContantBuffer);

            _deferredContexts[threadNumber].PixelShader.SetShaderResource(0, texture.TextureData);

            if (texture.NormalMapData != null)
                _deferredContexts[threadNumber].PixelShader.SetShaderResource(1, texture.NormalMapData);
            
            _deferredContexts[threadNumber].DrawIndexed(indexCount, 0, 0);
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
