using FunAndGamesWithSlimDX.DirectX;
using FunAndGamesWithSlimDX.Engine;
using SlimDX.Direct3D11;
using SlimDX.DXGI;
using System.Collections.Generic;
using Device = SlimDX.Direct3D11.Device;

namespace FunAndGamesWithSlimDX.Entities
{
    public class Grid : Mesh
    {

        private float _gridSpacing;
        private int _numberOfLines;

        public Grid(Device device, IShader shader, float gridSpacing, int numberOfLines) 
            : base(device, shader)
        {
            _gridSpacing = gridSpacing;
            _numberOfLines = numberOfLines;

            var model = InitializeModel(_numberOfLines);

            base.LoadVectorsFromModel(model.ToArray());

            base.MeshRenderPrimitive = PrimitiveTopology.LineList;

        }

        public override void Render(Frustrum frustrum, DeviceContext context, Camera camera, ref int meshRenderedCount)
        {
            /*var worldMatrix = Matrix.Scaling(_gridSpacing, _gridSpacing, _gridSpacing) * 
                Matrix.Translation(
                    (int)(cameraPosition.X / _gridSpacing) * _gridSpacing,
                    0f,
                    (int)(cameraPosition.Z / _gridSpacing) * _gridSpacing);
          */
            var worldMatrix = ScaleMatrix * RotationMatrix;
            worldMatrix = worldMatrix * TranslationMatrix;

            context.InputAssembler.PrimitiveTopology = MeshRenderPrimitive;
            context.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(VertexBuffer, base._sizeOfVertex, 0));
            context.InputAssembler.SetIndexBuffer(IndexBuffer, Format.R16_UInt, 0);

            Shader.Render(context, GetIndexCount(), worldMatrix, camera.ViewMatrix, camera.ProjectionMatrix, _texture.TextureData);
        }


        private List<Model> InitializeModel(int numberOfLines)
        {
            var modelList = new List<Model>();

            for (int i = 0; i < numberOfLines; i++)
            {
                Model startLine = new Entities.Model();
                startLine.x = -_gridSpacing*numberOfLines;
                startLine.y = 0;
                startLine.z = i * _gridSpacing - (numberOfLines / 2);

                Model endLine = new Model();
                endLine.x = _gridSpacing*numberOfLines;
                endLine.y = 0;
                endLine.z = i * _gridSpacing + (numberOfLines / 2);

                modelList.Add(startLine);
                modelList.Add(endLine);
            }

            /*for (int i = 0; i < numberOfLines; i ++)
            {
                Model startLine = new Model();
                startLine.x = (i * _gridSpacing) - (numberOfLines / 2);
                startLine.y = 0;
                startLine.z = -_gridSpacing*numberOfLines;

                Model endLine = new Model();
                endLine.x = i * _gridSpacing + (numberOfLines / 2); ;
                endLine.y = 0;
                endLine.z = _gridSpacing*numberOfLines;

                modelList.Add(startLine);
                modelList.Add(endLine);

            }*/

            return modelList;
        }
    }
}
