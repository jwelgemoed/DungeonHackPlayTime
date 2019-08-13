using DungeonHack.Lights;
using SharpDX;
using System;

namespace DungeonHack.Entities.Projectiles
{
    public class BallOfLight : Projectile, IDisposable
    {
        private PointLight _pointLight;
        private FunAndGamesWithSharpDX.Entities.Console _console;

        public BallOfLight(Vector3 initialPosition, Vector3 direction, float speed, float radius, FunAndGamesWithSharpDX.Entities.Console console) : base(initialPosition,
            direction, speed, radius)
        {
            _console = console;
        }

        public override void OnCollision(Polygon collidedPolygon)
        {
            _console.WriteLine($"OUCH!!!! You hit a polygon with texture index {collidedPolygon.TextureIndex}");
        }

        public override void Render()
        {
            _pointLight = new PointLight
            {
                Diffuse = new Color4(0.0f, 15.0f, 0.0f, 1.0f),
                Ambient = new Color(),
                Specular = new Color4(0.1f, 5.1f, 0.1f, 0.1f),
                Position = _position,
                Range = 256.0f,
                Attentuation = new Vector3(0.0f, 0.1f, 0.0f)
            };

            LightEngine.AddPointLight(_pointLight);
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~BallOfLight()
        // {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
