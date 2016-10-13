using FunAndGamesWithSlimDX.DirectX;
using FunAndGamesWithSlimDX.Engine;
using SlimDX;
using SlimDX.D3DCompiler;
using SlimDX.Direct3D11;
using SlimDX.DXGI;
using System;
using System.Runtime.InteropServices;
using Buffer = SlimDX.Direct3D11.Buffer;
using Device = SlimDX.Direct3D11.Device;
using FunAndGamesWithSlimDX.Entities;
using FunAndGamesWithSlimDX.Lights;

namespace FunAndGamesWithSlimDX.FX
{
    public class TextureShaderEffects : IDisposable, IShader
    {
        private VertexShader VertexShader { get; set; }
        private PixelShader PixelShader { get; set; }
        private InputLayout InputLayout { get; set; }

        private SamplerDescription TextureSamplerDesc { get; set; }
        private SamplerState TextureSamplerState { get; set; }

        private Buffer MatrixBufferDesc { get; set; }
        private Buffer CameraBufferDesc { get; set; }

        private Device Device { get; set; }

        //Effect variables
        private bool UseEffectsFramework { get; set; }
        private Effect Effect { get; set; }
        private EffectTechnique EffectTechnique { get; set; }
        private EffectConstantBuffer EffectMatrixBuffer { get; set; }
        private EffectConstantBuffer EffectCameraBuffer { get; set; }
        private EffectMatrixVariable EffectWorldMatrixVariable { get; set; }
        private EffectMatrixVariable EffectViewMatrixVariable { get; set; }
        private EffectMatrixVariable EffectProjectionMatrixVariable { get; set; }
        private EffectVectorVariable EffectCameraPositionVariable { get; set; }
        private EffectResourceVariable EffectTextureVariable { get; set; }
        private EffectSamplerVariable EffectSamplerVariable { get; set; }

        private ShaderBytecode EffectShader { get; set; }


        [StructLayout(LayoutKind.Sequential)]
        internal struct MatrixBuffer
        {
            public Matrix world;
            public Matrix view;
            public Matrix projection;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct CameraBuffer
        {
            public Vector3 cameraPosition;
            public float padding;
        }

        public TextureShaderEffects(Device device)
        {
            Device = device;
        }

