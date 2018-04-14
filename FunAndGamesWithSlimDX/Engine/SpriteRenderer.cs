using SharpDX.Direct3D11;
using SpriteTextRenderer;

namespace FunAndGamesWithSharpDX.Engine
{
    public static class SpriteRenderer
    {
        private static SpriteTextRenderer.SharpDX.SpriteRenderer _spriteRenderer;

        public static void Initialize(Device device)
        {
            //_spriteRenderer = new SpriteTextRenderer.SharpDX.SpriteRenderer(device);
        }

        public static void Draw(ShaderResourceView texture, SharpDX.Vector2 position, SharpDX.Vector2 size)
        {
          //  _spriteRenderer.Draw(texture, position, size, CoordinateType.Absolute);
        }

        public static void FinalizeDraw()
        {
           // _spriteRenderer.Flush();
        }

        public static void Dispose()
        {
          //  _spriteRenderer.Dispose();
        }
    }
}