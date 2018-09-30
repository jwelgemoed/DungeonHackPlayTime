using SharpDX;
using System.Runtime.InteropServices;

namespace DungeonHack.DirectX.ConstantBuffer
{
    [StructLayout(LayoutKind.Sequential)]
    public struct ConstantBufferDeferredInfo
    {
        public Vector4 PerspectiveValues;
        public Matrix ViewInv; 
    }
}
