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
using DungeonHack.Engine;

namespace DungeonHack.Builders
{
    public class PolygonBuilder
    {
        protected Polygon _mesh;
        protected readonly Device _device;
        protected readonly LightShader _shader;
        protected readonly BufferFactory _bufferFactory;

        private bool _withTransformToWorld;

        public PolygonBuilder(Device device, LightShader shader, BufferFactory bufferFactory)
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
            GenerateTangentVectors();
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

        //Lengyel, Eric. “Computing Tangent Space Basis Vectors for an Arbitrary Mesh”. Terathon Software, 2001. http://terathon.com/code/tangent.html
        private void GenerateTangentVectors()
        {
            int numberOfTriangles = (_mesh.VertexData.Length / 3);
            int vertexCount = _mesh.VertexData.Length;
            Vector3[] tan1 = new Vector3[vertexCount];
            Vector3[] tan2 = new Vector3[vertexCount];

            for (int i = 0; i < numberOfTriangles; i++)
            {
                int i1 = _mesh.IndexData[i * 3];
                int i2 = _mesh.IndexData[(i * 3) + 1];
                int i3 = _mesh.IndexData[(i * 3) + 2];

                Vector4 v1 = _mesh.VertexData[i1].Position;
                Vector4 v2 = _mesh.VertexData[i2].Position;
                Vector4 v3 = _mesh.VertexData[i3].Position;

                Vector2 w1 = _mesh.VertexData[i1].Texture;
                Vector2 w2 = _mesh.VertexData[i2].Texture;
                Vector2 w3 = _mesh.VertexData[i3].Texture;

                float x1 = v2.X - v1.X;
                float x2 = v3.X - v1.X;
                float y1 = v2.Y - v1.Y;
                float y2 = v3.Y - v1.Y;
                float z1 = v2.Z - v1.Z;
                float z2 = v3.Z - v1.Z;

                float s1 = w2.X - w1.X;
                float s2 = w3.X - w1.X;
                float t1 = w2.Y - w1.Y;
                float t2 = w3.Y - w1.Y;

                float r = 1.0F / (s1 * t2 - s2 * t1);
                Vector3 sdir = new Vector3((t2* x1 -t1 * x2) *r, (t2 * y1 - t1 * y2) * r, (t2 * z1 - t1 * z2) * r);
                Vector3 tdir = new Vector3((s1* x2 -s2 * x1) *r, (s1 * y2 - s2 * y1) * r, (s1 * z2 - s2 * z1) * r);

                tan1[i1] += sdir;
                tan1[i2] += sdir;
                tan1[i3] += sdir;

                tan2[i1] += tdir;
                tan2[i2] += tdir;
                tan2[i3] += tdir;
            }

            for (long a = 0; a < vertexCount; a++)
            {
                Vector3 n = _mesh.VertexData[a].Normal;
                Vector3 t = tan1[a];

                // Gram-Schmidt orthogonalize
                Vector3 tangent = (t - n * Vector3.Dot(n, t));
                tangent.Normalize();
                //Handedness
                float w = Vector3.Dot(Vector3.Cross(n, t), tan2[a]) < 0.0f ? -1.0f : 1.0f;

                _mesh.VertexData[a].TangentU = new Vector4(tangent, w);
            }
        }
    }
}
