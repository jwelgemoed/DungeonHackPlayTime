using SlimDX;
using SlimDX.Direct3D11;
using SlimDX.DirectWrite;

namespace FunAndGamesWithSlimDX.Engine
{
    public static class FontRenderer
    {
        private static SpriteTextRenderer.SlimDX.TextBlockRenderer _textBlockRenderer;
        private static SpriteTextRenderer.SlimDX.SpriteRenderer _spriteRenderer;
        private static object _lock = new object();

        public static void Initialize(Device device, string fontName, FontWeight fontWeight, FontStyle fontStyle, FontStretch fontStretch, float fontSize)
        {
            _spriteRenderer = new SpriteTextRenderer.SlimDX.SpriteRenderer(device);
            _textBlockRenderer = new SpriteTextRenderer.SlimDX.TextBlockRenderer(_spriteRenderer, fontName, fontWeight, fontStyle, fontStretch, fontSize);
        }

        public static void DrawText(string text, SlimDX.Vector2 position, Color4 color)
        {
            lock (_lock)
            {
                _textBlockRenderer.DrawString(text, position, color);
            }
        }

        public static void FinalizeDraw()
        {
            _spriteRenderer.Flush();
        }
    }
}