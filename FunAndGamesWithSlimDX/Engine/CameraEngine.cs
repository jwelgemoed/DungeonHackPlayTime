using SlimDX.Direct3D11;
using SlimDX.Multimedia;
using SlimDX.RawInput;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace FunAndGamesWithSlimDX.Engine
{
    public abstract class CameraEngine : Engine, IDisposable
    {
        private float _previousMouseX;
        private float _previousMouseY;
        private readonly Point _centerPoint;
        private readonly List<string> _shaderTechniques = new List<string>() { "LightTech", "TextureTech", "NoSpotSpotLightTech" };
        private int currentTechId = 0;
        private bool _flashLightOn = true;

        public CameraEngine(float cameraHeight, bool restrictMovementPlaneXZ)
        {
            Camera = new Camera()
            {
                RestrictMovementPlaneXZ = restrictMovementPlaneXZ,
                Height = cameraHeight
            };

            Form.KeyDown += KeyDown;
            Form.KeyUp += KeyUp;
            SlimDX.RawInput.Device.RegisterDevice(UsagePage.Generic, UsageId.Mouse, SlimDX.RawInput.DeviceFlags.None);
            SlimDX.RawInput.Device.MouseInput += MoveMouse;

            _centerPoint = Form.PointToScreen(new Point(Form.ClientSize.Width / 2, Form.ClientSize.Height / 2));
        }

        public void Start()
        {
            Cursor.Hide();
        }

        public void Dispose()
        {
            Form.KeyUp -= KeyUp;
            Form.KeyDown -= KeyDown;
            SlimDX.RawInput.Device.MouseInput -= MoveMouse;
            Cursor.Show();
        }

        protected void KeyDown(object sender, KeyEventArgs e)
        {
            if ((e.KeyCode == Keys.Up) || (e.KeyCode == Keys.W))
            {
                Camera.CurrentMoveState.MoveForward = true;
            }
            else if ((e.KeyCode == Keys.Down) || (e.KeyCode == Keys.S))
            {
                Camera.CurrentMoveState.MoveBackward = true;
            }
            else if ((e.KeyCode == Keys.Left) || (e.KeyCode == Keys.A))
            {
                Camera.CurrentMoveState.MoveLeft = true;
            }
            else if ((e.KeyCode == Keys.Right) || (e.KeyCode == Keys.D))
            {
                Camera.CurrentMoveState.MoveRight = true;
            }
            else if (e.KeyCode == Keys.Q)
            {
                Camera.CurrentMoveState.MoveUp = true;
            }
            else if (e.KeyCode == Keys.Z)
            {
                Camera.CurrentMoveState.MoveDown = true;
            }
            else if (e.KeyCode == Keys.Add)
            {
                Camera.Speed += 0.05f;
            }
            else if (e.KeyCode == Keys.Subtract)
            {
                Camera.Speed -= 0.05f;
            }
            else if (e.KeyCode == Keys.R)
            {
                Camera.ResetCamera();
            }
            else if (e.KeyCode == Keys.F)
            {
                _flashLightOn = !_flashLightOn;

                if (_flashLightOn)
                {
                    Shader.SetSelectedShaderEffect(Renderer.Device, "NoSpotSpotLightTech");
                }
                else
                {
                    Shader.SetSelectedShaderEffect(Renderer.Device, "LightTech");
                }
                
            }
            else if (e.KeyCode == Keys.CapsLock)
            {
                currentTechId = (currentTechId + 1) % (_shaderTechniques.Count);

                if (currentTechId == _shaderTechniques.Count - 1) //last one is always wireframe
                {
                    Renderer.SetRasterizerState(FillMode.Wireframe, CullMode.Back);
                }
                else
                {
                    Renderer.SetRasterizerState(FillMode.Solid, CullMode.Back);
                    Shader.SetSelectedShaderEffect(Renderer.Device, _shaderTechniques[currentTechId]);
                }
            }
            else if (e.KeyCode == Keys.F2)
            {
                Camera.ToggleTopDownView();
            }

        }

        protected void KeyUp(object sender, KeyEventArgs e)
        {
            if ((e.KeyCode == Keys.Up) || (e.KeyCode == Keys.W))
            {
                Camera.CurrentMoveState.MoveForward = false;
            }
            else if ((e.KeyCode == Keys.Down) || (e.KeyCode == Keys.S))
            {
                Camera.CurrentMoveState.MoveBackward = false;
            }
            else if ((e.KeyCode == Keys.Left) || (e.KeyCode == Keys.A))
            {
                Camera.CurrentMoveState.MoveLeft = false;
            }
            else if ((e.KeyCode == Keys.Right) || (e.KeyCode == Keys.D))
            {
                Camera.CurrentMoveState.MoveRight = false;
            }
            else if (e.KeyCode == Keys.Q)
            {
                Camera.CurrentMoveState.MoveUp = false;
            }
            else if (e.KeyCode == Keys.Z)
            {
                Camera.CurrentMoveState.MoveDown = false;
            }
            else if (e.KeyCode == Keys.Add)
            {
                Camera.CurrentMoveState.IncreaseSpeed = false;
            }
            else if (e.KeyCode == Keys.Subtract)
            {
                Camera.CurrentMoveState.DecreaseSpeed = false;
            }
            else if (e.KeyCode == Keys.R)
            {
                Camera.CurrentMoveState.ResetPosition = false;
            }
            else if (e.KeyCode == Keys.PageDown)
            {
                Camera.DecreaseTopdownviewHeight();
            }
            else if (e.KeyCode == Keys.PageUp)
            {
                Camera.IncreaseTopdownviewHeight();
            }

        }

        protected void MoveMouse(object sender, MouseInputEventArgs e)
        {
            Camera.RelativeX = (e.X - _previousMouseX) * 0.1f;
            Camera.RelativeY = (e.Y - _previousMouseY) * 0.1f;

            _previousMouseX = -e.X;
            _previousMouseY = -e.Y;
            Cursor.Position = _centerPoint;

            //Player.Rotate();
            Camera.Rotate(Camera.RelativeX, Camera.RelativeY);
        }

        public override void UpdateScene()
        {
            Camera.FrameTime = (float) Timer.DeltaTime * 100;
            Camera.Move(Camera.CurrentMoveState);
            Camera.Render();
        }
    }
}