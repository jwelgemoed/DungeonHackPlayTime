using DungeonHack.CollisionDetection;
using FunAndGamesWithSharpDX.DirectX;
using FunAndGamesWithSharpDX.Entities;
using SharpDX;
using System;
using DungeonHack.Engine;

namespace FunAndGamesWithSharpDX.Engine
{
    public class Camera
    {
        private Vector3 _eyeAt;
        private Vector3 _lookAt;
        private Vector3 _up;
        private Vector3 _startingEyeAt;
        private Matrix? _projectionMatrix;

        public float Height { get; set; }
        public float RelativeX { get; set; }
        public float RelativeY { get; set; }
        public MoveState CurrentMoveState { get; set; }

        public Vector3 EyeAt
        {
            get
            {
                return _eyeAt;
            }
        }

        public Vector3 TopdownEyePosition
        {
            get
            {
                var eyepos = _eyeAt;
                eyepos.Y += _topdownHeight;
                
                return eyepos;
            }
        }

        public Vector3 LookAt
        {
            get
            {
                return _lookAt;
            }
        }

        public MoveState PreviousMoveState { get; set; }

        public bool RestrictMovementPlaneXZ { get; set; }

        public float Speed { get; set; }

        private float _forwardSpeed;
        private float _backwardSpeed;
        private float _leftSpeed;
        private float _rightSpeed;
        private float _upSpeed;
        private float _downSpeed;
        private readonly float _acceleration;
        private readonly float _deAcceleration;
        private readonly float _maxAcceleration;
        private readonly float _sidewaysAcceleration;
        private readonly float _maxSidewaysAcceleration;
        private readonly float _upAcceleration;
        private readonly float _maxUpAcceleration;
        private readonly float _downAcceleration;
        private readonly float _maxDownAcceleration;
        private bool _topdown;
        private readonly float _zoomInAcceleration;
        private readonly float _zoomOutAcceleration;
        private readonly float _maxZoomAcceleration;

        public float RotationSpeed { get; set; }

        public Matrix ViewMatrix { get; set; }

        public Matrix FirstPersonViewMatrix { get; set; }

        public BoundingSphere CameraSphere;

        private bool _initialPosition = true;

        private Vertex _collidedVertex;
        private float _topdownHeight;

        public float FrameTime { get; set; }

        public float AspectRatio()
        {
            return Renderer.Width / (float) Renderer.Height;
        }

        public Matrix ProjectionMatrix
        {
            get
            {
                if (!_projectionMatrix.HasValue)
                {
                    const float fieldOfView = (float)Math.PI / 4.0f;
                    _projectionMatrix = Matrix.PerspectiveFovLH(fieldOfView, AspectRatio(), Renderer.ScreenNear, Renderer.ScreenFar);
                }

                return _projectionMatrix.Value;
            }
        }

        public Matrix ViewProjectionMatrix { get; private set; }

        public Matrix TopDownViewProjectionMatrix { get; private set; }

        public Matrix RenderViewProjectionMatrix
        {
            get
            {
                if (_topdown)
                {
                    return TopDownViewProjectionMatrix;
                }

                return ViewProjectionMatrix;
            }
        }

        public ICollisionDetector CollisionDetector { get; set; }
        private bool _wallClip = true;
        private Vector3 LastEyeAtWithNoCollision;

        public Camera()
        {
            _up = new Vector3(0, 1, 0);
            _lookAt = new Vector3(0, 0, 1);
            Speed = 2.0f;
            RotationSpeed = 45;
            CurrentMoveState = new MoveState();
            PreviousMoveState = new MoveState();
            _acceleration = ConfigManager.Acceleration;
            _deAcceleration = ConfigManager.DeAcceleration;
            _maxAcceleration = ConfigManager.MaxAcceleration;
            _sidewaysAcceleration = ConfigManager.SidewaysAcceleration;
            _maxSidewaysAcceleration = ConfigManager.MaxSidewaysAcceleration;
            _upAcceleration = ConfigManager.UpAcceleration;
            _maxUpAcceleration = ConfigManager.MaxUpAcceleration;
            _downAcceleration = ConfigManager.DownAcceleration;
            _maxDownAcceleration = ConfigManager.MaxDownAcceleration;
            _topdown = ConfigManager.Topdown;
            _zoomInAcceleration = ConfigManager.ZoomInAcceleration;
            _zoomOutAcceleration = ConfigManager.ZoomOutAcceleration;
            _maxZoomAcceleration = ConfigManager.MaxZoomAcceleration;
            _wallClip = ConfigManager.WallClipEnabled;
            _topdownHeight = 1750.0f;
        }

