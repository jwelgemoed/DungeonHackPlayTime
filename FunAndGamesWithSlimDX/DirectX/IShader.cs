using FunAndGamesWithSlimDX.Entities;
using FunAndGamesWithSlimDX.Lights;
using SlimDX;
using SlimDX.Direct3D11;
using System;

namespace FunAndGamesWithSlimDX.DirectX
{
    public interface IShader : IDisposable
    {
        void Initialize(Device device);

        void SetSelectedShaderEffect(Device device, string technique);

        void Render(DeviceContext context, int indexCount, Matrix worldMatrix, Matrix viewMatrix,
                                    Matrix projectionMatrix);

        void Render(DeviceContext context, int indexCount, Matrix worldMatrix, Matrix viewMatrix,
                    Matrix projectionMatrix, ShaderResourceView texture);

        void Render(DeviceContext context, int indexCount, Matrix worldMatrix, Matrix viewMatrix,
                    Matrix projectionMatrix, ShaderResourceView texture, Color4 diffuseColor,
                    Vector3 lightDirection);

        void Render(DeviceContext context, int indexCount, Matrix worldMatrix, Matrix viewMatrix,
                           Matrix projectionMatrix, ShaderResourceView texture, Vector3 cameraPosition, Material material);

        void Render(DeviceContext context, int indexCount, Matrix worldMatrix, Matrix viewMatrix,
                    Matrix projectionMatrix, ShaderResourceView texture, Color4 diffuseColor, Color4 ambientColor,
                    Vector3 lightDirection, Vector3 cameraPosition);

        void RenderLights(DirectionalLight directionalLight, PointLight pointLight, Spotlight spotLight);
    }
}