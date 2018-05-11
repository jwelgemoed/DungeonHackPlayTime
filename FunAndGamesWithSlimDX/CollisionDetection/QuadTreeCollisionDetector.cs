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
        
        public Vector3? HasCollided()
        {
            if (CurrentNode == null)
                return null;

            return CurrentNode.Polygons
               .FirstOrDefault(x => x.PolygonType == PolygonType.Wall && x.BoundingBox.CollidesWithCamera(Camera))
                ?.Normal;
        }
    }
}
