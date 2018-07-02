using System.Runtime.InteropServices;
using SharpDX;

namespace DungeonHack.Lights
{
    [StructLayout(LayoutKind.Sequential, Size=64)]
    public struct DirectionalLight
    {
        public Color4 Ambient;
        public Color4 Diffuse;
        public Color4 Specular;
        public Vector3 Direction;
        public float pad;
    }
}
