using FunAndGamesWithSlimDX.Engine;
using FunAndGamesWithSlimDX.Entities;
using FunAndGamesWithSlimDX.Lights;
using SlimDX;
using SlimDX.D3DCompiler;
using SlimDX.Direct3D11;
using System;
using Device = SlimDX.Direct3D11.Device;

namespace FunAndGamesWithSlimDX.DirectX
{
    public class Shader : IShader, IDisposable
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

        private EffectResourceVariable _shaderTexture;
        private EffectSamplerVariable _sampleType;

        public Shader()
        {
        }

        public Shader(Device device)
        {
            _device = device;
        }

        public void Initialize(Device device)
        {
            _device = device;

            _elements = Vertex.GetInputElements();

            var basePath = ConfigManager.ResourcePath;

            var fileName = basePath + @"\FX\ShaderEffects.fx";

            string errors;

            _shader = ShaderBytecode.CompileFromFile(fileName, null, "fx_5_0",
                                                     ShaderFlags.SkipOptimization | ShaderFlags.Debug,
                                                     EffectFlags.None, null, new IncludeFX(), out errors);

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

            var samplerDesc = new SamplerDescription()
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

            _samplerState = SamplerState.FromDescription(device, samplerDesc);

            _worldMatrix = _cbPerObject.GetMemberByName("worldMatrix").AsMatrix();
            _viewMatrix = _cbPerObject.GetMemberByName("viewMatrix").AsMatrix();
            _projectionMatrix = _cbPerObject.GetMemberByName("projectionMatrix").AsMatrix();

            _gDirLight = _cbPerFrame.GetMemberByName("gDirLight");
            _gPointLight = _cbPerFrame.GetMemberByName("gPointLight");
            _gSpotLight = _cbPerFrame.GetMemberByName("gSpotLight");

            _material = _cbPerObject.GetMemberByName("material");

            _cameraPosition = _cbPerFrame.GetMemberByName("cameraPosition").AsVector();

            _shaderTexture = _fx.GetVariableByName("shaderTexture").AsResource();
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
            var d = Util.GetArray(directionalLight);
            //Array.Copy(d, 0, _diretionalLightArray, 0, Directional.Stride);

            _gDirLight.SetRawValue(new DataStream(d, false, false), DirectionalLight.Stride);

            var p = Util.GetArray(pointLight);
            _gPointLight.SetRawValue(new DataStream(p, false, false), PointLight.Stride);

            var s = Util.GetArray(spotLight);
            _gSpotLight.SetRawValue(new DataStream(s, false, false), Spotlight.Stride);
        }

        private void RenderShader(DeviceContext context, int indexCount)
        {
            context.InputAssembler.InputLayout = _layout;

            var techDesc = _technique.Description;

            for (int i = 0; i < techDesc.PassCount; i++)
            {
                _technique.GetPassByIndex(i).Apply(context);

                context.PixelShader.SetSampler(_samplerState, 0);

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
            _sampleType.SetSamplerState(0, _samplerState);

            context.PixelShader.SetShaderResource(texture, 0);
        }

        private void SetShaderParameters(DeviceContext context, Matrix worldMatrix, Matrix viewMatrix,
                                         Matrix projectionMatrix, ShaderResourceView texture, Color4? diffuseColor, Color4? ambientColor,
                                         Vector3? lightDirection, Vector3 cameraPosition, Material material)
        {
            _worldMatrix.SetMatrix(worldMatrix);
            _viewMatrix.SetMatrix(viewMatrix);
            _projectionMatrix.SetMatrix(projectionMatrix);

            var s = Util.GetArray(material);
            _material.SetRawValue(new DataStream(s, false, false), Material.Stride);
            
            _cameraPosition.Set(cameraPosition);

            _shaderTexture.SetResource(texture);
            _sampleType.SetSamplerState(0, _samplerState);

            context.PixelShader.SetShaderResource(texture, 0);
        }

        public void Dispose()
        {
            _shader.Dispose();
            _fx.Dispose();
            _layout.Dispose();
            _samplerState.Dispose();
        }

        public void Render(DeviceContext context, int indexCount, Matrix worldMatrix, Matrix viewMatrix, Matrix projectionMatrix, ShaderResourceView texture, Color4 diffuseColor, Color4 ambientColor, Vector3 lightDirection, Vector3 cameraPosition)
        {
            throw new NotImplementedException();
        }

        public void Render(DeviceContext context, int indexCount, Matrix worldMatrix, Matrix viewMatrix, Matrix projectionMatrix, ShaderResourceView texture)
        {
            throw new NotImplementedException();
        }
    }
}
