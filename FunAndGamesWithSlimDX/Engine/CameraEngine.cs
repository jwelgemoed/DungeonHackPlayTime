using FunAndGamesWithSharpDX.DirectX;
using SharpDX.Direct3D11;
using SharpDX.Multimedia;
using SharpDX.RawInput;
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using DungeonHack.Engine;
using SharpDX;
using Point = System.Drawing.Point;
using DungeonHack.OcclusionCulling;
using FunAndGamesWithSharpDX.Entities;

namespace FunAndGamesWithSharpDX.Engine
{
    public abstract class CameraEngine : DungeonHack.Engine.Engine, IDisposable
    {
        private float _previousMouseX;
        private float _previousMouseY;
        private readonly Point _centerPoint;
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
            SharpDX.RawInput.Device.RegisterDevice(UsagePage.Generic, UsageId.GenericMouse, DeviceFlags.None);
            SharpDX.RawInput.Device.MouseInput += MoveMouse;

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
            SharpDX.RawInput.Device.MouseInput -= MoveMouse;
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
                ConfigManager.SpotlightOn = !ConfigManager.SpotlightOn;
            }
            else if (e.KeyCode == Keys.CapsLock)
            {
                currentTechId = (currentTechId + 1) % 3;

                if ((ShaderTechnique) currentTechId == ShaderTechnique.WireFrame)
                {
                    Renderer.SetRasterizerState(FillMode.Wireframe, CullMode.Back);
                }
                else
                {
                    Renderer.SetRasterizerState(FillMode.Solid, CullMode.Back);
                }
            }
            else if (e.KeyCode == Keys.F2)
            {
                Camera.ToggleTopDownView();
            }
            else if (e.KeyCode == Keys.I)
            {
                ConfigManager.SpotLightFactor++;
            }
            else if (e.KeyCode == Keys.U)
            {
                ConfigManager.SpotLightFactor--;
            }
            else if (e.KeyCode == Keys.O)
            {
                ConfigManager.SpotLightRange -= 10;
            }
            else if (e.KeyCode == Keys.P)
            {
                ConfigManager.SpotLightRange += 10;
            }
            else if (e.KeyCode == Keys.G)
            {
                ConfigManager.SpotLightAttentuationA -= 1.0f;

                if (ConfigManager.SpotLightAttentuationA <= 0)
                    ConfigManager.SpotLightAttentuationA = 0;
            }
            else if (e.KeyCode == Keys.H)
            {
                ConfigManager.SpotLightAttentuationA += 1.0f;
            }
            else if (e.KeyCode == Keys.J)
            {
                ConfigManager.SpotLightAttentuationB -= 1.0f;

                if (ConfigManager.SpotLightAttentuationB <= 0)
                    ConfigManager.SpotLightAttentuationB = 0;
            }
            else if (e.KeyCode == Keys.K)
            {
                ConfigManager.SpotLightAttentuationB += 1.0f;
            }
            else if (e.KeyCode == Keys.L)
            {
                ConfigManager.SpotLightAttentuationC -= 1.0f;

                if (ConfigManager.SpotLightAttentuationC <= 0)
                    ConfigManager.SpotLightAttentuationC = 0;
            }
            else if (e.KeyCode == Keys.OemSemicolon)
            {
                ConfigManager.SpotLightAttentuationC += 1.0f;
            }
            else if (e.KeyCode == Keys.V)
            {
                ConfigManager.FogStart -=5;
                _console.WriteLine($"Fog Start: {ConfigManager.FogStart}");
            }
            else if (e.KeyCode == Keys.B)
            {
                ConfigManager.FogStart +=5;
                _console.WriteLine($"Fog Start: {ConfigManager.FogStart}");
            }
            else if (e.KeyCode == Keys.N)
            {
                ConfigManager.FogEnd -= 5;
                _console.WriteLine($"Fog End: {ConfigManager.FogEnd}");
            }
            else if (e.KeyCode == Keys.M)
            {
                ConfigManager.FogEnd += 5;
                _console.WriteLine($"Fog End: {ConfigManager.FogEnd}");
            }
            else if (e.KeyCode == Keys.D1)
            {
                if (ConfigManager.UseNormalMap == 1)
                    ConfigManager.UseNormalMap = 0;
                else
                    ConfigManager.UseNormalMap = 1;
            }
            else if (e.KeyCode == Keys.Space)
            {
                if (ApplicationStateEngine.CurrentState != ApplicationStateEnum.Interactive)
                {
                    ApplicationStateEngine.CurrentState = ApplicationStateEnum.Interactive;
                    SharpDX.RawInput.Device.MouseInput -= MoveMouse;
                    SharpDX.RawInput.Device.MouseInput += MoveMouseInteractive;
                    Cursor.Show();
                }
                else
                {
                    Cursor.Hide();
                    SharpDX.RawInput.Device.MouseInput -= MoveMouseInteractive;
                    SharpDX.RawInput.Device.MouseInput += MoveMouse;
                    ApplicationStateEngine.CurrentState = ApplicationStateEnum.Normal;
                }
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

            if (e.ButtonFlags == MouseButtonFlags.Button1Down)
            {
                OnFirePrimary();
            }
        }

        protected abstract void OnFirePrimary();

        protected void MoveMouseInteractive(object sender, MouseInputEventArgs e)
        {
            if (e.ButtonFlags == MouseButtonFlags.Button1Down)
            {
                var point = Cursor.Position;

                var pickedPolygon = InteractiveEngine.GetPickedPolygon(new SharpDX.Point(point.X, point.Y), Camera, GetRenderedItems());

                if (pickedPolygon != null)
                    pickedPolygon.TextureIndex = 9;
            }
        }

        public override void UpdateScene()
        {
            Camera.FrameTime = (float) Timer.DeltaTime *100.0f;
            Camera.Move(Camera.CurrentMoveState);
            Camera.Render();
        }
    }
}