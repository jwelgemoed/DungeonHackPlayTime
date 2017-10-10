using SharpDX;
using System.Runtime.InteropServices;

namespace FunAndGamesWithSharpDX.Lights
{
    [StructLayout(LayoutKind.Sequential)]
    public struct PointLight
    {
        public Color4 Ambient { get; }
        public Color4 Diffuse { get; }
        public Color4 Specular { get; }
        public Vector3 Position { get; }
        public float Range { get; }
        public Vector3 Attentuation { get; }
        public float Pad { get; }

        public static readonly int Stride = Marshal.SizeOf(typeof(PointLight));

        public PointLight(Color4 ambient, Color4 diffuse, Color4 specular, Vector3 position, float range, Vector3 attentuation)
        {
            Ambient = ambient;
            Diffuse = diffuse;
            Specular = specular;
            Position = position;
            Range = range;
            Attentuation = attentuation;
            Pad = 0.0f;
        }
    }
}
