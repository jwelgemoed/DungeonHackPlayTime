using System.Linq;
using DungeonHack.Entities;
using FunAndGamesWithSlimDX.Entities;
using SlimDX;
using SlimDX.Direct3D11;

namespace DungeonHack.Builders
{
    public class PolygonBuilder
    {
        private Polygon _polygon;
        private const int SizeOfShort = sizeof(uint);
        private readonly Device _device;
        private bool _createBoundingBox = false;
        private bool _transformToWorld = false;
        private bool _calculateIndexData = true;

        public PolygonBuilder(Device device)
        {
            _device = device;
        }

        public PolygonBuilder New()
        {
            _transformToWorld = false;
            _createBoundingBox = false;
            _calculateIndexData = true;
            _polygon = new Polygon();
            _polygon.RotationMatrix = Matrix.Identity;
            _polygon.ScaleMatrix = Matrix.Identity;
            _polygon.TranslationMatrix = Matrix.Identity;
            return this;
        }

        public Polygon Build()
        {
            RecalculateWorldMatrix();
            if (_transformToWorld)
            {
                TransformCoordinates();
            }
            if (_createBoundingBox)
            {
                CreateBoundingBox();
            }
            if (_calculateIndexData)
            {
                CalculateIndexData();
            }
            CreateVertexBuffer();
            CreateIndexBuffer();

            return _polygon;
        }

        public PolygonBuilder SetScaling(float scale)
        {
            _polygon.ScaleMatrix = Matrix.Scaling(scale, scale, scale);
            RecalculateWorldMatrix();
            return this;
        }

        public PolygonBuilder SetScaling(float scaleX, float scaleY, float scaleZ)
        {
            _polygon.ScaleMatrix = Matrix.Scaling(scaleX, scaleY, scaleZ);
            RecalculateWorldMatrix();
            return this;
        }

        public PolygonBuilder SetRotation(float yaw, float pitch, float roll)
        {
            _polygon.RotationMatrix = Matrix.RotationYawPitchRoll(yaw, pitch, roll);
            RecalculateWorldMatrix();
            return this;
        }

        public PolygonBuilder SetTranslation(float translationX, float translationY, float translationZ)
        {
            _polygon.TranslationMatrix = Matrix.Translation(translationX, translationY, translationZ);
            RecalculateWorldMatrix();
            return this;
        }

        public PolygonBuilder SetScalingMatrix(Matrix scaleMatrix)
        {
            _polygon.ScaleMatrix = scaleMatrix;
            RecalculateWorldMatrix();
            return this;
        }

        public PolygonBuilder SetRotationMatrix(Matrix rotationMatrix)
        {
            _polygon.RotationMatrix = rotationMatrix;
            RecalculateWorldMatrix();
            return this;
        }

        public PolygonBuilder SetTranslationMatrix(Matrix translationMatrix)
        {
            _polygon.TranslationMatrix = translationMatrix;
            RecalculateWorldMatrix();
            return this;
        }

        public PolygonBuilder SetTextureIndex(int textureIndex)
        {
            _polygon.TextureIndex = textureIndex;
            return this;
        }

        public PolygonBuilder SetMaterialIndex(int materialIndex)
        {
            _polygon.MaterialIndex = materialIndex;
            return this;
        }

        public PolygonBuilder SetVertexData(Vertex[] vertexData)
        {
            _polygon.VertexData = vertexData;
            return this;
        }

        public PolygonBuilder SetIndexData(short[] indexData)
        {
            _polygon.IndexData = indexData;
            _calculateIndexData = false;
            return this;
        }

        public PolygonBuilder TransformToWorldCoordinates()
        {
            _transformToWorld = true;
            return this;
        }

        public PolygonBuilder WithBoundingBox()
        {
            _createBoundingBox = true;
            return this;
        }

        private void TransformCoordinates()
        {
            for (int i = 0; i < _polygon.VertexData.Length; i++)
            {
                var vertex = Vector3.TransformCoordinate(
                    _polygon.VertexData[i].Position.ToVector3(), _polygon.WorldMatrix);

                _polygon.VertexData[i].Position = new Vector4(vertex.X, vertex.Y, vertex.Z, 1.0f);

                var normal = Vector3.TransformNormal(_polygon.VertexData[i].Normal, _polygon.WorldMatrix);

                _polygon.VertexData[i].Normal = normal;
            }

            var minimum = _polygon.BoundingBox.Minimum;
            var maximum = _polygon.BoundingBox.Maximum;

            minimum = Vector3.TransformCoordinate(minimum, _polygon.WorldMatrix);
            maximum = Vector3.TransformCoordinate(maximum, _polygon.WorldMatrix);
            
            _polygon.BoundingBox = new BoundingBox(minimum, maximum);

        }

        private void CreateBoundingBox()
        {
            _polygon.BoundingBox = BoundingBox.FromPoints(_polygon.VertexData
                .Select(
                    x => new Vector3(x.Position.X, x.Position.Y, x.Position.Z)
                    )
                .ToArray());
        }

        private void CreateVertexBuffer()
        {
            var vertices = new DataStream(Vertex.SizeOf * _polygon.VertexData.Length, true, true);
            vertices.WriteRange(_polygon.VertexData);
            vertices.Position = 0;

            _polygon.VertexBuffer = new SlimDX.Direct3D11.Buffer(_device, vertices, Vertex.SizeOf * _polygon.VertexData.Length
                    , ResourceUsage.Default, BindFlags.VertexBuffer, CpuAccessFlags.None, ResourceOptionFlags.None, Vertex.SizeOf);
        }

        private void CreateIndexBuffer()
        {
            var indexStream = new DataStream(SizeOfShort * _polygon.IndexData.Length, true, true);
            indexStream.WriteRange(_polygon.IndexData);
            indexStream.Position = 0;

            _polygon.IndexBuffer = new SlimDX.Direct3D11.Buffer(_device, indexStream, SizeOfShort * _polygon.IndexData.Length,
                                                           ResourceUsage.Default, BindFlags.IndexBuffer,
                                                           CpuAccessFlags.None, ResourceOptionFlags.None, 0);
        }

        private void RecalculateWorldMatrix()
        {
            _polygon.WorldMatrix = _polygon.ScaleMatrix * _polygon.RotationMatrix;
            _polygon.WorldMatrix = _polygon.WorldMatrix * _polygon.TranslationMatrix;
        }

        private void CalculateIndexData()
        {
            _polygon.IndexData = new short[_polygon.VertexData.Length];

            for (short i = 0; i < _polygon.VertexData.Length; i++)
            { 
                 _polygon.IndexData[i] = i;
            }
        }
    }
}
