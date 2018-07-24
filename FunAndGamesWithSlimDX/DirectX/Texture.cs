using System;
using SharpDX.Direct3D11;
using CommonDX;

namespace FunAndGamesWithSharpDX.DirectX
{
    public class Texture : IDisposable
    {
        public ShaderResourceView TextureData { get; set; }
        public ShaderResourceView NormalMapData { get; set; }
        public ShaderResourceView DisplacementMapData { get; set; }

        public void LoadTexture(Device device, string fileName)
        {
            TextureData = LoadShaderResourceView(device, fileName);
        }

        public void LoadNormalMap(Device device, string fileName)
        {
            NormalMapData = LoadShaderResourceView(device, fileName);
        }

        public void LoadDisplacementMap(Device device, string fileName)
        {
            DisplacementMapData = LoadShaderResourceView(device, fileName);
        }

        private ShaderResourceView LoadShaderResourceView(Device device, string fileName)
        {
            var bitmapResource = TextureLoader.LoadBitmap(new SharpDX.WIC.ImagingFactory2(), fileName);
            var texture = TextureLoader.CreateTexture2DFromBitmap(device, bitmapResource);
            return new ShaderResourceView(device, texture);
        }

        public void Dispose()
        {
            if (TextureData != null)
                TextureData.Dispose();

            if (NormalMapData != null)
                NormalMapData.Dispose();

            if (DisplacementMapData != null)
                DisplacementMapData.Dispose();
        }
    }
}
