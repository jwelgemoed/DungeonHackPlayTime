using DungeonHack.DirectX.ConstantBuffer;
using DungeonHack.Engine;
using FunAndGamesWithSharpDX.Engine;
using FunAndGamesWithSharpDX.Entities;
using SharpDX;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using Device = SharpDX.Direct3D11.Device;

namespace FunAndGamesWithSharpDX.DirectX
{
    public class SpriteShader
    {
        private Device _device;
        private DeviceContext _context;
        private InputLayout _layout;

        private SamplerState _samplerState;
        private InputElement[] _elements;

        private ShaderResourceView _currentTexture;

        private SharpDX.Direct3D11.Buffer _vertexBuffer;
        private VertexBufferBinding _vbBinding;

        static InputElement[] _spriteElements = {
            new InputElement("TEXCOORD", 0, Format.R32G32_Float, 0),
            new InputElement("TEXCOORDSIZE", 0, Format.R32G32_Float, 8),
            new InputElement("COLOR", 0, Format.B8G8R8A8_UNorm, 16),
            new InputElement("TOPLEFT", 0, Format.R32G32_Float, 20),
            new InputElement("TOPRIGHT", 0, Format.R32G32_Float, 28),
            new InputElement("BOTTOMLEFT", 0, Format.R32G32_Float, 36),
            new InputElement("BOTTOMRIGHT", 0, Format.R32G32_Float, 44)};

        struct SpriteStruct
        {
            internal Vector2 TexCoord;
            internal Vector2 TexCoordSize;
            internal int Color;
            internal Vector2 TopLeft;
            internal Vector2 TopRight;
            internal Vector2 BottomLeft;
            internal Vector2 BottomRight;
        }

        public SpriteShader(Device device, DeviceContext context)
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

            var fileName = basePath + @"\Shaders\Sprite.hlsl";

            var bytecode = ShaderBytecode.CompileFromFile(fileName, "mainVS", "vs_4_0");
            var vertexShader = new VertexShader(device, bytecode);

            _layout = new InputLayout(device, bytecode, _spriteElements);
            bytecode.Dispose();

            bytecode = ShaderBytecode.CompileFromFile(fileName, "mainPS", "ps_4_0");
            var pixelShader = new PixelShader(device, bytecode);
            bytecode.Dispose();

            bytecode = ShaderBytecode.CompileFromFile(fileName, "mainGS", "ps_4_0");
            var geometryShader = new GeometryShader(device, bytecode);
            bytecode.Dispose();

            var spriteSize = Utilities.SizeOf<SpriteStruct>();
            _vertexBuffer = new SharpDX.Direct3D11.Buffer(device, spriteSize,
                ResourceUsage.Dynamic, BindFlags.VertexBuffer, CpuAccessFlags.None, ResourceOptionFlags.None, spriteSize);

            _vbBinding = new VertexBufferBinding(_vertexBuffer, spriteSize, 0);

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
            _context.InputAssembler.PrimitiveTopology = SharpDX.Direct3D.PrimitiveTopology.PointList;
            _context.InputAssembler.SetVertexBuffers(0, _vbBinding);

            _context.VertexShader.SetConstantBuffer(0, _vertexBuffer);
            _context.VertexShader.Set(vertexShader);

            _context.PixelShader.Set(pixelShader);
            _context.PixelShader.SetSampler(0, _samplerState);

            _context.GeometryShader.Set(geometryShader);
        }
                      
        //public void Render(DeviceContext context, ShaderResourceView texture, )
        //{
        //    //_perObjectBuffer.WorldMatrix = worldMatrix;
        //    //_perObjectBuffer.WorldMatrix.Transpose();
        //    //_perObjectBuffer.ViewProjectionMatrix = viewProjectionMatrix;
        //    //_perObjectBuffer.ViewProjectionMatrix.Transpose();
        //    //_perObjectBuffer.Material = material;

        //    //context.UpdateSubresource(ref _perObjectBuffer, _vertexBuffer);

        //    //context.PixelShader.SetShaderResource(0, texture);

        //    //context.DrawIndexed(indexCount, 0, 0);
        //}

        public void Dispose()
        {
            if (_layout != null)
                _layout.Dispose();

            if (_samplerState != null)
                _samplerState.Dispose();

            if (_vertexBuffer != null)
                _vertexBuffer.Dispose();


        }

    }
}
