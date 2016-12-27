using SlimDX;

namespace DungeonHack
{
    public static class VectorExtensions
    {

        public static Vector3 ToVector3(this Vector4 input)
        {
            return new Vector3(input.X, input.Y, input.Z);
        }

        public static Vector4 ToVector4(this Vector3 input)
        {
            return new Vector4(input.X, input.Y, input.Z, 1.0f);
        }
    }
}
