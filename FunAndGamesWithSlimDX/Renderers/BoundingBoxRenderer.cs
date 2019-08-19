using DungeonHack.DataDictionaries;
using DungeonHack.DirectX;
using DungeonHack.Entities;
using FunAndGamesWithSharpDX.Engine;
using FunAndGamesWithSharpDX.Entities;
using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.DXGI;

namespace DungeonHack.Renderers
{
    public class BoundingBoxRenderer
    {
        private readonly MaterialDictionary _materialDictionary;
        private readonly TextureDictionary _textureDictionary;
        private readonly DeviceContext _immediateContext;
        private readonly DeviceContext[] _deferredContexts;
        private readonly Camera _camera;
        private readonly LightShader _shader;
        private readonly object _lock = new object();

        public BoundingBoxRenderer(MaterialDictionary materialDictionary,
                            TextureDictionary textureDictionary,
                            DeviceContext immediateContext,
                            DeviceContext[] deferredContexts,
                            Camera camera,
                            LightShader shader)
        {
            _materialDictionary = materialDictionary;
            _textureDictionary = textureDictionary;
            _immediateContext = immediateContext;
            _deferredContexts = deferredContexts;
            _camera = camera;
            _shader = shader;
        }

        public void RenderBoundingBox(int threadNumber, AABoundingBox boundingBox, Matrix worldMatrix, Matrix viewProjectionMatrix, int textureIndex, int materialIndex)
        {
            lock (_lock)
            {
                _deferredContexts[threadNumber].InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(boundingBox.BoundingBoxVertexBuffer, Vertex.SizeOf, 0));
                _deferredContexts[threadNumber].InputAssembler.SetIndexBuffer(boundingBox.BoundingBoxIndexBuffer, Format.R16_UInt, 0);

                _shader.Render(threadNumber,
                                boundingBox.Indexes.Length,
                                worldMatrix,
                                _camera.ViewMatrix,
                                viewProjectionMatrix,
                                _textureDictionary.GetTexture(textureIndex),
                                _camera.GetPosition(),
                                _materialDictionary.GetMaterial(materialIndex));
            }
        }
    }
}
