using SlimDX;
using SlimDX.D3DCompiler;
using SlimDX.Direct3D11;
using SlimDX.DXGI;
using System;
using System.IO;
using Device = SlimDX.Direct3D11.Device;
using FunAndGamesWithSlimDX.Entities;
using FunAndGamesWithSlimDX.Lights;

namespace FunAndGamesWithSlimDX.DirectX
{
    public struct MatrixBufferType
    {
        public Matrix gWorldViewProj;
    }

    public class ColorShader : IDisposable, IShader
    {
        private InputLayout _layout;
        private Effect _fx;
        private EffectTechnique _technique;
        private EffectMatrixVariable _fxWorldProjectionView;

        public void Initialize(Device device)
        {
            var elements = new[]
                {
                    new InputElement("POSITION", 0, Format.R32G32B32A32_Float, 0),
                    new InputElement("COLOR", 0, Format.R32G32B32A32_Float, 16,0)
                        {
                            Classification = InputClassification.PerVertexData
                        },
                };

            var basePath = Directory.GetCurrentDirectory();

            var fileName = basePath + @"\FX\colorShader.fx";

            var shader = ShaderBytecode.CompileFromFile(fileName, null, "fx_5_0",
                                                      ShaderFlags.SkipOptimization | ShaderFlags.Debug,
                                                      EffectFlags.None);

            _fx = new Effect(device, shader);

            _technique = _fx.GetTechniqueByName("ColorTech");
            _fxWorldProjectionView = _fx.GetVariableByName("gWorldViewProj").AsMatrix();

            var passDescription = _technique.GetPassByIndex(0).Description;

            _layout = new InputLayout(device, passDescription.Signature, elements);
        }

        public void SetSelectedShaderEffect(Device device, string technique)
        {
            throw new NotImplementedException();
        }

        public void Render(DeviceContext context, int indexCount, Matrix worldMatrix, Matrix viewMatrix,
                           Matrix projectionMatrix)
        {
            SetShaderParameters(worldMatrix, viewMatrix, projectionMatrix);

            RenderShader(context, indexCount);
        }

        public void Render(DeviceContext context, int indexCount, Matrix worldMatrix, Matrix viewMatrix, Matrix projectionMatrix,
                           ShaderResourceView texture)
        {
            Render(context, indexCount, worldMatrix, viewMatrix, projectionMatrix);
        }

        public void Render(DeviceContext context, int indexCount, Matrix worldMatrix, Matrix viewMatrix, Matrix projectionMatrix,
                           ShaderResourceView texture, Color4 diffuseColor, Vector3 lightDirection)
        {
            Render(context, indexCount, worldMatrix, viewMatrix, projectionMatrix);
        }

        public void Render(DeviceContext context, int indexCount, Matrix worldMatrix, Matrix viewMatrix, Matrix projectionMatrix,
                           ShaderResourceView texture, Color4 diffuseColor, Color4 ambientColor, Vector3 lightDirection, Vector3 cameraPosition)
        {
            Render(context, indexCount, worldMatrix, viewMatrix, projectionMatrix);
        }

        private void RenderShader(DeviceContext context, int indexCount)
        {
            context.InputAssembler.InputLayout = _layout;

            var techDesc = _technique.Description;

            for (int i = 0; i < techDesc.PassCount; i++)
            {
                _technique.GetPassByIndex(i).Apply(context);

                context.DrawIndexed(indexCount, 0, 0);
            }
        }

        private void SetShaderParameters(Matrix worldMatrix, Matrix viewMatrix, Matrix projectionMatrix)
        {
            var projection = viewMatrix * projectionMatrix;
            var worldViewProj = worldMatrix * projection;
            worldViewProj = Matrix.Transpose(worldViewProj);

            _fxWorldProjectionView.SetMatrixTranspose(worldViewProj);
        }

        public void Dispose()
        {
            _layout.Dispose();
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