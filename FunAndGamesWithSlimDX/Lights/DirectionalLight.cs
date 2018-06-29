using System.Runtime.InteropServices;
using SharpDX;

namespace DungeonHack.Lights
{
    [StructLayout(LayoutKind.Explicit, Size = 16, Pack =16)]
    public struct DirectionalLight
    {
        [FieldOffset(0)]
        public Vector4 Ambient;
        //[FieldOffset(16)]
        //public Vector4 Diffuse;
        //[FieldOffset(32)]
        //public Vector4 Specular;
        //[FieldOffset(48)]
        //public Vector3 Direction;
        //[FieldOffset(60)]
        //public float Pad;
    }
}
