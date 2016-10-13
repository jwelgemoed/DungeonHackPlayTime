using FunAndGamesWithSlimDX.Entities;
using SlimDX;
using SlimDX.D3DCompiler;
using SlimDX.Direct3D11;
using System;
using System.IO;
using Device = SlimDX.Direct3D11.Device;
using FunAndGamesWithSlimDX.Lights;

namespace FunAndGamesWithSlimDX.DirectX
{
    public class LightShader : IShader, IDisposable
    {
        private Device _device;
        private InputLayout _layout;
        private Effect _fx;
        private EffectTechnique _technique;
        private EffectConstantBuffer _matrixBuffer;
        private EffectConstantBuffer _lightBuffer;
        private SamplerState _samplerState;
        private ShaderBytecode _shader;

        public void Initialize(Device device)
        {
            _device = device;

            var elements = Vertex.GetInputElements();

            var basePath = Directory.GetCurrentDirectory();

            var fileName = basePath + @"\FX\Light.fx";

            _shader = ShaderBytecode.CompileFromFile(fileName, null, "fx_5_0",
                                                     ShaderFlags.SkipOptimization | ShaderFlags.Debug,
                                                     EffectFlags.None);

            _fx = new Effect(device, _shader);

            _technique = _fx.GetTechniqueByName("LightTech");

            _matrixBuffer = _fx.GetConstantBufferByName("MatrixBuffer").AsConstantBuffer();
            _lightBuffer = _fx.GetConstantBufferByName("LightBuffer").AsConstantBuffer();

            var passDescription = _technique.GetPassByIndex(0).Description;

            _layout = new InputLayout(device, passDescription.Signature, elements);

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
        }

        public void SetSelectedShaderEffect(Device device, string technique)
        {
            throw new NotImplementedException();
        }

        public void Render(DeviceContext context, int indexCount, Matrix worldMatrix, Matrix viewMatrix, Matrix projectionMatrix)
        {
            throw new NotImplementedException();
        }

        public void Render(DeviceContext context, int indexCount, Matrix worldMatrix, Matrix viewMatrix, Matrix projectionMatrix,
                           ShaderResourceView texture)
        {
            throw new NotImplementedException();
        }

        public void Render(DeviceContext context, int indexCount, Matrix worldMatrix, Matrix viewMatrix,
                          Matrix projectionMatrix, ShaderResourceView texture, Color4 diffuseColor,
                                        Vector3 lightDirection)
        {
            SetShaderParameters(context, worldMatrix, viewMatrix, projectionMatrix, texture, diffuseColor, Colors.White, lightDirection);

            RenderShader(context, indexCount);
        }

        public void Render(DeviceContext context, int indexCount, Matrix worldMatrix, Matrix viewMatrix,
                           Matrix projectionMatrix, ShaderResourceView texture, Color4 diffuseColor, Color4 ambientColor,
                                         Vector3 lightDirection, Vector3 cameraPosition)
        {
            SetShaderParameters(context, worldMatrix, viewMatrix, projectionMatrix, texture, diffuseColor, ambientColor, lightDirection);

            RenderShader(context, indexCount);
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
                                         Matrix projectionMatrix, ShaderResourceView texture, Color4 diffuseColor,
                                         Color4 ambientColor, Vector3 lightDirection)
        {
            _matrixBuffer.GetMemberByName("worldMatrix").AsMatrix().SetMatrix(worldMatrix);
            _matrixBuffer.GetMemberByName("viewMatrix").AsMatrix().SetMatrix(viewMatrix);
            _matrixBuffer.GetMemberByName("projectionMatrix").AsMatrix().SetMatrix(projectionMatrix);

            _lightBuffer.GetMemberByName("ambientColor").AsVector().Set(ambientColor);
            _lightBuffer.GetMemberByName("diffuseColor").AsVector().Set(diffuseColor);
            _lightBuffer.GetMemberByName("lightDirection").AsVector().Set(lightDirection);

            _fx.GetVariableByName("shaderTexture").AsResource().SetResource(texture);
            _fx.GetVariableByName("SampleType").AsSampler().SetSamplerState(0, _samplerState);

            context.PixelShader.SetShaderResource(texture, 0);
        }

        public void Dispose()
        {
            _shader.Dispose();
            _fx.Dispose();
            _layout.Dispose();
            _samplerState.Dispose();
        }

        public void Render(DeviceContext context, int indexCount, Matrix worldMatrix, Matrix viewMatrix, Matrix projectionMatrix, ShaderResourceView texture, Vector3 cameraPosition, Material material)
        {
            throw new NotImplementedException();
        }

        public void RenderLights(DirectionalLight directionalLight, PointLight pointLight, Spotlight spotLight)
        {
            throw new NotImplementedException();
        }
    }
}