        internal void ToggleTopDownView()
        {
            _topdown = !_topdown;
        }

        public void SetPosition(float x, float y, float z)
        {
            if (_initialPosition)
            {
                _startingEyeAt.X = x;
                _startingEyeAt.Y = y;
                _startingEyeAt.Z = z;

                _initialPosition = false;
            }

            _eyeAt.X = x;
            _eyeAt.Y = y;
            _eyeAt.Z = z;
        }

        public Vector3 GetPosition()
        {
            return _eyeAt;
        }

        public void ResetCamera()
        {
            _eyeAt = _startingEyeAt;
            _up = new Vector3(0, 1, 0);
            _lookAt = new Vector3(0, 0, 1);
            Speed = 0.05f;
        }

        public void Move(MoveState moveState)
        {
            var direction = Vector3.Normalize(_lookAt - _eyeAt);
            Vector3[] invertedNormal = new Vector3[0];

            if (!_wallClip)
            {
                invertedNormal = CollisionDetector.HasCollided();
            }

            MoveForward(moveState, direction, invertedNormal);
            MoveBackward(moveState, direction, invertedNormal);
            MoveLeft(moveState, direction, invertedNormal);
            MoveRight(moveState, direction, invertedNormal);
            MoveUp(moveState, direction, invertedNormal);
            MoveDown(moveState, direction, invertedNormal);

            if (RestrictMovementPlaneXZ)
            {
                _eyeAt.Y = _startingEyeAt.Y;
            }
        }

        private void MoveRight(MoveState moveState, Vector3 direction, Vector3[] normals)
        {
            if (moveState.MoveRight)
            {
                _rightSpeed += FrameTime * _sidewaysAcceleration;

                if (_rightSpeed > (FrameTime * _maxSidewaysAcceleration))
                {
                    _rightSpeed = FrameTime * _maxSidewaysAcceleration;
                }

                direction = Vector3.Cross(direction, _up);
                direction = Vector3.Normalize(direction);

                if (normals.Length > 0)
                {
                    direction = GetDirectionAfterCollision(direction, normals, false);
                }

                if (RestrictMovementPlaneXZ)
                    direction.Y = 0.0f;

                _eyeAt -= direction * _rightSpeed;
                _lookAt -= direction * _rightSpeed;
            }
            else if (!moveState.MoveRight)
            {
                _rightSpeed -= FrameTime * _deAcceleration;

                if (_rightSpeed < 0.0f)
                {
                    _rightSpeed = 0.0f;
                }

                direction = Vector3.Cross(direction, _up);
                direction = Vector3.Normalize(direction);

                if (normals.Length > 0)
                {
                    direction = GetDirectionAfterCollision(direction, normals, false);
                }

                if (RestrictMovementPlaneXZ)
                    direction.Y = 0.0f;
                _eyeAt -= direction * _rightSpeed;
                _lookAt -= direction * _rightSpeed;
            }
        }

        private static Vector3 GetDirectionAfterCollision(Vector3 direction, Vector3[] normals, bool invertNormal = false)
        {
            Vector3 invertedNormal = new Vector3(0, 0, 0);

            if (invertNormal)
            {
                foreach (var normal in normals)
                {
                    invertedNormal -= normal * (direction * normal).Length();
                }
            }
            else
            {
                foreach (var normal in normals)
                {
                    invertedNormal += normal * (direction * normal).Length();
                }
            }

            return direction - invertedNormal; 
        }

