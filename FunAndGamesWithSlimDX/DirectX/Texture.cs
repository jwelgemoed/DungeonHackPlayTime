using System;
using SharpDX.Direct3D11;
using CommonDX;

namespace FunAndGamesWithSharpDX.DirectX
{
    public class Texture : IDisposable
    {
        public ShaderResourceView TextureData { get; set; }

        public void LoadTexture(Device device, string fileName)
        {
            var bitmapResource = TextureLoader.LoadBitmap(new SharpDX.WIC.ImagingFactory2(), fileName);
            var texture = TextureLoader.CreateTexture2DFromBitmap(device, bitmapResource);
            TextureData = new ShaderResourceView(device, texture);
        }

        public void Dispose()
        {
            if (TextureData != null)
                TextureData.Dispose();
        }
    }
}
