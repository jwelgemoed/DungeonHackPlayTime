using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SlimDX;
using SlimDX.DXGI;
using SlimDX.Direct3D11;
using SlimDX.Direct3D9;
using Device = SlimDX.Direct3D11.Device;
using Format = SlimDX.DXGI.Format;
using MapFlags = SlimDX.Direct3D11.MapFlags;

namespace FunAndGamesWithSlimDX
{
    public class D3DUtil
    {
        public ShaderResourceView CreateTexture2DArraySRV(Device device, DeviceContext deviceContext,
                                                          string[] fileNames, Format format, FilterFlags filter,
                                                          FilterFlags mipFilter)
        {
            var size = fileNames.Length;
            
            var textures = (from fileName in fileNames
                                         let loadInfo = new ImageLoadInformation
                                             {
                                                 Width = D3DX.FromFile,
                                                 Height = D3DX.FromFile,
                                                 Depth = D3DX.FromFile,
                                                 FirstMipLevel = 0,
                                                 MipLevels = D3DX.FromFile,
                                                 Usage = ResourceUsage.Staging,
                                                 BindFlags = 0,
                                                 CpuAccessFlags = CpuAccessFlags.Write | CpuAccessFlags.Read,
                                                 Format = format,
                                                 FilterFlags = filter,
                                                 MipFilterFlags = mipFilter
                                             }

                                         select Texture2D.FromFile(device, fileName, loadInfo)).ToArray();

            Texture2DDescription desc = new Texture2DDescription();
            desc = textures[0].Description;

            Texture2DDescription texArrayDesc = new Texture2DDescription();
            texArrayDesc.Width = desc.Width;
            texArrayDesc.Height = desc.Height;
            texArrayDesc.MipLevels = desc.MipLevels;
            texArrayDesc.ArraySize = desc.ArraySize;
            texArrayDesc.Format = desc.Format;
            texArrayDesc.SampleDescription = new SampleDescription(1, 0);
            texArrayDesc.Usage = ResourceUsage.Default;
            texArrayDesc.BindFlags = BindFlags.ShaderResource;
            texArrayDesc.CpuAccessFlags = 0;

            Texture2D texArray = new Texture2D(device, texArrayDesc);

            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < desc.MipLevels; j++)
                {
                    var mappedTex2d = deviceContext.MapSubresource(textures[i], j, MapMode.Read, MapFlags.None);

                    //deviceContext.UpdateSubresource(new DataBox, );
                }
            }

            return null;
        }

    }
}
