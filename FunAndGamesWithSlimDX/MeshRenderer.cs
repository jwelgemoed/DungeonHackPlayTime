using DungeonHack.DataDictionaries;
using FunAndGamesWithSlimDX.DirectX;
using FunAndGamesWithSlimDX.Engine;
using FunAndGamesWithSlimDX.Entities;
using SlimDX.Direct3D11;
using SlimDX.DXGI;

namespace DungeonHack
{
    public class MeshRenderer
    {
        private MaterialDictionary _materialDictionary;
        private TextureDictionary _textureDictionary;
        private DeviceContext _deviceContext;
        private Camera _camera;
        private IShader _shader;

        public MeshRenderer(MaterialDictionary materialDictionary, 
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

        public virtual void Render(Frustrum frustrum, Mesh mesh, ref int meshRenderedCount)
        {
            //Frustrum culling.
            if (ConfigManager.FrustrumCullingEnabled &&
                frustrum.CheckBoundingBox(mesh.BoundingBox) == 0)
            {
                return;
            }

            meshRenderedCount++;

            _deviceContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
            _deviceContext.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(mesh.VertexBuffer, Vertex.SizeOf, 0));
            _deviceContext.InputAssembler.SetIndexBuffer(mesh.IndexBuffer, Format.R16_UInt, 0);

            _shader.Render(_deviceContext, 
                            mesh.IndexData.Length, 
                            mesh.WorldMatrix, 
                            _camera.ViewMatrix,
                            _camera.ProjectionMatrix, 
                            _textureDictionary.GetTexture(mesh.TextureIndex).TextureData, 
                            _camera.GetPosition(), 
                            _materialDictionary.GetMaterial(mesh.MaterialIndex));
        }
    }
}
