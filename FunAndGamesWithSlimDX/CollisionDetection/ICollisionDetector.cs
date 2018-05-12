using SharpDX;

namespace DungeonHack.CollisionDetection
{
    public interface ICollisionDetector
    {
        Vector3[] HasCollided();
    }
}