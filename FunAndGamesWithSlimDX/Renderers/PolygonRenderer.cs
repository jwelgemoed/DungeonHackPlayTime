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
    public class PolygonRenderer
    {
        private readonly MaterialDictionary _materialDictionary;
        private readonly TextureDictionary _textureDictionary;
        private readonly DeviceContext _immediateContext;
        private readonly DeviceContext[] _deferredContexts;
        private CommandList[] _commandLists;
        private readonly Camera _camera;
        private readonly LightShader _shader;
        private readonly object _lock = new object() ;

        public PolygonRenderer(MaterialDictionary materialDictionary,
                            TextureDictionary textureDictionary,
                            DeviceContext deviceContext,
                            DeviceContext[] deferredContexts,
                            CommandList[] commandLists,
                            Camera camera,
                            LightShader shader)
        {
            _materialDictionary = materialDictionary;
            _textureDictionary = textureDictionary;
            _immediateContext = deviceContext;
            _deferredContexts = deferredContexts;
            _commandLists = commandLists;
            _camera = camera;
            _shader = shader;
        }

        public void RenderFrame(Camera camera)
        {
            _shader.RenderFrame(camera);
        }

        public void FinalizeRender(int threadNumber)
        {
            _commandLists[threadNumber] = _deferredContexts[threadNumber].FinishCommandList(true);
        }

        public void RenderAll()
        {
            for (int i = 0; i < _deferredContexts.Length; i++)
            {
                var commandList = _commandLists[i];
                // Execute the deferred command list on the immediate context
                _immediateContext.ExecuteCommandList(commandList, false);

                // Release the command list
                commandList.Dispose();
                _commandLists[i] = null;
            }
        }

        public void Render(int threadNumber, Polygon polygon, ref int polygonRenderedCount)
        {
            polygonRenderedCount++;

            lock (_lock)
            {
                _deferredContexts[threadNumber].InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(polygon.VertexBuffer, Vertex.SizeOf, 0));
                _deferredContexts[threadNumber].InputAssembler.SetIndexBuffer(polygon.IndexBuffer, Format.R16_UInt, 0);
            }

            _shader.Render(threadNumber,
                            polygon.IndexData.Length,
                            polygon.WorldMatrix,
                            _camera.ViewMatrix,
                            _camera.RenderViewProjectionMatrix,
                            _textureDictionary.GetTexture(polygon.TextureIndex),
                            _camera.GetPosition(),
                            _materialDictionary.GetMaterial(polygon.MaterialIndex));
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

        public void ItemRenderer(Item item)
        {

        }
    }
}
