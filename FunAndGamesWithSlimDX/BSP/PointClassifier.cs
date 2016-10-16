using FunAndGamesWithSlimDX.Entities;
using SlimDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DungeonHack.BSP
{
    public enum PointClassification
    {
        Front,
        Back,
        OnPlane
    }

    public class PointClassifier
    {

        public PointClassification ClassifyPoint(Vector3 position, Mesh plane)
        {
            float result;
            Model model = plane.Model[0];
            Vector3 vector = new Vector3(model.x, model.y, model.z);
            vector = Vector3.TransformCoordinate(vector, plane.WorldMatrix);
            Vector3 direction = vector - position;
            Vector3 normal = new Vector3(model.nx, model.ny, model.nz);
            normal = Vector3.Normalize(Vector3.TransformCoordinate(normal, plane.WorldMatrix));
            result = Vector3.Dot(direction, normal);

            if (result < -0.001)
            {
                return PointClassification.Front;
            }
            if (result > 0.001)
            {
                return PointClassification.Back;
            }
            return PointClassification.OnPlane;
        }
    }
}
