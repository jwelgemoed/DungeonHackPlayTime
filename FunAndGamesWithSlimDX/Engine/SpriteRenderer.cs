using SlimDX.Direct3D11;
using SpriteTextRenderer;

namespace FunAndGamesWithSlimDX.Engine
{
    public static class SpriteRenderer
    {
        private static SpriteTextRenderer.SlimDX.SpriteRenderer _spriteRenderer;

        public static void Initialize(Device device)
        {
            _spriteRenderer = new SpriteTextRenderer.SlimDX.SpriteRenderer(device);
        }

        public static void Draw(ShaderResourceView texture, SlimDX.Vector2 position, SlimDX.Vector2 size)
        {
            _spriteRenderer.Draw(texture, position, size, CoordinateType.Absolute);
        }

        public static void FinalizeDraw()
        {
            _spriteRenderer.Flush();
        }

        public static void Dispose()
        {
            _spriteRenderer.Dispose();
        }
    }
}