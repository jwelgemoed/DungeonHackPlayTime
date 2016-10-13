using FunAndGamesWithSlimDX.DirectX;
using FunAndGamesWithSlimDX.Engine;
using FunAndGamesWithSlimDX.Entities;
using FunAndGamesWithSlimDX.FX;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace FunAndGamesWithSlimDX
{
    public class SimpleTriangleDemo : Engine.Engine, IDisposable
    {
        private readonly TextureShader _shader;
        private List<Mesh> _meshes;
        private float _previousMouseX;
        private int _counter;
        private float _previousMouseY;
        private bool _start;

        public SimpleTriangleDemo() : base()
        {
            Camera = new Camera()
            {
                RestrictMovementPlaneXZ = false,
            };

            _shader = new TextureShader(base.Renderer.Device);

            _start = false;
        }

        protected override List<Mesh> GetSceneMeshes()
        {
            return _meshes;
        }

        protected override void DrawScene()
        {
            Camera.Render();

            var viewMatrix = Camera.ViewMatrix;
            var projectionMatrix = Renderer.ProjectionMatrix;

            foreach (var mesh in _meshes)
            {
                if (mesh.GetTexture() != null)
                    mesh.Render(null, base.Renderer.Context, Camera, ref _meshRenderedCount);
            }
        }

        public void KeyDown(object sender, KeyEventArgs e)
        {
            _start = true;

           /* if (e.KeyCode == Keys.Up)
            {
                Camera.CurrentMoveState = MoveStateEnum.MoveForward;
            }
            else if (e.KeyCode == Keys.Down)
            {
                Camera.CurrentMoveState = MoveStateEnum.MoveBackward;
            }
            else if (e.KeyCode == Keys.Left)
            {
                Camera.CurrentMoveState = MoveStateEnum.MoveLeft;
            }
            else if (e.KeyCode == Keys.Right)
            {
                Camera.CurrentMoveState = MoveStateEnum.MoveRight;
            }
            else if (e.KeyCode == Keys.Q)
            {
                Camera.CurrentMoveState = MoveStateEnum.MoveUp;
            }
            else if (e.KeyCode == Keys.A)
            {
                Camera.CurrentMoveState = MoveStateEnum.MoveDown;
            }
            else if (e.KeyCode == Keys.Add)
            {
                Camera.CurrentMoveState = MoveStateEnum.IncreaseSpeed;
            }
            else if (e.KeyCode == Keys.Subtract)
            {
                Camera.CurrentMoveState = MoveStateEnum.DecreaseSpeed;
            }
            else if (e.KeyCode == Keys.R)
            {
                Camera.CurrentMoveState = MoveStateEnum.ResetPosition;
            }*/
        }

        protected override void UpdateScene()
        {
            _counter++;
        }

        protected override void InitializeScene()
        {
            base.Form.KeyDown += KeyDown;
            base.Form.MouseMove += MoveMouse;
            _counter = 0;

            Camera.SetPosition(0, 0, 0.0f);
            _shader.Initialize(base.Renderer.Device);

            var box1 = new BoxMesh(base.Renderer.Device, 0.5f, _shader);
            //box1.SetLocation(0.0f, 0.0f, 15.0f);
            box1.LoadTexture("terrain.dds");

            var box2 = new BoxMesh(base.Renderer.Device, 0.5f, _shader);
            //box2.SetLocation(-10.0f, 0.0f, 35.0f);
            box2.LoadTexture("terrain.dds");

            var triangle = new TriangleMesh(base.Renderer.Device, 0.5f, _shader);
            //triangle.SetLocation(0.0f, 0.0f, 0f);
            triangle.LoadTexture("terrain.dds");

            _meshes = new List<Mesh>
                {
                    box1,
                    box2,
                    triangle,
                };
        }

        private void MoveMouse(object sender, MouseEventArgs e)
        {
            if (!_start)
            {
                return;
            }

            Camera.RelativeX = (e.X - _previousMouseX) / 200;
            Camera.RelativeY = (e.Y - _previousMouseY) / 200;

            _previousMouseX = e.X;

            _previousMouseY = e.Y;
        }

        public void Dispose()
        {
            foreach (var mesh in _meshes)
                mesh.Dispose();

            _shader.Dispose();
        }
    }
}