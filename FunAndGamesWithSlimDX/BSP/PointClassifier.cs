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

        public PointClassification ClassifyPoint(Vector3 point, Mesh plane)
        {
            float result;
            Vector3 vector = plane.VertexData[0].Position.ToVector3();
            Vector3 direction = point - vector;
            Vector3 normal = plane.VertexData[0].Normal;
            result = Vector3.Dot(direction, normal);

            if (result < -0.001)
            {
                return PointClassification.Back;
            }
            if (result > 0.001)
            {
                return PointClassification.Front;
            }
            return PointClassification.OnPlane;
        }
    }
}
