using FunAndGamesWithSharpDX.DirectX;
using FunAndGamesWithSharpDX.Engine;
using SharpDX.Direct3D11;
using System;

namespace FunAndGamesWithSharpDX.Entities
{
    public class Sprite : IDisposable
    {
        private Texture _texture;
        private SharpDX.Vector2 _size;
        private SharpDX.Vector2 _position;

        public Sprite(Device device, string spriteFileName, int x, int y, int sizeX, int sizeY)
        {
            _texture = new Texture();
            var basePath = ConfigManager.ResourcePath;
            var fileNamePath = basePath + @"\Resources\" + spriteFileName;
            _texture.LoadTexture(device, fileNamePath);
            _position = new SharpDX.Vector2(x, y);
            _size = new SharpDX.Vector2(sizeX, sizeY);
        }

        public void SetPosition(int x, int y)
        {
            _position.X = x;
            _position.Y = y;
        }

        public void SetSize(int sizeX, int sizeY)
        {
            _size.X = sizeX;
            _size.Y = sizeY;
        }

        public void Draw()
        {
            SpriteRenderer.Draw(_texture.TextureData, _position, _size);
        }

        public void Draw(int x, int y)
        {
            SetPosition(x, y);
            SpriteRenderer.Draw(_texture.TextureData, _position, _size);
        }

        public void Draw(int x, int y, int sizeX, int sizeY)
        {
            SetPosition(x, y);
            SetSize(sizeX, sizeY);
            SpriteRenderer.Draw(_texture.TextureData, _position, _size);
        }

        public void Dispose()
        {
            if (_texture != null)
                _texture.Dispose();
        }
    }
}