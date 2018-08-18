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
                new List<string>(Directory.GetFileSystemEntries(path, "*.png"))
                        .Select(x => x.ToLower())
                        .Where(x => !x.Contains("_nrm.png"))
                        .Where(x => !x.Contains("_disp.png"))
                        .Where(x => !x.Contains("_color.png"))
                        .Where(x => !x.Contains("_occ.png"))
                        .Where(x => !x.Contains("_spec.png"))
                    .OrderBy(x => x))
            {
                AddTextureAndNormalMapFullPath(file);
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

        public int AddTextureAndNormalMapFullPath(string filenamePath)
        {
            var texture = LoadTextureAndNormalMap(filenamePath);

            _dictionary.Add(_dictionary.Count, new Tuple<Texture, string>(texture, filenamePath));

            return _dictionary.Count - 1;
        }

        public Texture GetTexture(int id)
        {
            return _dictionary[id].Item1;
        }
        
        private Texture LoadTexture(string fileName)
        {
            var texture = new Texture(_device);

            var basePath = ConfigManager.ResourcePath;

            var fileNamePath = basePath + @"\Resources\" + fileName;

            texture.LoadTexture(fileNamePath);

            return texture;
        }

        public Texture LoadTextureFullPath(string filePath)
        {
            var texture = new Texture(_device);

            texture.LoadTexture(filePath);

            return texture;
        }

        public Texture LoadTextureAndNormalMap(string filePath)
        {
            var texture = new Texture(_device);

            texture.LoadTexture(filePath);

            if (File.Exists(filePath.Replace(".png", "_nrm.png")))
                texture.LoadNormalMap(filePath.Replace(".png", "_nrm.png"));

            if (File.Exists(filePath.Replace(".png", "_disp.png")))
                texture.LoadDisplacementMap(filePath.Replace(".png", "_disp.png"));

            if (File.Exists(filePath.Replace(".png", "_spec.png")))
                texture.LoadSpecularMap(filePath.Replace(".png", "_spec.png"));

            return texture;
        }

        public void AddAllTextureFromPath(object p)
        {
            throw new NotImplementedException();
        }
    }
}
