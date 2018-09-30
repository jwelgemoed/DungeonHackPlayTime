using SharpDX;
using System.Runtime.InteropServices;

namespace DungeonHack.Lights
{
    [StructLayout(LayoutKind.Sequential, Size = 32)]
    public struct AmbientLight
    {
        public Vector3 AmbientDown;
        public float pad1;
        public Vector3 AmbientUp;
        public float pad2;
    }
}
