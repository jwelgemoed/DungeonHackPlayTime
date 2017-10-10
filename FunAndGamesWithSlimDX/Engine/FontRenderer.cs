using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.DirectWrite;

namespace FunAndGamesWithSharpDX.Engine
{
    public static class FontRenderer
    {
        private static SpriteTextRenderer.SharpDX.TextBlockRenderer _textBlockRenderer;
        private static SpriteTextRenderer.SharpDX.SpriteRenderer _spriteRenderer;
        private static object _lock = new object();

        public static void Initialize(Device device, string fontName, FontWeight fontWeight, FontStyle fontStyle, FontStretch fontStretch, float fontSize)
        {
           // _spriteRenderer = new SpriteTextRenderer.SharpDX.SpriteRenderer(device);
           // _textBlockRenderer = new SpriteTextRenderer.SharpDX.TextBlockRenderer(_spriteRenderer, fontName, fontWeight, fontStyle, fontStretch, fontSize);
        }

        public static void DrawText(string text, SharpDX.Vector2 position, Color4 color)
        {
            lock (_lock)
            {
             //   _textBlockRenderer.DrawString(text, position, color);
            }
        }

        public static void FinalizeDraw()
        {
          //  _spriteRenderer.Flush();
        }
    }
}