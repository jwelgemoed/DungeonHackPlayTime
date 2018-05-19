using DungeonHack.Builders;
using DungeonHack.DataDictionaries;
using SharpDX;
using System;

namespace DungeonHack.Entities
{
    public class ItemFactory
    {
        private PolygonBuilder _builder;
        private TextureDictionary _textureDictionary;
        private MaterialDictionary _materialDictionary;

        public ItemFactory(PolygonBuilder builder, TextureDictionary textureDictionary, MaterialDictionary materialDictionary)
        {
            _builder = builder;
            _textureDictionary = textureDictionary;
            _materialDictionary = materialDictionary;
        }

        public Item CreateItem(string modelPath, string texturePath=null, string materialPath=null, Vector3 initialWorldLocation = new Vector3())
        {
            if (string.IsNullOrWhiteSpace(modelPath))
            {
                throw new ArgumentNullException(nameof(modelPath));
            }

            int textureId = 0;
            int materialId;

            if (!string.IsNullOrWhiteSpace(texturePath))
            {
                textureId = _textureDictionary.AddTextureRelativePath(texturePath);
            }

            return new Item
            {
                Polygon = _builder.New()
                            .CreateFromModel(modelPath)
                            .SetPosition(initialWorldLocation.X, initialWorldLocation.Y, initialWorldLocation.Z)
                            .SetType(PolygonType.Other)
                            .SetTextureIndex(textureId)
                            .SetScaling(10, 10, 10)
                            .WithTransformToWorld()
                            .Build(),

                Location = initialWorldLocation
            };
        }
    }
}
