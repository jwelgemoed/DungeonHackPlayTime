using FunAndGamesWithSharpDX.DirectX;
using FunAndGamesWithSharpDX.Engine;
using SharpDX.Direct3D11;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.IO;
using DungeonHack.Engine;

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
                new List<string>(Directory.GetFileSystemEntries(path, "*.png")).OrderBy(x => x))
            {
                AddTextureFullPath(file);
            }
        }
        
        public int AddTextureRelativePath(string filename)
        {
            var texture = LoadTexture(filename);

            _dictionary.Add(_dictionary.Count, new Tuple<Texture, string>(texture, filename));

            return _dictionary.Count - 1;
        }

        public int AddTextureFullPath(string filenamePath)
        {
            var texture = LoadTextureFullPath(filenamePath);

            _dictionary.Add(_dictionary.Count, new Tuple<Texture, string>(texture, filenamePath));

            return _dictionary.Count - 1;
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

        public void AddAllTextureFromPath(object p)
        {
            throw new NotImplementedException();
        }
    }
}
