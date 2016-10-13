using SlimDX;
using System.Runtime.InteropServices;

namespace FunAndGamesWithSlimDX.Entities
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Material
    {
        public Color4 Ambient;
        public Color4 Diffuse;
        public Color4 Specular;
        public Color4 Reflect;

        public static int Stride = Marshal.SizeOf(typeof(Material));
    }
}
