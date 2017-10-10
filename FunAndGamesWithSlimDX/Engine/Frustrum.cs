using SharpDX;

namespace FunAndGamesWithSharpDX.Engine
{
    public class Frustrum
    {
        private SharpDX.Plane[] planes = new SharpDX.Plane[6];

        public void ConstructFrustrum(Matrix vp)
        {
            planes[0] = new Plane(vp.M14 + vp.M11, vp.M24 + vp.M21, vp.M34 + vp.M31, vp.M44 + vp.M41);
            planes[1] = new Plane(vp.M14 - vp.M11, vp.M24 - vp.M21, vp.M34 - vp.M31, vp.M44 - vp.M41);
            planes[2] = new Plane(vp.M14 - vp.M12, vp.M24 - vp.M22, vp.M34 - vp.M32, vp.M44 - vp.M42);
            planes[3] = new Plane(vp.M14 + vp.M12, vp.M24 + vp.M22, vp.M34 + vp.M32, vp.M44 + vp.M42);
            planes[4] = new Plane(vp.M13, vp.M23, vp.M33, vp.M43);
            planes[5] = new Plane(vp.M14 - vp.M13, vp.M24 - vp.M23, vp.M34 - vp.M33, vp.M44 - vp.M43);

            foreach (var plane in planes)
            {
                plane.Normalize();
            }
        }

        public int CheckBoundingBox(BoundingBox boundingBox)
        {
            var totalIn = 0;

            foreach (var plane in planes)
            {
                var intersection = plane.Intersects(ref boundingBox);
                if (intersection == PlaneIntersectionType.Back) return 0;
                if (intersection == PlaneIntersectionType.Front)
                {
                    totalIn++;
                }
            }
            if (totalIn == 6)
            {
                return 2;
            }
            return 1;
        }
    }
}