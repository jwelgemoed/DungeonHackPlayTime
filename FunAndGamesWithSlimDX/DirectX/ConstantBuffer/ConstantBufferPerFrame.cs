using DungeonHack.Lights;
using SharpDX;
using System.Runtime.InteropServices;

namespace DungeonHack.DirectX.ConstantBuffer
{
    //Pointlight size = 80 = 272
    [StructLayout(LayoutKind.Sequential, Size=832)]
    public struct ConstantBufferPerFrame
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1)]
        public DirectionalLight[] DirectionalLight;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1)]
        public Spotlight[] SpotLight;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
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
