using DungeonHack.Lights;
using SharpDX;
using System.Runtime.InteropServices;

namespace DungeonHack.DirectX.ConstantBuffer
{
    [StructLayout(LayoutKind.Explicit, Size = 48)]//96, Pack =16)]
    public struct ConstantBufferPerFrame
    {
        [FieldOffset(0), MarshalAs(UnmanagedType.ByValArray, SizeConst = 1)]
        public DirectionalLight[] DirectionalLight;
       // [FieldOffset(128), MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
       // public Spotlight[] SpotLight;
       // [FieldOffset(320), MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
       // public PointLight[] PointLight;
        //[FieldOffset(480)]
        [FieldOffset(16)]//64)]
        public Vector3 CameraPosition;
        //[FieldOffset(492)]
        [FieldOffset(28)]//76)]
        public float FogStart;
        //[FieldOffset(496)]
        [FieldOffset(32)]//80)]
        public float FogEnd;
        //[FieldOffset(500)]
        [FieldOffset(36)]//84)]
        public Vector3 Pad;

        //public static readonly int Stride = Marshal.SizeOf(typeof(ConstantBufferPerFrame));
    }
}
