using DungeonHack.Lights;
using System.Runtime.InteropServices;

namespace DungeonHack.DirectX
{
    [StructLayout(LayoutKind.Sequential)]
    public struct ConstantBufferDirectionalLight
    {
        public DirectionalLight DirectionalLight { get; set; }
    }
}
