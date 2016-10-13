using FunAndGamesWithSlimDX.DirectX;
using FunAndGamesWithSlimDX.Entities;
using SlimDX;
using System;

namespace FunAndGamesWithSlimDX.Engine
{
    public class Camera
    {
        private Vector3 _eyeAt;
        private Vector3 _lookAt;
        private Vector3 _up;
        private Vector3 _right;
        private Vector3 _forward;
        private Vector3 _startingEyeAt;
        private Matrix? _worldMatrix;
        private Matrix? _projectionMatrix;
        private Matrix? _orthMatrix;

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

        public Vector3 LookAt
        {
            get
            {
                return _lookAt;
            }
        }

        public MoveState PreviousMoveState { get; set; }

        public bool Collided { get; set; }

        public Vertex? CollidedVertex { get; set; }

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

        public float RotationSpeed { get; set; }

        public Matrix ViewMatrix { get; set; }

        public BoundingSphere CameraSphere;

        private bool _initialPosition = true;

        private Vertex _collidedVertex;

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

        public Matrix WorldMatrix
        {
            get
            {
                if (!_worldMatrix.HasValue)
                {
                    _worldMatrix = Matrix.Identity;
                }

                return _worldMatrix.Value;
            }
        }

        public Matrix OrthoMatrix
        {
            get
            {
                if (!_orthMatrix.HasValue)
                {
                    _orthMatrix = Matrix.OrthoLH(Renderer.Width, Renderer.Height, Renderer.ScreenNear, Renderer.ScreenFar);
                }

                return _orthMatrix.Value;
            }
        }

        public Camera()
        {
            _up = new Vector3(0, 1, 0);
            _right = new Vector3(1, 0, 0);
            _forward = new Vector3(0, 0, 1); 
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

            if (CollidedVertex.HasValue)
            {
                Vector3 invertedNormal = Vector3.Negate(CollidedVertex.Value.Normal);
                MoveForward(moveState, direction, invertedNormal);
                MoveBackward(moveState, direction, invertedNormal);
                MoveLeft(moveState, direction, invertedNormal);
                MoveRight(moveState, direction, invertedNormal);
                MoveUp(moveState, direction, invertedNormal);
                MoveDown(moveState, direction, invertedNormal);
            }
            else //Don't handle collisions
            {
                MoveForward(moveState, direction, null);
                MoveBackward(moveState, direction, null);
                MoveLeft(moveState, direction, null);
                MoveRight(moveState, direction, null);
                MoveUp(moveState, direction, null);
                MoveDown(moveState, direction, null);

                Collided = false;
            }

            CollidedVertex = null;

            if (RestrictMovementPlaneXZ)
            {
                _eyeAt.Y = _startingEyeAt.Y;
            }

        }

        private void MoveRight(MoveState moveState, Vector3 direction, Vector3? invertedNormal)
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

                if (invertedNormal.HasValue)
                {
                    Vector3 desiredMotion = (Vector3.Dot(direction, invertedNormal.Value) * invertedNormal.Value);
                    desiredMotion.Normalize();
                    direction = direction - desiredMotion;
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

                if (invertedNormal.HasValue)
                {
                    Vector3 desiredMotion = (Vector3.Dot(direction, invertedNormal.Value) * invertedNormal.Value);
                    desiredMotion.Normalize();
                    direction = direction - desiredMotion;
                }

                if (RestrictMovementPlaneXZ)
                    direction.Y = 0.0f;
                _eyeAt -= direction * _rightSpeed;
                _lookAt -= direction * _rightSpeed;
            }
        }

        private void MoveLeft(MoveState moveState, Vector3 direction, Vector3? invertedNormal)
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

