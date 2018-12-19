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
    public class AmbientLightShader
    {
        private Device _device;
        private DeviceContext _immediateContext;
        private InputLayout _layout;
        private DeferredShadingRenderer _deferredShadingRenderer;

        private SamplerState _samplerState;
        private InputElement[] _elements;
        private VertexShader _vertexShader;
        private PixelShader _pixelShader;
        private Camera _camera;

        private ConstantBuffer<ConstantBufferAmbientLight> _ambientLightConstantBuffer;
        private ConstantBuffer<ConstantBufferDeferredInfo> _deferredInfoConstantBuffer;

        private ConstantBufferAmbientLight _constantBufferAmbientLight;
        private ConstantBufferDeferredInfo _constantBufferDeferredInfo;

        public AmbientLightShader(Renderer renderer, Camera camera, DeferredShadingRenderer deferredShadingRenderer)
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

            var vsShaderName = basePath + @"\Shaders\AmbientLightVS.hlsl";
            var psShaderName = basePath + @"\Shaders\AmbientLightPS.hlsl";

            var bytecode = ShaderBytecode.CompileFromFile(vsShaderName, "AmbientLightVS", "vs_5_0", ShaderFlags.Debug | ShaderFlags.SkipOptimization,
                include: FileIncludeHandler.Default);

            _vertexShader = new VertexShader(_device, bytecode);

            _layout = new InputLayout(_device, bytecode, _elements);
            bytecode.Dispose();

            bytecode = ShaderBytecode.CompileFromFile(psShaderName, "AmbientLightPS", "ps_5_0", ShaderFlags.Debug | ShaderFlags.SkipOptimization,
                include: FileIncludeHandler.Default);

            _pixelShader = new PixelShader(_device, bytecode);
            bytecode.Dispose();

            _ambientLightConstantBuffer = new ConstantBuffer<ConstantBufferAmbientLight>(_device);

            _constantBufferAmbientLight = new ConstantBufferAmbientLight();

            _deferredInfoConstantBuffer = new ConstantBuffer<ConstantBufferDeferredInfo>(_device);
            _constantBufferDeferredInfo = new ConstantBufferDeferredInfo();
            _constantBufferDeferredInfo.PerspectiveValues = new Vector4();

            SamplerStateDescription samplerDesc = CreateSamplerStateDescription();

            _samplerState = new SamplerState(_device, samplerDesc);

            _constantBufferDeferredInfo.PerspectiveValues.X = 1 / _camera.ProjectionMatrix.M11;
            _constantBufferDeferredInfo.PerspectiveValues.Y = 1 / _camera.ProjectionMatrix.M22;
            _constantBufferDeferredInfo.PerspectiveValues.Z = _camera.ProjectionMatrix.M32;
            _constantBufferDeferredInfo.PerspectiveValues.W = _camera.ProjectionMatrix.M22;

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
            _immediateContext.InputAssembler.PrimitiveTopology = SharpDX.Direct3D.PrimitiveTopology.TriangleList;

            _immediateContext.PixelShader.SetConstantBuffer(0, _deferredInfoConstantBuffer.Buffer);
            _immediateContext.PixelShader.SetConstantBuffer(1, _ambientLightConstantBuffer.Buffer);

            _immediateContext.VertexShader.Set(vertexShader);
            _immediateContext.PixelShader.Set(pixelShader);
        }

        public void RenderLights(AmbientLight[] ambientLights)
        {
            _immediateContext.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(null, 0, 0));
            _immediateContext.InputAssembler.SetIndexBuffer(null, SharpDX.DXGI.Format.R32_UInt, 0);
            _constantBufferDeferredInfo.ViewInv = Matrix.Invert(_camera.ViewMatrix);
            _immediateContext.PixelShader.SetShaderResource(0, _deferredShadingRenderer.DepthShaderResourceView);
            _immediateContext.PixelShader.SetShaderResources(1, _deferredShadingRenderer.ShaderResourceViews);
            
            for (int i=0; i<ambientLights.Length; i++)
            {
                _constantBufferAmbientLight.AmbientLight = ambientLights[i];

                _ambientLightConstantBuffer.UpdateValue(_immediateContext, _constantBufferAmbientLight);

                _deferredInfoConstantBuffer.UpdateValue(_immediateContext, _constantBufferDeferredInfo);

                _immediateContext.Draw(4, 0);
            }
        }

        public void Dispose()
        {
            _layout?.Dispose();
            _samplerState?.Dispose();
            _samplerState?.Dispose();
            _ambientLightConstantBuffer?.Dispose();
            _deferredInfoConstantBuffer?.Dispose();
            _vertexShader?.Dispose();
            _pixelShader?.Dispose();
        }
    }
}
