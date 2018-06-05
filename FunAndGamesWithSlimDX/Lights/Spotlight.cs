using System.Runtime.InteropServices;
using SharpDX;

namespace DungeonHack.Lights
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Spotlight
    {
        public Color4 Ambient { get; }
        public Color4 Diffuse { get; }
        public Color4 Specular { get; }
        public Vector3 Position { get; }
        public float Range { get; }
        public Vector3 Direction { get; }
        public float Spot { get; }
        public Vector3 Attentuation { get; }
        public float Pad { get; }

        public static readonly int Stride = Marshal.SizeOf(typeof(Spotlight));

        public Spotlight(Color4 ambient, Color4 diffuse, Color4 specular, Vector3 position, float range, Vector3 direction,
                float spot, Vector3 attentuation)
        {
            Ambient = ambient;
            Diffuse = diffuse;
            Specular = specular;
            Position = position;
            Range = range;
            Direction = direction;
            Spot = spot;
            Attentuation = attentuation;
            Pad = 0.0f;
        }
    }
}
