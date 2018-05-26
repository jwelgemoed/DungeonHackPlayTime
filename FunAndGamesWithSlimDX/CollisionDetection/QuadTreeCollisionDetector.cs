using DungeonHack.Entities;
using DungeonHack.QuadTree;
using FunAndGamesWithSharpDX.Engine;
using SharpDX;
using System.Linq;

namespace DungeonHack.CollisionDetection
{
    public class QuadTreeCollisionDetector : ICollisionDetector
    {
        public Camera Camera { get; set; }
        public QuadTreeNode CurrentNode { get; set; }
        
        public Vector3[] HasCollided()
        {
            if (CurrentNode == null)
                return new Vector3[0];

            return CurrentNode.Polygons
               .Where(x => (x.PolygonType == PolygonType.Wall) && x.BoundingBox.CollidesWithCamera(Camera))
               .Select(x => x.Normal)
               .ToArray();
        }
    }
}
