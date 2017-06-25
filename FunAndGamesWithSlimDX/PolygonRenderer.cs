using DungeonHack.DataDictionaries;
using FunAndGamesWithSlimDX.DirectX;
using FunAndGamesWithSlimDX.Engine;
using FunAndGamesWithSlimDX.Entities;
using SlimDX.Direct3D11;
using SlimDX.DXGI;

namespace DungeonHack
{
    public class PolygonRenderer
    {
        private readonly MaterialDictionary _materialDictionary;
        private readonly TextureDictionary _textureDictionary;
        private readonly DeviceContext _deviceContext;
        private readonly Camera _camera;
        private readonly IShader _shader;

        public PolygonRenderer(MaterialDictionary materialDictionary, 
                            TextureDictionary textureDictionary,
                            DeviceContext deviceContext,
                            Camera camera,
                            IShader shader)
        {
            _materialDictionary = materialDictionary;
            _textureDictionary = textureDictionary;
            _deviceContext = deviceContext;
            _camera = camera;
            _shader = shader;
        }

        public void Render(Frustrum frustrum, Polygon polygon, ref int polygonRenderedCount)
        {
            //Frustrum culling.
            if (ConfigManager.FrustrumCullingEnabled &&
                frustrum.CheckBoundingBox(polygon.BoundingBox) == 0)
            {
                return;
            }

            polygonRenderedCount++;

            _deviceContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
            _deviceContext.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(polygon.VertexBuffer, Vertex.SizeOf, 0));
            _deviceContext.InputAssembler.SetIndexBuffer(polygon.IndexBuffer, Format.R16_UInt, 0);

            _shader.Render(_deviceContext, 
                            polygon.IndexData.Length, 
                            polygon.WorldMatrix, 
                            _camera.ViewMatrix,
                            _camera.ProjectionMatrix, 
                            _textureDictionary.GetTexture(polygon.TextureIndex).TextureData, 
                            _camera.GetPosition(), 
                            _materialDictionary.GetMaterial(polygon.MaterialIndex));
        }
    }
}
