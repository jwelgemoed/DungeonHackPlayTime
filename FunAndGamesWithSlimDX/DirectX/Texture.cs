using System;
using SharpDX.Direct3D11;
using CommonDX;

namespace FunAndGamesWithSharpDX.DirectX
{
    public class Texture : IDisposable
    {
        public ShaderResourceView TextureData { get; private set; }
        public ShaderResourceView NormalMapData { get; private set; }
        public ShaderResourceView DisplacementMapData { get; private set; }
        public ShaderResourceView SpecularMapData { get; private set; }

        private readonly Device _device;

        public Texture(Device device)
        {
            _device = device;
        }
        
        public void LoadTexture(string fileName)
        {
            TextureData = LoadShaderResourceView(fileName);
        }

        public void LoadNormalMap(string fileName)
        {
            NormalMapData = LoadShaderResourceView(fileName);
        }

        public void LoadDisplacementMap(string fileName)
        {
            DisplacementMapData = LoadShaderResourceView(fileName);
        }

        public void LoadSpecularMap(string fileName)
        {
            SpecularMapData = LoadShaderResourceView(fileName);
        }

        private ShaderResourceView LoadShaderResourceView(string fileName)
        {
            using (var factory = new SharpDX.WIC.ImagingFactory2())
            {
                var bitmapResource = TextureLoader.LoadBitmap(new SharpDX.WIC.ImagingFactory2(), fileName);
                var texture = TextureLoader.CreateTexture2DFromBitmap(_device, bitmapResource);
                return new ShaderResourceView(_device, texture);
            }
        }

        public void Dispose()
        {
            if (TextureData != null)
                TextureData.Dispose();

            if (NormalMapData != null)
                NormalMapData.Dispose();

            if (DisplacementMapData != null)
                DisplacementMapData.Dispose();

            if (SpecularMapData != null)
                SpecularMapData.Dispose();
        }
    }
}
