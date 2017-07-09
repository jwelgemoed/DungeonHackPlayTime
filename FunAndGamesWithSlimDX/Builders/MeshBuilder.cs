using FunAndGamesWithSlimDX.DirectX;
using FunAndGamesWithSlimDX.Engine;
using FunAndGamesWithSlimDX.Entities;
using SlimDX;
using SlimDX.Direct3D11;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DungeonHack.Builders
{
    public class PolygonBuilder
    {
        protected Polygon _mesh;
        protected readonly Device _device;
        protected readonly IShader _shader;
        private bool _withTransformToWorld;

        public PolygonBuilder(Device device, IShader shader)
        {
            _device = device;
            _shader = shader;
        }

        public PolygonBuilder New()
        {
            _mesh = new Polygon(_device, _shader);

            return this;
        }

        public Polygon Build()
        {
            LoadVectorsFromModel(null, null);
            if (_withTransformToWorld)
            {
                TransformToWorld();
            }
            _mesh.VertexBuffer = GetVertexBuffer();
            _mesh.IndexBuffer = GetIndexBuffer();
            return _mesh;
        }

        public PolygonBuilder SetPosition(float x, float y, float z)
        {
            _mesh.TranslationMatrix = Matrix.Translation(x, y, z);
            return this;
        }

        public PolygonBuilder SetScaling(float scale)
        {
            _mesh.ScaleMatrix = Matrix.Scaling(scale, scale, scale);
            return this;
        }

        public PolygonBuilder SetScaling(float scaleX, float scaleY, float scaleZ)
        {
            _mesh.ScaleMatrix = Matrix.Scaling(scaleX, scaleY, scaleZ);
            return this;
        }

        public PolygonBuilder SetRotationMatrix(Matrix rotationMatrix)
        {
            _mesh.RotationMatrix = rotationMatrix;
            return this;
        }

        public PolygonBuilder SetScaleMatrix(Matrix scaleMatrix)
        {
            _mesh.ScaleMatrix = scaleMatrix;
            return this;
        }

        public PolygonBuilder SetTranslationMatrix(Matrix translationMatrix)
        {
            _mesh.TranslationMatrix = translationMatrix;
            return this;
        }

        public PolygonBuilder SetTextureIndex(int textureIndex)
        {
            _mesh.TextureIndex = textureIndex;
            return this;
        }

        public PolygonBuilder SetMaterialIndex(int materialIndex)
        {
            _mesh.MaterialIndex = materialIndex;
            return this;
        }

        public PolygonBuilder SetModel(Model[] model)
        {
            _mesh.Model = model;
            return this;
        }

        public PolygonBuilder SetIndexData(short[] indexData)
        {
            _mesh.IndexData = indexData;
            return this;
        }

        public PolygonBuilder SetVertexData(Vertex[] vertexData)
        {
            _mesh.VertexData = vertexData;
            return this;
        }
        
        public PolygonBuilder CreateFromModel(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                throw new ArgumentException("Param cannot be empty", "fileName");

            var basePath = ConfigManager.ResourcePath;

            var fileNamePath = basePath + @"\Resources\" + fileName;

            using (var inputStream = new StreamReader(fileNamePath))
            {
                string inputLine = "";

                while (!inputLine.Contains("Vertex Count"))
                {
                    inputLine = inputStream.ReadLine();
                }

                int vertexCount = int.Parse(inputLine.Substring(inputLine.IndexOf(":") + 1).Trim());

                _mesh.Model = new Model[vertexCount];

                while (!inputLine.Contains("Data"))
                {
                    inputLine = inputStream.ReadLine();
                }

                for (int i = 0; i < vertexCount; i++)
                {
                    inputLine = inputStream.ReadLine();

                    var data = inputLine.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                    _mesh.Model[i].x = float.Parse(data[0].Trim());
                    _mesh.Model[i].y = float.Parse(data[1].Trim());
                    _mesh.Model[i].z = float.Parse(data[2].Trim());
                    _mesh.Model[i].tx = float.Parse(data[3].Trim());
                    _mesh.Model[i].ty = float.Parse(data[4].Trim());
                    _mesh.Model[i].nx = float.Parse(data[5].Trim());
                    _mesh.Model[i].ny = float.Parse(data[6].Trim());
                    _mesh.Model[i].nz = float.Parse(data[7].Trim());

                }
            }

            LoadVectorsFromModel(null, null);

            return this;
        }

        public PolygonBuilder WithTransformToWorld()
        {
            _withTransformToWorld = true;

            return this;
        }

        private SlimDX.Direct3D11.Buffer GetVertexBuffer()
        {
            var vertices = new DataStream(Vertex.SizeOf * _mesh.VertexData.Length, true, true);
            vertices.WriteRange(_mesh.VertexData);
            vertices.Position = 0;

            return new SlimDX.Direct3D11.Buffer(_device, vertices, Vertex.SizeOf * _mesh.VertexData.Length
                    , ResourceUsage.Default, BindFlags.VertexBuffer, CpuAccessFlags.None, ResourceOptionFlags.None, Vertex.SizeOf);
        }

        private SlimDX.Direct3D11.Buffer GetIndexBuffer()
        {
            var indexStream = new DataStream(sizeof(uint) * _mesh.IndexData.Length, true, true);
            indexStream.WriteRange(_mesh.IndexData);
            indexStream.Position = 0;

            return new SlimDX.Direct3D11.Buffer(_device, indexStream, sizeof(uint) * _mesh.IndexData.Length,
                                                           ResourceUsage.Default, BindFlags.IndexBuffer,
                                                           CpuAccessFlags.None, ResourceOptionFlags.None, 0);
        }

        private void RecalculateWorldMatrix()
        {
            _mesh.WorldMatrix = _mesh.ScaleMatrix * _mesh.RotationMatrix;
            _mesh.WorldMatrix = _mesh.WorldMatrix * _mesh.TranslationMatrix;
        }

        private void TransformToWorld()
        {
            RecalculateWorldMatrix();

            for (int i = 0; i < _mesh.VertexData.Length; i++)
            {
                var vertex = Vector3.TransformCoordinate(
                    new Vector3(_mesh.VertexData[i].Position.X, _mesh.VertexData[i].Position.Y, _mesh.VertexData[i].Position.Z)
                    , _mesh.WorldMatrix);

                _mesh.VertexData[i].Position = new Vector4(vertex.X, vertex.Y, vertex.Z, 1.0f);

                var normal = Vector3.TransformNormal(
                    new Vector3(_mesh.VertexData[i].Normal.X, _mesh.VertexData[i].Normal.Y, _mesh.VertexData[i].Normal.Z)
                    , _mesh.WorldMatrix);

                _mesh.VertexData[i].Normal = normal;
            }

            var minimumVector = Vector3.TransformCoordinate(_mesh.BoundingBox.Minimum, _mesh.WorldMatrix);
            var maximumVector = Vector3.TransformCoordinate(_mesh.BoundingBox.Maximum, _mesh.WorldMatrix);

            _mesh.BoundingBox = new BoundingBox(minimumVector, maximumVector);

        }

        protected void LoadVectorsFromModel(Model[] model, short[] indexes)
        {
            if (_mesh.VertexData != null)
            {
                SetIndexAndBoundingBoxDataFromVertexData();
                return;
            }

            _mesh.VertexData = new Vertex[_mesh.Model.Length];
            _mesh.IndexData = new short[_mesh.Model.Length];
            Vector3[] positions = new Vector3[_mesh.VertexData.Length];

            for (int i = 0; i < _mesh.Model.Length; i++)
            {
                _mesh.VertexData[i] = new Vertex()
                {
                    Position = new Vector4(_mesh.Model[i].x, _mesh.Model[i].y, _mesh.Model[i].z, 1.0f),
                    Texture = new Vector2(_mesh.Model[i].tx, _mesh.Model[i].ty),
                    Normal = new Vector3(_mesh.Model[i].nx, _mesh.Model[i].ny, _mesh.Model[i].nz)
                };

                positions[i] = new Vector3(_mesh.Model[i].x, _mesh.Model[i].y, _mesh.Model[i].z);

                if (indexes == null)
                    _mesh.IndexData[i] = (short)i;
            }

            _mesh.BoundingBox = BoundingBox.FromPoints(positions);
        }

        private void SetIndexAndBoundingBoxDataFromVertexData()
        {
            if (_mesh.VertexData == null)
                return;

            var positions = _mesh.VertexData
                            .Select(x => new Vector3(x.Position.X, x.Position.Y, x.Position.Z));

            _mesh.BoundingBox = BoundingBox.FromPoints(positions.ToArray());

            if (_mesh.IndexData != null)
            {
                return;
            }

            _mesh.IndexData = new short[_mesh.VertexData.Length];

            for (short i=0; i< _mesh.IndexData.Length; i++)
            {
                _mesh.IndexData[i] = i;
            }
        }
    }
}
