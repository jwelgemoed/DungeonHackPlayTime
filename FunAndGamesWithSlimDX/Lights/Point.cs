using System.Runtime.InteropServices;
using SharpDX;

namespace DungeonHack.Lights
{
    [StructLayout(LayoutKind.Sequential, Size=80)]
    public struct PointLight
    {
        public Color4 Ambient;
        public Color4 Diffuse;
        public Color4 Specular;
        public Vector3 Position;
        public float Range;
        public Vector3 Attentuation;
        public float pad;
    }
}
