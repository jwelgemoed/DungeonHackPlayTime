using System.Runtime.InteropServices;
using SharpDX;

namespace DungeonHack.Lights
{
    [StructLayout(LayoutKind.Sequential)]
    public struct PointLight
    {
        public Color4 Color;
        public Vector3 Position;
        public float Range;
        public Vector3 Attentuation;
        public float pad;
        public Matrix LightCalculations;
    }
}