                if (invertedNormal.HasValue)
                {
                    Vector3 desiredMotion = (Vector3.Dot(direction, invertedNormal.Value) * invertedNormal.Value);
                    desiredMotion.Normalize();
                    direction = direction - desiredMotion;
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

                if (invertedNormal.HasValue)
                {
                    Vector3 desiredMotion = (Vector3.Dot(direction, invertedNormal.Value) * invertedNormal.Value);
                    desiredMotion.Normalize();
                    direction = direction - desiredMotion;
                }

                if (RestrictMovementPlaneXZ)
                    direction.Y = 0.0f;
                _eyeAt += direction * _leftSpeed;
                _lookAt += direction * _leftSpeed;
            }
        }

        private void MoveBackward(MoveState moveState, Vector3 direction, Vector3? invertedNormal)
        {
            if (moveState.MoveBackward)
            {
                _backwardSpeed += FrameTime * _acceleration;

                if (_backwardSpeed > (FrameTime * _maxAcceleration))
                {
                    _backwardSpeed = FrameTime * _maxAcceleration;
                }

                if (invertedNormal.HasValue)
                {
                    Vector3 desiredMotion = (Vector3.Dot(direction, invertedNormal.Value) * invertedNormal.Value);
                    desiredMotion.Normalize();
                    direction = direction - desiredMotion;
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

                if (invertedNormal.HasValue)
                {
                    Vector3 desiredMotion = (Vector3.Dot(direction, invertedNormal.Value) * invertedNormal.Value);
                    desiredMotion.Normalize();
                    direction = direction - desiredMotion;
                }

                if (RestrictMovementPlaneXZ)
                    direction.Y = 0.0f;

                _eyeAt -= direction * _backwardSpeed;
                _lookAt -= direction * _backwardSpeed;
            }
        }

        private void MoveForward(MoveState moveState, Vector3 direction, Vector3? invertedNormal)
        {
            if (moveState.MoveForward)
            {
                _forwardSpeed += FrameTime * _acceleration;

                if (_forwardSpeed > (FrameTime * _maxAcceleration))
                {
                    _forwardSpeed = FrameTime * _maxAcceleration;
                }

                if (invertedNormal.HasValue)
                {
                    Vector3 desiredMotion = (Vector3.Dot(direction, invertedNormal.Value) * invertedNormal.Value);
                    desiredMotion.Normalize();
                    direction = direction - desiredMotion;
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

                if (invertedNormal.HasValue)
                {
                    Vector3 desiredMotion = (Vector3.Dot(direction, invertedNormal.Value) * invertedNormal.Value);
                    desiredMotion.Normalize();
                    direction = direction - desiredMotion;
                }

                if (RestrictMovementPlaneXZ)
                    direction.Y = 0.0f;
                _eyeAt += direction * _forwardSpeed;
                _lookAt += direction * _forwardSpeed;
            }
        }

        public void MoveUp(MoveState moveState, Vector3 direction, Vector3? invertedNormal)
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

                if (invertedNormal.HasValue)
                {
                    Vector3 desiredMotion = (Vector3.Dot(direction, invertedNormal.Value) * invertedNormal.Value);
                    desiredMotion.Normalize();
                    direction = direction - desiredMotion;
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

                if (invertedNormal.HasValue)
                {
                    Vector3 desiredMotion = (Vector3.Dot(direction, invertedNormal.Value) * invertedNormal.Value);
                    desiredMotion.Normalize();
                    direction = direction - desiredMotion;
                }

                _eyeAt += direction * _upSpeed;
                _lookAt += direction * _upSpeed;
            }
        }

        public void MoveDown(MoveState moveState, Vector3 direction, Vector3? invertedNormal)
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

                if (invertedNormal.HasValue)
                {
                    Vector3 desiredMotion = (Vector3.Dot(direction, invertedNormal.Value) * invertedNormal.Value);
                    desiredMotion.Normalize();
                    direction = direction - desiredMotion;
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

                if (invertedNormal.HasValue)
                {
                    Vector3 desiredMotion = (Vector3.Dot(direction, invertedNormal.Value) * invertedNormal.Value);
                    desiredMotion.Normalize();
                    direction = direction - desiredMotion;
                }

                _eyeAt += direction * _downSpeed;
                _lookAt += direction * _downSpeed;
            }
        }

        public void Rotate(float relativeX, float relativeY)
        {
            var direction = Vector3.Normalize(_lookAt - _eyeAt);
            var rotAxis = Vector3.Cross(direction, _up);
            rotAxis = Vector3.Normalize(rotAxis);

            var matRotAxis = Matrix.RotationAxis(rotAxis, relativeY/-RotationSpeed);
            var matRotY = Matrix.RotationY(relativeX/RotationSpeed);
            
            direction = Vector3.TransformCoordinate(direction, matRotAxis*matRotY);
            _up = Vector3.TransformCoordinate(_up, matRotAxis*matRotY);
            _lookAt = direction + _eyeAt;
        }

        public void Render()
        {
            //Rotate(RelativeX, RelativeY);
            //Move(CurrentMoveState);

            //Update boundingbox sphere of camera;
            CameraSphere = BoundingSphere.FromPoints(new Vector3[] { _eyeAt });

            CameraSphere.Radius = 5.0f;

            ViewMatrix = Matrix.LookAtLH(_eyeAt, _lookAt, _up);
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
