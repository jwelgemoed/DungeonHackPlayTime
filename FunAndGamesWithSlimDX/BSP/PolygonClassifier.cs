using FunAndGamesWithSlimDX.Entities;
using SlimDX;
using Plane = DungeonHack.Entities.Plane;

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

        public PolygonClassification ClassifyPolygon(Polygon plane, Polygon mesh)
        {
            return ClassifyPolygon(
                new Plane {
                    PointOnPlane = new Vector3(
                                        plane.VertexData[0].Position.X, 
                                        plane.VertexData[0].Position.Y, 
                                        plane.VertexData[0].Position.Z),
                    Normal = plane.Normal
                },
                mesh);
        }
        
            
       public PolygonClassification ClassifyPolygon(Plane plane, Polygon mesh)
        {
            int inFront = 0;
            int behind = 0;
            int onPlane = 0;
            float result;

            Vector3 vector = plane.PointOnPlane;

            Vector3 normal = plane.Normal;

            int numberOfVertices = mesh.VertexData.Length;
            foreach (var vertexMesh in mesh.VertexData)
            {
                Vector3 vector2 = new Vector3(vertexMesh.Position.X, vertexMesh.Position.Y, vertexMesh.Position.Z);
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
