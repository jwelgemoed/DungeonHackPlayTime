using FunAndGamesWithSharpDX.Entities;
using SharpDX;
using System.Runtime.InteropServices;

namespace DungeonHack.DirectX.ConstantBuffer
{
    [StructLayout(LayoutKind.Sequential)]
    public struct ConstantBufferPerObject
    {
        public Matrix WorldMatrix;
        public Matrix ViewMatrix;
        public Matrix ProjectionMatrix;
        public Material Material;

        public static readonly int Stride = Marshal.SizeOf(typeof(ConstantBufferPerObject));
    }
}
