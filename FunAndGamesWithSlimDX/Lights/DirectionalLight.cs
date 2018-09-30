using System.Runtime.InteropServices;
using SharpDX;

namespace DungeonHack.Lights
{
    [StructLayout(LayoutKind.Sequential)]
    public struct DirectionalLight
    {
        public Color4 Color;
        public Vector3 Direction;
        public float pad;
    }
}