        private void MoveLeft(MoveState moveState, Vector3 direction, Vector3[] normals)
        {
            if (moveState.MoveLeft)
            {
                _leftSpeed += FrameTime * _sidewaysAcceleration;

                if (_leftSpeed > (FrameTime * _maxSidewaysAcceleration))
                {
                    _leftSpeed = FrameTime * _maxSidewaysAcceleration;
                }

                direction = Vector3.Cross(direction, _up);
                direction = Vector3.Normalize(direction);

                if (normals.Length > 0)
                {
                    direction = GetDirectionAfterCollision(direction, normals, true);
                }

                if (RestrictMovementPlaneXZ)
                    direction.Y = 0.0f;

                _eyeAt += direction * _leftSpeed;
                _lookAt += direction * _leftSpeed;
            }
            else if (!moveState.MoveLeft)
            {
                _leftSpeed -= FrameTime * _deAcceleration;

                if (_leftSpeed < 0.0f)
                {
                    _leftSpeed = 0.0f;
                }

                direction = Vector3.Cross(direction, _up);
                direction = Vector3.Normalize(direction);

                if (normals.Length > 0)
                {
                    direction = GetDirectionAfterCollision(direction, normals, true);
                }

                if (RestrictMovementPlaneXZ)
                    direction.Y = 0.0f;
                _eyeAt += direction * _leftSpeed;
                _lookAt += direction * _leftSpeed;
            }
        }

        private void MoveBackward(MoveState moveState, Vector3 direction, Vector3[] normals)
        {
            if (moveState.MoveBackward)
            {
                _backwardSpeed += FrameTime * _acceleration;

                if (_backwardSpeed > (FrameTime * _maxAcceleration))
                {
                    _backwardSpeed = FrameTime * _maxAcceleration;
                }

                if (normals.Length > 0)
                {
                    direction = GetDirectionAfterCollision(direction, normals, false);
                }

                if (RestrictMovementPlaneXZ)
                    direction.Y = 0.0f;

                _eyeAt -= direction * _backwardSpeed;
                _lookAt -= direction * _backwardSpeed;
            }
            else if (!moveState.MoveBackward)
            {
                _backwardSpeed -= FrameTime * _deAcceleration;

                if (_backwardSpeed < 0.0f)
                {
                    _backwardSpeed = 0.0f;
                }

                if (normals.Length > 0)
                {
                    direction = GetDirectionAfterCollision(direction, normals, false);
                }

                if (RestrictMovementPlaneXZ)
                    direction.Y = 0.0f;

                _eyeAt -= direction * _backwardSpeed;
                _lookAt -= direction * _backwardSpeed;
            }
        }

        private void MoveForward(MoveState moveState, Vector3 direction, Vector3[] normals)
        {
            if (moveState.MoveForward)
            {
                _forwardSpeed += FrameTime * _acceleration;

                if (_forwardSpeed > (FrameTime * _maxAcceleration))
                {
                    _forwardSpeed = FrameTime * _maxAcceleration;
                }

                if (normals.Length > 0)
                {
                    direction = GetDirectionAfterCollision(direction, normals, true);
                }

                if (RestrictMovementPlaneXZ)
                    direction.Y = 0.0f;

                _eyeAt += direction * _forwardSpeed;
                _lookAt += direction * _forwardSpeed;
            }
            else if (!moveState.MoveForward)
            {
                _forwardSpeed -= FrameTime * _deAcceleration;

                if (_forwardSpeed < 0.0f)
                {
                    _forwardSpeed = 0.0f;
                }

                if (normals.Length > 0)
                {
                    direction = GetDirectionAfterCollision(direction, normals, true);
                }

                if (RestrictMovementPlaneXZ)
                    direction.Y = 0.0f;

                _eyeAt += direction * _forwardSpeed;
                _lookAt += direction * _forwardSpeed;
            }
        }

        public void MoveUp(MoveState moveState, Vector3 direction, Vector3[] normals)
        {
            if (moveState.MoveUp)
            {
                _upSpeed += FrameTime * _upAcceleration;

                if (_upSpeed > (FrameTime * _maxUpAcceleration))
                {
                    _upSpeed = FrameTime * _maxUpAcceleration;
                }

                direction = _up;
                direction = Vector3.Normalize(direction);

                if (normals.Length > 0)
                {
                    direction = GetDirectionAfterCollision(direction, normals);
                }

                _eyeAt += direction * _upSpeed;
                _lookAt += direction * _upSpeed;
            }
            else if (!moveState.MoveUp)
            {
                _upSpeed -= FrameTime * _deAcceleration;

                if (_upSpeed < 0.0f)
                {
                    _upSpeed = 0.0f;
                }

                direction = _up;
                direction = Vector3.Normalize(direction);

                if (normals.Length > 0)
                {
                    direction = GetDirectionAfterCollision(direction, normals);
                }

                _eyeAt += direction * _upSpeed;
                _lookAt += direction * _upSpeed;
            }
        }

