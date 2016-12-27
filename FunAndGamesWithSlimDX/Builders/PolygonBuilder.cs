using DungeonHack.Entities;
using FunAndGamesWithSlimDX.Entities;
using SlimDX;

namespace DungeonHack.Builders
{
    public class PolygonBuilder
    {
        private Polygon _polygon;
        
        public PolygonBuilder New()
        {
            _polygon = new Polygon();
            return this;
        }

        public Polygon Build()
        {
            RecalculateWorldMatrix();

            return _polygon;
        }

        public PolygonBuilder SetScaling(float scale)
        {
            _polygon.ScaleMatrix = Matrix.Scaling(scale, scale, scale);

            return this;
        }

        public PolygonBuilder SetRotation(float yaw, float pitch, float roll)
        {
            _polygon.RotationMatrix = Matrix.RotationYawPitchRoll(yaw, pitch, roll);

            return this;
        }

        public PolygonBuilder SetTranslation(float translationX, float translationY, float translationZ)
        {
            _polygon.TranslationMatrix = Matrix.Translation(translationX, translationY, translationZ);

            return this;
        }

        public PolygonBuilder TransformToWorldCoordinates()
        {
            RecalculateWorldMatrix();

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

            return this;
        }

        public PolygonBuilder SetVertexData(Vertex[] vertexData)
        {
            _polygon.VertexData = vertexData;
            CalculateIndexData();
            return this;
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
