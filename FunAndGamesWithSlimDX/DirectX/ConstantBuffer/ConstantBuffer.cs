using SharpDX;
using SharpDX.Direct3D11;
using System;
using System.Runtime.InteropServices;

namespace DungeonHack.DirectX.ConstantBuffer
{
    public class ConstantBuffer<T> : IDisposable
    {
        private readonly Device _device;
        private readonly SharpDX.Direct3D11.Buffer _buffer;
        private readonly DataStream _dataStream;

        public SharpDX.Direct3D11.Buffer Buffer { get { return _buffer; } }

        public ConstantBuffer(Device device)
        {
            _device = device;

            int size = Marshal.SizeOf(typeof(T));

            _buffer = new SharpDX.Direct3D11.Buffer(device, new BufferDescription
            {
                Usage = ResourceUsage.Default,
                BindFlags = BindFlags.ConstantBuffer,
                SizeInBytes = size,
                CpuAccessFlags = CpuAccessFlags.None,
                OptionFlags = ResourceOptionFlags.None,
                StructureByteStride = 0
            });

            _dataStream = new DataStream(size, true, true);
        }

        public void UpdateValue(DeviceContext context, T value)
        {
            // If no specific marshalling is needed, can use 
            // dataStream.Write(value) for better performance.
            Marshal.StructureToPtr(value, _dataStream.DataPointer, false);

            var dataBox = new DataBox(_dataStream.DataPointer, 0, 0);

            context.UpdateSubresource(dataBox, _buffer, 0);
        }

        public void Dispose()
        {
            if (_dataStream != null)
            {
                _dataStream.Dispose();
            }

            if (_buffer != null)
            {
                _buffer.Dispose();
            }
        }
    }
}