        public void MoveDown(MoveState moveState, Vector3 direction, Vector3[] normals)
        {
            if (moveState.MoveDown)
            {
                _downSpeed += FrameTime * _downAcceleration;

                if (_downSpeed > (FrameTime * _maxDownAcceleration))
                {
                    _downSpeed = FrameTime * _maxDownAcceleration;
                }

                direction = _up;
                direction = Vector3.Normalize(direction);

                if (normals.Length > 0)
                {
                    direction = GetDirectionAfterCollision(direction, normals);
                }

                _eyeAt += direction * _downSpeed;
                _lookAt += direction * _downSpeed;
            }
            else if (!moveState.MoveUp)
            {
                _downSpeed -= FrameTime * _deAcceleration;

                if (_downSpeed < 0.0f)
                {
                    _downSpeed = 0.0f;
                }

                direction = _up;
                direction = Vector3.Normalize(direction);

                if (normals.Length > 0)
                {
                    direction = GetDirectionAfterCollision(direction, normals);
                }

                _eyeAt += direction * _downSpeed;
                _lookAt += direction * _downSpeed;
            }
        }

        public void Rotate(float relativeX, float relativeY)
        {
            if (_topdown)
                TopdownRotate(relativeX, relativeY);
            else
                FreeLookRotate(relativeX, relativeY);
        }

        private void FreeLookRotate(float relativeX, float relativeY)
        {
            Vector3 direction = Vector3.Normalize(_lookAt - _eyeAt);
            Vector3 rotAxis = Vector3.Cross(direction, _up);
            Matrix matRotAxis;
            Matrix matRotY;
            rotAxis = Vector3.Normalize(rotAxis);

            matRotY = Matrix.RotationY(relativeX / RotationSpeed);

            matRotAxis = Matrix.RotationAxis(rotAxis, relativeY / -RotationSpeed);

            direction = Vector3.TransformCoordinate(direction, matRotAxis * matRotY);
            _up = Vector3.TransformCoordinate(_up, matRotAxis * matRotY);

            _lookAt = direction + _eyeAt;
        }

        private void TopdownRotate(float relativeX, float relativeY)
        {
            Vector3 direction = Vector3.Normalize(_lookAt - _eyeAt);
            Vector3 rotAxis = Vector3.Cross(direction, _up);
            Matrix matRotAxis;
            Matrix matRotY;
            rotAxis = Vector3.Normalize(rotAxis);

            matRotY = Matrix.RotationY(relativeX / RotationSpeed);

            matRotAxis = Matrix.RotationAxis(rotAxis, relativeY / -RotationSpeed);
            direction = Vector3.TransformCoordinate(direction, matRotAxis * matRotY);
            _up = Vector3.TransformCoordinate(_up, matRotAxis * matRotY);
            
            _lookAt = direction + _eyeAt;
            _lookAt.Y = Height * 2;
        }

        public void DecreaseTopdownviewHeight()
        {
             _topdownHeight -= 50.0f;

            if (_topdownHeight <= 50.0f)
                _topdownHeight = 50.0f;
        }

        public void IncreaseTopdownviewHeight()
        {
            _topdownHeight += 50.0f;
        }

        public void Render()
        {
            CameraSphere = BoundingSphere.FromPoints(new Vector3[] { _eyeAt });

            CameraSphere.Radius = 10.0f;

            if (_topdown)
            {
                ViewMatrix = Matrix.LookAtLH(TopdownEyePosition, _eyeAt, _up);
                FirstPersonViewMatrix = Matrix.LookAtLH(_eyeAt, _lookAt, _up);
                TopDownViewProjectionMatrix = ViewMatrix * ProjectionMatrix;
            }
            else
            {
                ViewMatrix = FirstPersonViewMatrix = Matrix.LookAtLH(_eyeAt, _lookAt, _up);
            }

            ViewProjectionMatrix = FirstPersonViewMatrix * ProjectionMatrix;
        }
    }

    public class MoveState
    {
        public bool Stop { get; set; }
        public bool MoveForward { get; set; }
        public bool MoveBackward { get; set; }
        public bool MoveLeft { get; set; }
        public bool MoveRight { get; set; }
        public bool RotateLeft { get; set; }
        public bool RotateRight { get; set; }
        public bool MoveUp { get; set; }
        public bool MoveDown { get; set; }
        public bool IncreaseSpeed { get; set; }
        public bool DecreaseSpeed { get; set; }
        public bool ResetPosition { get; set; }
    }
}
