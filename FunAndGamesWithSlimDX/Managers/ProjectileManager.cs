using DungeonHack.Entities.Projectiles;
using DungeonHack.Quadtree;
using System.Collections.Generic;

namespace DungeonHack.CollisionDetection
{
    public class ProjectileManager
    {
        private QuadTreeTraverser _quadTreeTraverser;

        private IList<Projectile> _activeProjectileList;
        private IList<Projectile> _renderList;

        public ProjectileManager(QuadTreeTraverser quadTreeTraverser)
        {
            _quadTreeTraverser = quadTreeTraverser;
            _activeProjectileList = new List<Projectile>();
            _renderList = new List<Projectile>();
        }

        public void RegisterProjectile(Projectile projectile)
        {
            _activeProjectileList.Add(projectile);
            _renderList.Add(projectile);
        }

        public bool CanAddMoreProjectiles()
        {
            return _activeProjectileList.Count == 0;
        }

        public void UpdateProjectiles()
        {
            for (var i=_activeProjectileList.Count-1; i >= 0; i--) 
            {
                var projectile = _activeProjectileList[i];

                projectile.UpdatePosition();

                var boundingSphere = projectile.GetBoundingSphere();
                var node = _quadTreeTraverser.FindCurrentLeafNodeForBoundingSphere(boundingSphere);

                if (node == null)
                {
                    continue;
                }

                foreach (var polygon in node.Polygons)
                {
                    if (polygon.BoundingBox.ContainsOrIntersectsBoundingSphere(boundingSphere))
                    {
                        projectile.OnCollision(polygon);
                        _activeProjectileList.RemoveAt(i);
                        break;
                    }
                }
            }
        }

        public void RenderProjectiles()
        {
            foreach (var projectile in _renderList)//_activeProjectileList)
            {
                projectile.Render();
            }
        }
    }
}
