using DungeonHack.Lights;
using SharpDX;
using System.Runtime.InteropServices;

namespace DungeonHack.DirectX.ConstantBuffer
{
    [StructLayout(LayoutKind.Sequential, Size=272)]
    public struct ConstantBufferPerFrame
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1)]
        public DirectionalLight[] DirectionalLight;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1)]
        public Spotlight[] SpotLight;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
        public PointLight[] PointLight;
        public Vector3 CameraPosition;
        public float FogStart;
        public float FogEnd;
        public int UseNormalMap;
        public float gMaxTessDistance;
        public float gMinTessDistance;
        public float gMinTessFactor;
        public float gMaxTessFactor;
        public Vector2 pad;
    }
}
