using DungeonHack.Entities;
using FunAndGamesWithSlimDX.DirectX;
using FunAndGamesWithSlimDX.Engine;
using FunAndGamesWithSlimDX.Entities;
using SlimDX;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FunAndGamesWithSlimDX
{
    public class TerrainDemo : Engine.CameraEngine, IDisposable
    {
        private List<Mesh> _meshes;
        private Sprite _sprite;
        private int _counter;
        private float _timeElapsed;

        public TerrainDemo() : base(1.0f, false)
        {
        }

        public override List<Polygon> GetScenePolygons()
        {
            throw new NotImplementedException();
        }

        public override void DrawScene()
        {
            Camera.Render();

            var viewMatrix = Camera.ViewMatrix;
            var worldMatrix = Renderer.WorldMatrix;
            var projectionMatrix = Renderer.ProjectionMatrix;
            var orthoMatrix = Renderer.OrthoMatrix;

            //Construct the frustrum
            if (ConfigManager.FrustrumCullingEnabled)
                _frustrum.ConstructFrustrum(viewMatrix * projectionMatrix);

            _meshRenderedCount = 0;

            Vector3 cameraPosition = Camera.GetPosition();

            bool doUpdate = false;
            int rotation = 0;

            if ((Timer.TotalTime() - _timeElapsed) >= 1.0f)
            {
                _timeElapsed += 0.0125f;

                _counter++;

                rotation = _counter % 360;

                doUpdate = true;
            }

            Parallel.ForEach(_meshes, (x) =>
            {
                if (doUpdate)
                    x.UpdateMesh(rotation);

                x.Render(_frustrum, base.Renderer.Context, Camera, ref _meshRenderedCount, null);
            });


            //2d rendering
            _sprite.Draw();
        }

        public override void UpdateScene()
        {
            /*Camera.Render();

            if ((Timer.TotalTime() - _timeElapsed) >= 1.0f)
            {
                _timeElapsed += 0.0125f;

                _counter++;

                int rotation = _counter % 360;

                Parallel.ForEach(_meshes, (x) => { x.UpdateMesh(rotation); });
                /*foreach (var mesh in _meshes)
                {
                    if (mesh.GetType() == typeof (TerrainMesh))
                    {
                        var tlight = Vector3.Transform(mesh.LightDirection.Value,
                                                       Matrix.RotationZ((float) (rotation*(Math.PI/360))));
                        Vector3 newLight = new Vector3(tlight.X, tlight.Y, tlight.Z);

                        mesh.LightDirection = newLight;
                    }

                    mesh.UpdateMesh(rotation);
                }
            }*/
        }

        public override void InitializeScene()
        {
            _counter = 0;

            Camera.SetPosition(0, 0.0f, 0.0f);
            Camera.Speed = 5.0f;

            var random = new Random((int)DateTime.Now.Ticks);
            _meshes = new List<Mesh>();

            _sprite = new Sprite(base.Renderer.Device, "crate2_diffuse.png", 0, 0, 64, 64);

            //create random boxes.
            for (int i = 0; i < 1000; i++)
            {
                float randomSize = (float)random.NextDouble() * 3;
                float randomX = 0.5f - (float)random.NextDouble() * 200;
                float randomZ = 0.5f - (float)random.NextDouble() * 200;

                var box2 = new Mesh(base.Renderer.Device, Shader);
                box2.LoadModel("cube.txt");
                box2.LoadTexture("crate2_diffuse.png");
                box2.SetPosition(randomX, 0.5f, randomZ);
                box2.SetScaling(randomSize);

                _meshes.Add(box2);
            }

            var boxMesh = new Mesh(base.Renderer.Device, Shader);

            boxMesh.UpdateMeshAction = (x) =>
            {
                boxMesh.SetRotationMatrix(Matrix.RotationY((float)(-x * Math.PI / 90)));
            };

            boxMesh.LoadModel("F-35_Lightning_II.obj-model.txt");
            boxMesh.LoadTexture("F-35_Lightning_II_P01.dds");
            boxMesh.SetPosition(0.5f, 0.5f, 150.5f);
            boxMesh.SetScaling(1);

            var boxMaya = new Mesh(base.Renderer.Device, Shader);

            boxMaya.LoadModel("cat2.obj-model.txt");
            //boxMaya.LoadTexture("VMaskCol.jpg");
            boxMaya.LoadTexture("BrickRound0109_5_S.jpg");
            boxMaya.SetPosition(1.0f, 1.0f, 5000.0f);
            boxMaya.SetScaling(100);

            boxMaya.UpdateMeshAction = (x) =>
                {
                    boxMaya.SetRotationMatrix(Matrix.RotationY((float)(x * Math.PI / 90)));
                    //boxMaya.SetPosition(2000 * (float)(Math.Cos(x * Math.PI / 90)), 1.0f, 2500.0f + (2000 * (float)Math.Sin(x * Math.PI / 90)));
                };

            _meshes.Add(boxMesh);
            _meshes.Add(boxMaya);

            var boxRoom = new Mesh(base.Renderer.Device, Shader);

            boxRoom.LoadModel("Cube - Room.txt");
            boxRoom.LoadTexture("crate2_diffuse.png");
            boxRoom.SetPosition(0.0f, 0.0f, 0.0f);
            boxRoom.SetScaling(30000);


        }

        public void Dispose()
        {
            foreach (var mesh in _meshes)
                mesh.Dispose();

            _sprite.Dispose();
        }
    }
}