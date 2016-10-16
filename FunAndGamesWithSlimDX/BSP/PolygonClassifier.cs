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
            Model model = plane.Model[0];
            Vector3 vector = new Vector3(model.x, model.y, model.z);
            vector = Vector3.TransformCoordinate(vector, plane.WorldMatrix);
            Vector3 normal = new Vector3(model.nx, model.ny, model.nz);
            normal = Vector3.Normalize(Vector3.TransformCoordinate(normal,plane.WorldMatrix));
            int numberOfVertices = mesh.Model.Length;
            foreach (var modelMesh in mesh.Model)
            {
                Vector3 vector2 = new Vector3(modelMesh.x, modelMesh.y, modelMesh.z);
                vector2 = Vector3.TransformCoordinate(vector2, mesh.WorldMatrix);
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
                return PolygonClassification.Front;
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
