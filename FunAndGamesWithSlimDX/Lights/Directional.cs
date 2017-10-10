using SharpDX;
using System.Runtime.InteropServices;

namespace FunAndGamesWithSharpDX.Lights
{
    [StructLayout(LayoutKind.Sequential)]
    public struct DirectionalLight
    {
        public Color4 Ambient { get; }
        public Color4 Diffuse { get; }
        public Color4 Specular { get; }
        public Vector3 Direction { get; }
        public float Pad { get; }

        public static readonly int Stride = Marshal.SizeOf(typeof(DirectionalLight));

        public DirectionalLight(Color4 ambient, Color4 diffuse, Color4 specular, Vector3 direction)
        {
            Ambient = ambient;
            Diffuse = diffuse;
            Specular = specular;
            Direction = direction;
            Pad = 0.0f;
        }

    }
}
