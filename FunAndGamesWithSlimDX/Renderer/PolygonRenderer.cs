using DungeonHack.DataDictionaries;
using DungeonHack.Entities;
using FunAndGamesWithSlimDX.DirectX;
using FunAndGamesWithSlimDX.Engine;
using FunAndGamesWithSlimDX.Entities;
using SlimDX.Direct3D11;
using SlimDX.DXGI;
using System.Collections.Generic;

namespace DungeonHack
{
    public class PolygonRenderer
    {
        private DeviceContext _deviceContext;
        private bool _doFrustrumCulling;
        private IShader _shader;
        private Frustrum _frustrum;
        private Camera _camera;

        private readonly int _sizeOfVertex = Vertex.SizeOf;

        private TextureDictionary _textureDictionary;
        private MaterialDictionary _materialDictionary;
        
        public PolygonRenderer SetDeviceContext(DeviceContext deviceContext)
        {
            _deviceContext = deviceContext;
            return this;
        }

        public PolygonRenderer SetShader(IShader shader)
        {
            _shader = shader;
            return this;
        }

        public PolygonRenderer SetFrustrum(Frustrum frustrum)
        {
            _frustrum = frustrum;
            return this;
        }

        public PolygonRenderer SetTextureDictionary(TextureDictionary textureDictionary)
        {
            _textureDictionary = textureDictionary;
            return this;
        }

        public PolygonRenderer SetMaterialDictionary(MaterialDictionary materialDictionary)
        {
            _materialDictionary = materialDictionary;
            return this;
        }

        public PolygonRenderer SetCamera(Camera camera)
        {
            _camera = camera;
            return this;
        }

        public PolygonRenderer SetPrimitiveTopology(PrimitiveTopology topology)
        {
            _deviceContext.InputAssembler.PrimitiveTopology = topology;
            return this;
        }

        public PolygonRenderer WithFrustrumCulling()
        {
            _doFrustrumCulling = true;
            return this;
        }

        public void Render(Polygon polygon)
        {
            //Frustrum culling.
            if (_doFrustrumCulling &&
                _frustrum.CheckBoundingBox(polygon.BoundingBox) == 0)
            {
                return;
            }

            _deviceContext.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(polygon.VertexBuffer, _sizeOfVertex, 0));
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
