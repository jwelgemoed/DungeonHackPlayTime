using DungeonHack.Lights;
using System.Runtime.InteropServices;

namespace DungeonHack.DirectX.ConstantBuffer
{
    [StructLayout(LayoutKind.Sequential)]
    public struct ConstantBufferAmbientLight
    {
        public AmbientLight AmbientLight { get; set; }
    }
}
