using FunAndGamesWithSlimDX.Entities;
using SlimDX;

namespace DungeonHack.BSP
{
    public enum PolygonClassification
    {
        Front,
        Back,
        OnPlane,
        Spanning
    }

    public class PolygonClassifier
    {
    
       public PolygonClassification ClassifyPolygon(Mesh plane, Mesh mesh)
        {
            int inFront = 0;
            int behind = 0;
            int onPlane = 0;
            float result;
            Vertex vertex = plane.VertexData[0];

            Vector3 vector = vertex.Position.ToVector3();

            Vector3 normal = vertex.Normal;

            int numberOfVertices = mesh.VertexData.Length;
            foreach (var vertexMesh in mesh.VertexData)
            {
                Vector3 vector2 = vertexMesh.Position.ToVector3();
                Vector3 direction = vector - vector2;

                result = Vector3.Dot(direction, normal);

                if (result < -0.001)
                {
                    inFront++;
                }
                else if (result > 0.001)
                {
                    behind++;
                }
                else
                {
                    inFront++;
                    onPlane++;
                    behind++;
                }
            }

            if (onPlane == numberOfVertices)
            {
                return PolygonClassification.OnPlane;
            }
            if (behind == numberOfVertices)
            {
                return PolygonClassification.Back;
            }
            if (inFront == numberOfVertices)
            {
                return PolygonClassification.Front;
            }

            return PolygonClassification.Spanning;
        }
    }
}
