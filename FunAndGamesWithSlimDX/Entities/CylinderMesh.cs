using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FunAndGamesWithSlimDX.DirectX;
using SlimDX;
using SlimDX.Direct3D11;

namespace FunAndGamesWithSlimDX.Entities
{
  public class CylinderMesh : Mesh
  {
    public CylinderMesh(Device device, Color4 color, IShader shader, float height, float width, float depth, int m, int n)
            : base(device, shader)
    {

    }
  }
}
