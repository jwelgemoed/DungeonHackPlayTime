using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using System.Runtime.InteropServices;

namespace FunAndGamesWithSharpDX.Entities
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Vertex
    {
        public Vector4 Position;
        public Vector2 Texture;
        public Vector3 Normal;

        public static int SizeOf
        {
            get { return Marshal.SizeOf(typeof(Vertex)); }
        }

        public static InputElement[] GetInputElements()
        {
            var elements = new[]
                {
                    new InputElement("POSITION", 0, Format.R32G32B32A32_Float, 0)
                        {
                            Classification = InputClassification.PerVertexData
                        },
                    new InputElement("TEXCOORD", 0, Format.R32G32_Float, 0)
                        {
                            Classification = InputClassification.PerVertexData
                        },
                     new InputElement("NORMAL", 0, Format.R32G32B32_Float, 0)
                        {
                            Classification = InputClassification.PerVertexData
                        },
                };

            return elements;
        }
    }
}