﻿using DungeonHack.DataDictionaries;
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
        private readonly DeviceContext _deviceContext;
        private readonly Camera _camera;
        private readonly Shader _shader;
        private readonly object _lock = new object();

        public BoundingBoxRenderer(MaterialDictionary materialDictionary,
                            TextureDictionary textureDictionary,
                            DeviceContext deviceContext,
                            Camera camera,
                            Shader shader)
        {
            _materialDictionary = materialDictionary;
            _textureDictionary = textureDictionary;
            _deviceContext = deviceContext;
            _camera = camera;
            _shader = shader;
        }

        public void RenderBoundingBox(AABoundingBox boundingBox, Matrix worldMatrix, Matrix viewProjectionMatrix, int textureIndex, int materialIndex)
        {
            lock (_lock)
            {
                _deviceContext.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(boundingBox.BoundingBoxVertexBuffer, Vertex.SizeOf, 0));
                _deviceContext.InputAssembler.SetIndexBuffer(boundingBox.BoundingBoxIndexBuffer, Format.R16_UInt, 0);

                _shader.Render(_deviceContext,
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