        public bool Initialize()
        {
            try
            {
                var fxFileName = ConfigManager.ResourcePath + @"FX\Shaders\Texture.fx";

                var fxShaderByteCode = ShaderBytecode.CompileFromFile(fxFileName, null, "fx_5_0", ShaderFlags.None, EffectFlags.None);

                Effect = new Effect(Device, fxShaderByteCode);

                EffectMatrixBuffer = Effect.GetConstantBufferByName("MatrixBuffer").AsConstantBuffer();
                EffectCameraBuffer = Effect.GetConstantBufferByName("CameraBuffer").AsConstantBuffer();

                EffectWorldMatrixVariable = Effect.GetVariableByName("worldMatrix").AsMatrix();
                EffectViewMatrixVariable = Effect.GetVariableByName("viewMatrix").AsMatrix();
                EffectProjectionMatrixVariable = Effect.GetVariableByName("projectionMatrix").AsMatrix();

                EffectCameraPositionVariable = Effect.GetVariableByName("cameraPosition").AsVector();

                EffectTextureVariable = Effect.GetVariableByName("shaderTexture").AsResource();
                EffectSamplerVariable = Effect.GetVariableByName("SampleType").AsSampler();

                EffectTechnique = Effect.GetTechniqueByName("TextureTech");

                var passDescription = EffectTechnique.GetPassByIndex(0).Description;

                InputLayout = new InputLayout(Device, passDescription.Signature, CreateInputElements());

                TextureSamplerDesc = CreateSamplerDescription();
                TextureSamplerState = SamplerState.FromDescription(Device, TextureSamplerDesc);

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public void SetSelectedShaderEffect(Device device, string technique)
        {
            throw new NotImplementedException();
        }

        private bool Render(DeviceContext context, int indexCount, Matrix worldMatrix, Matrix viewMatrix, Matrix projectionMatrix, ShaderResourceView texture, Vector3 cameraPosition)
        {
            if (!SetShaderParameters(context, worldMatrix, viewMatrix, projectionMatrix, texture, cameraPosition))
                return false;

            RenderShader(context, indexCount);

            return true;
        }

        private bool SetShaderParameters(DeviceContext context, Matrix worldMatrix, Matrix viewMatrix, Matrix projectionMatrix, ShaderResourceView texture, Vector3 cameraPosition)
        {
            try
            {
                EffectWorldMatrixVariable.SetMatrix(worldMatrix);
                EffectViewMatrixVariable.SetMatrix(viewMatrix);
                EffectProjectionMatrixVariable.SetMatrix(projectionMatrix);

                EffectCameraPositionVariable.Set(cameraPosition);

                EffectTextureVariable.SetResource(texture);
                EffectSamplerVariable.SetSamplerState(0, TextureSamplerState);

                context.PixelShader.SetShaderResource(texture, 0);

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        private void RenderShader(DeviceContext context, int indexCount)
        {
            context.InputAssembler.InputLayout = InputLayout;

            var techDescription = EffectTechnique.Description;

            for (int i = 0; i < techDescription.PassCount; i++)
            {
                EffectTechnique.GetPassByIndex(i).Apply(context);

                context.PixelShader.SetSampler(TextureSamplerState, 0);

                context.DrawIndexed(indexCount, 0, 0);
            }
        }

        private InputElement[] CreateInputElements()
        {
            return new InputElement[]
            {
               new InputElement("POSITION", 0, Format.R32G32B32_Float, 0)
               {
                        Classification = InputClassification.PerVertexData,
               },
               new InputElement("TEXCOORD", 0, Format.R32G32_Float, 0)
               {
                        Classification = InputClassification.PerVertexData,
               },
               new InputElement("NORMAL", 0, Format.R32G32B32_Float, 0)
               {
                       Classification = InputClassification.PerVertexData
               },
            };
        }

        private SamplerDescription CreateSamplerDescription()
        {
            return new SamplerDescription()
            {
                Filter = Filter.MinMagMipLinear,
                AddressU = TextureAddressMode.Wrap,
                AddressV = TextureAddressMode.Wrap,
                AddressW = TextureAddressMode.Wrap,
                MipLodBias = 0,
                MaximumAnisotropy = 1,
                ComparisonFunction = Comparison.Always,
                BorderColor = new Color4(0, 0, 0, 0),
                MinimumLod = 0,
                MaximumLod = 0
            };
        }

        public void Dispose()
        {
            if (VertexShader != null)
            {
                VertexShader.Dispose();
                VertexShader = null;
            }

            if (PixelShader != null)
            {
                PixelShader.Dispose();
                PixelShader = null;
            }

            if (InputLayout != null)
            {
                InputLayout.Dispose();
                InputLayout = null;
            }

            if (TextureSamplerState != null)
            {
                TextureSamplerState.Dispose();
                TextureSamplerState = null;
            }

            if (MatrixBufferDesc != null)
            {
                MatrixBufferDesc.Dispose();
                MatrixBufferDesc = null;
            }

            if (CameraBufferDesc != null)
            {
                CameraBufferDesc.Dispose();
                CameraBufferDesc = null;
            }
        }

        public void Initialize(Device device)
        {
            Initialize();
        }

        public void Render(DeviceContext context, int indexCount, Matrix worldMatrix, Matrix viewMatrix, Matrix projectionMatrix)
        {
            throw new NotImplementedException();
        }

        public void Render(DeviceContext context, int indexCount, Matrix worldMatrix, Matrix viewMatrix, Matrix projectionMatrix, ShaderResourceView texture)
        {
            throw new NotImplementedException();
        }

        public void Render(DeviceContext context, int indexCount, Matrix worldMatrix, Matrix viewMatrix, Matrix projectionMatrix, ShaderResourceView texture, Color4 diffuseColor, Vector3 lightDirection)
        {
            throw new NotImplementedException();
        }

        public void Render(DeviceContext context, int indexCount, Matrix worldMatrix, Matrix viewMatrix, Matrix projectionMatrix, ShaderResourceView texture, Color4 diffuseColor, Color4 ambientColor, Vector3 lightDirection, Vector3 cameraPosition)
        {
            Render(context, indexCount, worldMatrix, viewMatrix, projectionMatrix, texture, cameraPosition);
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
