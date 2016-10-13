using System;
using SlimDX.Direct3D11;

namespace FunAndGamesWithSlimDX.DirectX
{
    public class Texture : IDisposable
    {
        public ShaderResourceView TextureData { get; set; }

        public void LoadTexture(Device device, string fileName)
        {
            TextureData = ShaderResourceView.FromFile(device, fileName);
        }

        public void Dispose()
        {
            if (TextureData != null)
                TextureData.Dispose();
        }
    }
}
