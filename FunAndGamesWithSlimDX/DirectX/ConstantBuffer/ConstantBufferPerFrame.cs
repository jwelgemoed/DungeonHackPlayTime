using FunAndGamesWithSharpDX.Entities;
using SharpDX;
using System.Runtime.InteropServices;
using FunAndGamesWithSharpDX.Lights;

namespace DungeonHack.DirectX.ConstantBuffer
{
    [StructLayout(LayoutKind.Sequential)]
    public struct ConstantBufferPerFrame
    {
        public DirectionalLight DirectionalLight;
        public PointLight PointLight;
        public Spotlight SpotLight;
        public Vector3 CameraPosition;
        public float Pad;

        public static readonly int Stride = Marshal.SizeOf(typeof(ConstantBufferPerFrame));
    }
}
