using FunAndGamesWithSharpDX.Entities;
using System.Collections.Generic;

namespace DungeonHack.DataDictionaries
{
    public class MaterialDictionary
    {
        private readonly Dictionary<int, Material> _materialDictionary;

        public MaterialDictionary()
        {
            _materialDictionary = new Dictionary<int, Material>();
        }

        public void AddMaterial(Material mat)
        {
            _materialDictionary.Add(_materialDictionary.Count, mat);
        }

        public Material GetMaterial(int id)
        {
            return _materialDictionary[id];
        }
    }
}
