using FunAndGamesWithSlimDX.DirectX;
using FunAndGamesWithSlimDX.Engine;
using SlimDX.Direct3D11;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;

namespace DungeonHack.DataDictionaries
{
    public class TextureDictionary
    {
        private readonly Device _device;
        private readonly Dictionary<int, Tuple<Texture, string>> _dictionary;

        public TextureDictionary(Device device)
        {
            _device = device;
            _dictionary = new Dictionary<int, Tuple<Texture, string>>();
        }

        public void AddAllTextureFromPath(string path)
        {
            foreach (var file in 
                Directory.GetFileSystemEntries(path, "*.png"))
            {
                AddTextureFullPath(file);
            }
        }
        
        public void AddTextureRelativePath(string filename)
        {
            var texture = LoadTexture(filename);

            _dictionary.Add(_dictionary.Count, new Tuple<Texture, string>(texture, filename));
        }

        public void AddTextureFullPath(string filenamePath)
        {
            var texture = LoadTextureFullPath(filenamePath);

            _dictionary.Add(_dictionary.Count, new Tuple<Texture, string>(texture, filenamePath));
        }

        public Texture GetTexture(int id)
        {
            return _dictionary[id].Item1;
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
