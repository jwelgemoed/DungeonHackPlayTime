using Assimp;
using DungeonHack.DirectX;
using DungeonHack.Entities;
using FunAndGamesWithSharpDX.DirectX;
using FunAndGamesWithSharpDX.Engine;
using FunAndGamesWithSharpDX.Entities;
using SharpDX;
using SharpDX.Direct3D11;
using System;
using System.IO;
using System.Linq;

namespace DungeonHack.Builders
{
    public class PolygonBuilder
    {
        protected Polygon _mesh;
        protected readonly Device _device;
        protected readonly Shader _shader;
        protected readonly BufferFactory _bufferFactory;

        private bool _withTransformToWorld;

        public PolygonBuilder(Device device, Shader shader, BufferFactory bufferFactory)
        {
            _device = device;
            _shader = shader;
            _bufferFactory = bufferFactory;
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
            _mesh.VertexBuffer = _bufferFactory.GetVertexBuffer(_mesh.VertexData);
            _mesh.IndexBuffer = _bufferFactory.GetIndexBuffer(_mesh.IndexData);
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

        public PolygonBuilder SetType(PolygonType type)
        {
            _mesh.PolygonType = type;
            return this;
        }

        public PolygonBuilder LoadFromMesh(Mesh mesh)
        {
            if (!mesh.HasVertices)
            {
                throw new ArgumentException("No vertices in mesh!");
            }

            _mesh.IndexData = mesh.GetShortIndices();
            int index = 0;

            foreach (var vector in mesh.Vertices)
            {
                Vertex vertex = new Vertex();
                vertex.Position = new Vector4(vector.X, vector.Y, vector.Z, 1.0f);
                vertex.Normal = new Vector3(mesh.Normals[index].X, mesh.Normals[index].Y, mesh.Normals[index].Z);

                //Can only handle 1 channel at the moment
                if (mesh.HasTextureCoords(0))
                {
                    vertex.Texture = new Vector2(mesh.TextureCoordinateChannels[0][index].X,
                                                 mesh.TextureCoordinateChannels[0][index].Y);

                }

                index++;
            }
            
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

        private void RecalculateWorldMatrix()
        {
            _mesh.WorldMatrix = _mesh.ScaleMatrix * _mesh.RotationMatrix;
            _mesh.WorldMatrix = _mesh.WorldMatrix * _mesh.TranslationMatrix;
        }

        private void TransformToWorld()
        {
            RecalculateWorldMatrix();

            _mesh.WorldVectors = new Vector3[_mesh.VertexData.Length];

            for (int i = 0; i < _mesh.VertexData.Length; i++)
            {
                var vector = new Vector3(_mesh.VertexData[i].Position.X, _mesh.VertexData[i].Position.Y, _mesh.VertexData[i].Position.Z);

                var vertex = Vector3.TransformCoordinate(vector, _mesh.WorldMatrix);

                _mesh.WorldVectors[i] = vertex;

                _mesh.VertexData[i].Position = new Vector4(vertex.X, vertex.Y, vertex.Z, 1.0f);

                var normal = Vector3.TransformNormal(
                    new Vector3(_mesh.VertexData[i].Normal.X, _mesh.VertexData[i].Normal.Y, _mesh.VertexData[i].Normal.Z)
                    , _mesh.WorldMatrix);

                _mesh.VertexData[i].Normal = normal;
            }

            var minimumVector = Vector3.TransformCoordinate(_mesh.BoundingBox.BoundingBox.Minimum, _mesh.WorldMatrix);
            var maximumVector = Vector3.TransformCoordinate(_mesh.BoundingBox.BoundingBox.Maximum, _mesh.WorldMatrix);

            _mesh.BoundingBox = new AABoundingBox(new BoundingBox(minimumVector, maximumVector), new BufferFactory(_device));

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

            _mesh.BoundingBox = new AABoundingBox(BoundingBox.FromPoints(positions), new BufferFactory(_device));
        }

        private void SetIndexAndBoundingBoxDataFromVertexData()
        {
            if (_mesh.VertexData == null)
                return;

            var positions = _mesh.VertexData
                            .Select(x => new Vector3(x.Position.X, x.Position.Y, x.Position.Z));

            _mesh.BoundingBox = new AABoundingBox(BoundingBox.FromPoints(positions.ToArray()), new BufferFactory(_device));

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
