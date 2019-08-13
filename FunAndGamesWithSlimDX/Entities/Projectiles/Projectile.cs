using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DungeonHack.Entities.Projectiles
{
    public abstract class Projectile
    {
        protected Vector3 _position;
        protected BoundingSphere _boundingSphere;
        private Vector3 _direction;
        private float _speed;

        public Projectile(Vector3 initialPosition, Vector3 direction, float speed, float radius)
        {
            _position = initialPosition;
            _boundingSphere = new BoundingSphere(_position, radius);
            _speed = speed;
            _direction = direction;

            if (!_direction.IsNormalized)
            {
                _direction.Normalize();
            }
        }

        public abstract void Render();

        public abstract void OnCollision(Polygon collidedPolygon);

        public BoundingSphere GetBoundingSphere()
        {
            return _boundingSphere;
        }

        public Vector3 GetPosition()
        {
            return _position;
        }

        public void UpdatePosition()
        {
            _position += _direction * _speed;
            _boundingSphere.Center = _position;
        }

    }
}
