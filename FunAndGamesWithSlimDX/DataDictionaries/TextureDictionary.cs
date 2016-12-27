using FunAndGamesWithSlimDX.DirectX;
using FunAndGamesWithSlimDX.Engine;
using SlimDX.Direct3D11;
using System.Collections.Generic;

namespace DungeonHack.DataDictionaries
{
    public class TextureDictionary
    {
        private readonly Device _device;
        private readonly Dictionary<int, Texture> _dictionary;

        public TextureDictionary(Device device)
        {
            _device = device;
            _dictionary = new Dictionary<int, Texture>();
        }
        
        public void AddTextureRelativePath(string filename)
        {
            var texture = LoadTexture(filename);

            _dictionary.Add(_dictionary.Count, texture);
        }

        public void AddTextureFullPath(string filenamePath)
        {
            var texture = LoadTextureFullPath(filenamePath);

            _dictionary.Add(_dictionary.Count, texture);
        }

        public Texture GetTexture(int id)
        {
            return _dictionary[id];
        }
        
        private Texture LoadTexture(string fileName)
        {
            var texture = new Texture();

            var basePath = ConfigManager.ResourcePath;

            var fileNamePath = basePath + @"\Resources\" + fileName;

            texture.LoadTexture(_device, fileNamePath);

            return texture;
        }

        public Texture LoadTextureFullPath(string filePath)
        {
            var texture = new Texture();

            texture.LoadTexture(_device, filePath);

            return texture;
        }


    }
}
