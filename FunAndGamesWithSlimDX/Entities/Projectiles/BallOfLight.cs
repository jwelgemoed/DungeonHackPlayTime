using DungeonHack.Builders;
using DungeonHack.Lights;
using DungeonHack.Renderers;
using SharpDX;
using System;

namespace DungeonHack.Entities.Projectiles
{
    public class BallOfLight : Projectile, IDisposable
    {
        private PointLight _pointLight;
        private Polygon _polygon;
        private PolygonRenderer _renderer;
        private FunAndGamesWithSharpDX.Entities.Console _console;

        public BallOfLight(PolygonBuilder _builder, PolygonRenderer renderer, Vector3 initialPosition, Vector3 direction, float speed, float radius, FunAndGamesWithSharpDX.Entities.Console console) : base(initialPosition,
            direction, speed, radius)
        {
            _polygon = _builder.New()
                            .CreateFromModel("cat2.obj-model.txt")
                            //.CreateFromModel("only_quad_sphere.obj-model.txt")
                            .SetPosition(initialPosition.X, initialPosition.Y, initialPosition.Z)
                            .SetType(PolygonType.Other)
                            .SetTextureIndex(1)
                            .SetMaterialIndex(0)
                            .SetScaling(0.0001f, 0.0001f, 0.0001f)
                            .WithTransformToWorld()
                            .Build();

            _console = console;
            _renderer = renderer;
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
                Ambient = new Color(0.0f, 0.1f, 0.0f, 0.1f),
                Specular = new Color4(0.1f, 5.1f, 0.1f, 0.1f),
                Position = _position,
                Range = 256.0f,
                Attentuation = new Vector3(0.0f, 0.1f, 0.0f)
            };

            _polygon.TranslationMatrix = Matrix.Translation(_position);
            _polygon.WorldMatrix = _polygon.ScaleMatrix * _polygon.RotationMatrix;
            _polygon.WorldMatrix = _polygon.WorldMatrix * _polygon.TranslationMatrix;

            int counter = 0;
            _renderer.Render(0, _polygon, ref counter);

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
