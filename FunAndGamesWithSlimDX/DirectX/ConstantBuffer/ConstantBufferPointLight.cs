using DungeonHack.Lights;
using System.Runtime.InteropServices;

namespace DungeonHack.DirectX.ConstantBuffer
{
    [StructLayout(LayoutKind.Sequential)]
    public struct ConstantBufferPointLight
    {
        public PointLight PointLight { get; set; }
    }
}
