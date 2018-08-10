using SharpDX.Direct2D1;
using System.Collections.Generic;

namespace DungeonHack.DirectX
{
    public class TextRenderer
    {
        public List<TextStringLocation> StringBuffer { get; internal set; }
        public Brush Brush { get; internal set; }
        public string FontFamily { get; internal set; }
        public float FontSize { get; internal set; }
    }

    public struct TextStringLocation
    {
        public string Text;

        public int X;

        public int Y;
    }
}