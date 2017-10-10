using FunAndGamesWithSharpDX.Engine;
using FunAndGamesWithSharpDX;
using SharpDX;
using SharpDX.Direct3D11;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace FunAndGamesWithSharpDX.Entities
{
    public class Console : IDisposable
    {
        private Vector2 _topLeft;
        private Vector2 _topRight;
        private Vector2 _bottomLeft;
        private Vector2 _bottomRight;
        private Vector2 _size;
        private ShaderResourceView _backgroundTexture;
        private List<string> _buffer;
        private int _bufferSize = 1000;
        private int _currentLine = 0;
        private int _windowSize = 10;
        private Color4 _consoleColor;

        public Console(ShaderResourceView backgroundTexture, Vector2 position, Vector2 size, int bufferSize, Color4 consoleColor)
        {
            _size = size;
            _backgroundTexture = backgroundTexture;
            _topLeft = new Vector2(position.X, position.Y);
            _topRight = new Vector2(position.X + size.X, position.Y);
            _bottomLeft = new Vector2(position.X, position.Y + size.Y);
            _bottomRight = new Vector2(position.X + size.X, position.Y + size.Y);
            _buffer = new List<string>(bufferSize);
            _consoleColor = consoleColor;
        }

        public void WriteLine(string message)
        {
            if (_buffer.Count >= _bufferSize)
            {
                _buffer.Clear();
                _currentLine = 0;
            }

            _buffer.Add("["+_buffer.Count + "] " + message);
            if (_buffer.Count > _windowSize)
                _currentLine += 1;
        }

        public void Draw()
        {
            if (_backgroundTexture != null)
                SpriteRenderer.Draw(_backgroundTexture, _topLeft, _size);

            int startLine = _currentLine;
            int endLine = _currentLine + _windowSize;
            int counter = 0;
            Vector2 position = _bottomLeft;
            position.X = position.X + 5;
            position.Y = position.Y + 5;

            if (endLine >= _buffer.Count)
                endLine = _buffer.Count;

            for (int i=endLine-1; i >= startLine; i--)
            {
                counter++;
                position.Y = _bottomLeft.Y - (counter*12.2f);
                               
                int index = i - 1;

                if (index >= 0)
                    FontRenderer.DrawText(_buffer[index], position, _consoleColor);
            }
        }

        public void Dispose()
        {
            if (_backgroundTexture != null)
                _backgroundTexture.Dispose();
        }
    }
}
