using DungeonHack.DirectX.ConstantBuffer;
using FunAndGamesWithSharpDX.DirectX;
using FunAndGamesWithSharpDX.Engine;
using SharpDX;
using SharpDX.Direct3D11;
using System;

namespace DungeonHack.DirectX.LightShaders
{
    public class SharedBuffers : IDisposable
    {
        private Device _device;
        private Camera _camera;
        private DeviceContext _immediateContext;

        public ConstantBuffer<ConstantBufferDeferredInfo> DeferredInfoConstantBuffer;
        public ConstantBufferDeferredInfo ConstantBufferDeferredInfo;

        public SharedBuffers(Renderer renderer, Camera camera)
        {
            _device = renderer.Device;
            _immediateContext = renderer.ImmediateContext;
            _camera = camera;
        }

        public void Initialize()
        {
            DeferredInfoConstantBuffer = new ConstantBuffer<ConstantBufferDeferredInfo>(_device);
            ConstantBufferDeferredInfo = new ConstantBufferDeferredInfo();
            ConstantBufferDeferredInfo.PerspectiveValues = new Vector4();

            ConstantBufferDeferredInfo.PerspectiveValues.X = 1 / _camera.ProjectionMatrix.M11;
            ConstantBufferDeferredInfo.PerspectiveValues.Y = 1 / _camera.ProjectionMatrix.M22;
            ConstantBufferDeferredInfo.PerspectiveValues.Z = _camera.ProjectionMatrix.M32;
            ConstantBufferDeferredInfo.PerspectiveValues.W = _camera.ProjectionMatrix.M22;
        }

        public void UpdateBuffersPerFrame()
        {
            ConstantBufferDeferredInfo.ViewInv = Matrix.Invert(_camera.ViewMatrix);

            DeferredInfoConstantBuffer.UpdateValue(_immediateContext, ConstantBufferDeferredInfo);
        }

        public void Dispose()
        {
            DeferredInfoConstantBuffer?.Dispose();
        }
    }
}
