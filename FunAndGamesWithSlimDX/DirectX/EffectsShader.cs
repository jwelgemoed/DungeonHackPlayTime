using FunAndGamesWithSharpDX.Engine;
using FunAndGamesWithSharpDX.Entities;
using FunAndGamesWithSharpDX.Lights;
using SharpDX;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D11;
using System;
using System.Runtime.InteropServices;
using Device = SharpDX.Direct3D11.Device;

namespace FunAndGamesWithSharpDX.DirectX
{
    public class EffectsShader : IShader
    {
        private Device _device;
        private InputLayout _layout;
        private Effect _fx;
        private EffectTechnique _technique;

        private EffectConstantBuffer _cbPerFrame;
        private EffectConstantBuffer _cbPerObject;

        private EffectVariable _gDirLight;
        private EffectVariable _gPointLight;
        private EffectVariable _gSpotLight;

        private EffectVariable _material;

        private SamplerState _samplerState;
        private ShaderBytecode _shader;
        private InputElement[] _elements;

        private EffectMatrixVariable _worldMatrix;
        private EffectMatrixVariable _viewMatrix;
        private EffectMatrixVariable _projectionMatrix;

        private EffectVectorVariable _cameraPosition;

        private EffectShaderResourceVariable _shaderTexture;
        private EffectSamplerVariable _sampleType;

        public EffectsShader()
        {
        }

        public EffectsShader(Device device)
        {
            _device = device;
        }

        public void Initialize(Device device, DeviceContext context)
        {
            Initialize(device);
        }

        public void Initialize(Device device)
        {
            _device = device;

            _elements = Vertex.GetInputElements();

            var basePath = ConfigManager.ResourcePath;

            var fileName = basePath + @"\FX\ShaderEffects.fx";

            var result = ShaderBytecode.CompileFromFile(fileName, null, "fx_5_0");

            _shader = result.Bytecode;

            _fx = new Effect(device, _shader);

            _cbPerFrame = _fx.GetConstantBufferByName("cbPerFrame").AsConstantBuffer();
            _cbPerObject = _fx.GetConstantBufferByName("cbPerObject").AsConstantBuffer();

            Initialize(device, "LightTech");
        }

        public void Initialize(Device device, string technique)
        {
            _technique = _fx.GetTechniqueByName(technique);

            var passDescription = _technique.GetPassByIndex(0).Description;

            _layout = new InputLayout(device, passDescription.Signature, _elements);

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

            _worldMatrix = _cbPerObject.GetMemberByName("worldMatrix").AsMatrix();
            _viewMatrix = _cbPerObject.GetMemberByName("viewMatrix").AsMatrix();
            _projectionMatrix = _cbPerObject.GetMemberByName("projectionMatrix").AsMatrix();

            _gDirLight = _cbPerFrame.GetMemberByName("gDirLight");
            _gPointLight = _cbPerFrame.GetMemberByName("gPointLight");
            _gSpotLight = _cbPerFrame.GetMemberByName("gSpotLight");

            _material = _cbPerObject.GetMemberByName("material");

            _cameraPosition = _cbPerFrame.GetMemberByName("cameraPosition").AsVector();

            _shaderTexture = _fx.GetVariableByName("shaderTexture").AsShaderResource();
            _sampleType = _fx.GetVariableByName("SampleType").AsSampler();
        }

        
        public void SetSelectedShaderEffect(Device device, string technique)
        {
            Initialize(device, technique);
        }

        public void Render(DeviceContext context, int indexCount, Matrix worldMatrix, Matrix viewMatrix, Matrix projectionMatrix)
        {
            throw new NotImplementedException();
        }

        public void Render(DeviceContext context, int indexCount, Matrix worldMatrix, Matrix viewMatrix, Matrix projectionMatrix,
                           ShaderResourceView texture, Material material)
        {
            SetShaderParameters(context, worldMatrix, viewMatrix, projectionMatrix, texture);

            RenderShader(context, indexCount);
        }

