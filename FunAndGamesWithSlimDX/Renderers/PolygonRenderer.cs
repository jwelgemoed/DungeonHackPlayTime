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
        private readonly DeviceContext[] _deviceContexts;
        private readonly DeviceContext _immediateContext;
        private readonly CommandList[] _commandLists;
        private readonly Camera _camera;
        private readonly Shader _shader;
        private readonly object _lock = new object() ;

        public PolygonRenderer(MaterialDictionary materialDictionary,
                            TextureDictionary textureDictionary,
                            DeviceContext immediateContext,
                            DeviceContext[] deviceContexts,
                            Camera camera,
                            Shader shader)
        {
            _materialDictionary = materialDictionary;
            _textureDictionary = textureDictionary;
            _deviceContexts = deviceContexts;
            _immediateContext = immediateContext;
            _commandLists = new CommandList[_deviceContexts.Length];
            _camera = camera;
            _shader = shader;
        }

        public void RenderFrame(Camera camera)
        {
            _shader.RenderFrame(camera);
        }

        public void Render(int threadCount, Polygon polygon, ref int polygonRenderedCount)
        {
            lock (_lock)
            {
                polygonRenderedCount++;

                _deviceContexts[threadCount].InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(polygon.VertexBuffer, Vertex.SizeOf, 0));
                _deviceContexts[threadCount].InputAssembler.SetIndexBuffer(polygon.IndexBuffer, Format.R16_UInt, 0);

                _shader.Render(_deviceContexts[threadCount],
                                polygon.IndexData.Length,
                                polygon.WorldMatrix,
                                _camera.ViewMatrix,
                                _camera.RenderViewProjectionMatrix,
                                _textureDictionary.GetTexture(polygon.TextureIndex),
                                _camera.GetPosition(),
                                _materialDictionary.GetMaterial(polygon.MaterialIndex));
            }
        }

        public void CompleteRendering(int threadCount)
        {
            _commandLists[threadCount] = _deviceContexts[threadCount].FinishCommandList(false);
        }

        public void RenderFinal(int threadCount)
        {
            for (int i = 0; i < threadCount; i++)
            {
                var commandList = _commandLists[i];

                _immediateContext.ExecuteCommandList(commandList, false);
                commandList.Dispose();
            }
        }

        public void RenderBoundingBox(int threadCount, AABoundingBox boundingBox, Matrix worldMatrix, Matrix viewProjectionMatrix, int textureIndex, int materialIndex)
        {
            lock (_lock)
            {
                _deviceContexts[threadCount].InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(boundingBox.BoundingBoxVertexBuffer, Vertex.SizeOf, 0));
                _deviceContexts[threadCount].InputAssembler.SetIndexBuffer(boundingBox.BoundingBoxIndexBuffer, Format.R16_UInt, 0);

                _shader.Render(_deviceContexts[threadCount],
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
