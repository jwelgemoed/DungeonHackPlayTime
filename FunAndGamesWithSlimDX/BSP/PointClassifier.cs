using SlimDX;

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

        public PointClassification ClassifyPoint(Vector3 position, Entities.Plane plane)
        {
            return ClassifyPoint(position, plane.PointOnPlane, plane.Normal);
        }

        public PointClassification ClassifyPoint(Vector3 position, Vector3 pointOnPlane, Vector3 planeNormal)
        {
            float result;
            Vector3 direction = pointOnPlane - position;
            result = Vector3.Dot(direction, planeNormal);

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