        public void Render(DeviceContext context, int indexCount, Matrix worldMatrix, Matrix viewMatrix,
                           Matrix projectionMatrix, ShaderResourceView texture, Color4 diffuseColor,
                                         Vector3 lightDirection, Vector3 cameraPosition)
        {
            SetShaderParameters(context, worldMatrix, viewMatrix, projectionMatrix, texture, diffuseColor, Colors.White, lightDirection, cameraPosition, new Material());

            RenderShader(context, indexCount);
        }

        public void Render(DeviceContext context, int indexCount, Matrix worldMatrix, Matrix viewMatrix,
                           Matrix projectionMatrix, ShaderResourceView texture, Color4 diffuseColor,
                                         Vector3 lightDirection)
        {
            SetShaderParameters(context, worldMatrix, viewMatrix, projectionMatrix, texture, diffuseColor, Colors.White, lightDirection, new Vector3(1, 1, 1), new Material());

            RenderShader(context, indexCount);
        }

        public void Render(DeviceContext context, int indexCount, Matrix worldMatrix, Matrix viewMatrix,
                           Matrix projectionMatrix, ShaderResourceView texture, Vector3 cameraPosition, Material material)
        {
            SetShaderParameters(context, worldMatrix, viewMatrix, projectionMatrix, texture, null, null, null, cameraPosition, material);

            RenderShader(context, indexCount);
        }

        public void RenderLights(DirectionalLight directionalLight, PointLight pointLight, Spotlight spotLight)
        {
            //var d = Util.GetArray(directionalLight);
            //Array.Copy(d, 0, _diretionalLightArray, 0, Directional.Stride);

            using (var buffer = new DataStream(Marshal.SizeOf(directionalLight), false, false))
            {
                _gDirLight.SetRawValue(buffer, DirectionalLight.Stride);
            }

            //var p = Util.GetArray(pointLight);

            using (var buffer = new DataStream(Marshal.SizeOf(pointLight), false, false))
            {
                _gPointLight.SetRawValue(buffer, PointLight.Stride);
            }

            //var s = Util.GetArray(spotLight);

            using (var buffer = new DataStream(Marshal.SizeOf(spotLight), false, false))
            {
                _gSpotLight.SetRawValue(buffer, Spotlight.Stride);
            }
        }

        private void RenderShader(DeviceContext context, int indexCount)
        {
            context.InputAssembler.InputLayout = _layout;

            var techDesc = _technique.Description;

            for (int i = 0; i < techDesc.PassCount; i++)
            {
                _technique.GetPassByIndex(i).Apply(context);

                context.PixelShader.SetSampler(0, _samplerState);

                context.DrawIndexed(indexCount, 0, 0);
            }
        }

        private void SetShaderParameters(DeviceContext context, Matrix worldMatrix, Matrix viewMatrix,
                                         Matrix projectionMatrix, ShaderResourceView texture)
        {
            _worldMatrix.SetMatrix(worldMatrix);
            _viewMatrix.SetMatrix(viewMatrix);
            _projectionMatrix.SetMatrix(projectionMatrix);

            _shaderTexture.SetResource(texture);
            _sampleType.SetSampler(0, _samplerState);

            context.PixelShader.SetShaderResource(0, texture);
        }

        private void SetShaderParameters(DeviceContext context, Matrix worldMatrix, Matrix viewMatrix,
                                         Matrix projectionMatrix, ShaderResourceView texture, Color4? diffuseColor, Color4? ambientColor,
                                         Vector3? lightDirection, Vector3 cameraPosition, Material material)
        {
            _worldMatrix.SetMatrix(worldMatrix);
            _viewMatrix.SetMatrix(viewMatrix);
            _projectionMatrix.SetMatrix(projectionMatrix);
            

            //var s = Util.GetArray(material);

            using (var dataStream = new DataStream(Marshal.SizeOf(material), false, false))
            {
                _material.SetRawValue(dataStream, Material.Stride);
            }
            
            _cameraPosition.Set(cameraPosition);

            _shaderTexture.SetResource(texture);
            _sampleType.SetSampler(0, _samplerState);

            context.PixelShader.SetShaderResource(0, texture);
        }

        public void Dispose()
        {
            _shader.Dispose();
            _fx.Dispose();
            _layout.Dispose();
            _samplerState.Dispose();
        }
    }
}
