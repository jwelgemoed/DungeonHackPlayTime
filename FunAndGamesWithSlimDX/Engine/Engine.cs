using FunAndGamesWithSlimDX.DirectX;
using FunAndGamesWithSlimDX.Entities;
using FunAndGamesWithSlimDX.FX;
using FunAndGamesWithSlimDX.Menus;
using SlimDX;
using SlimDX.Direct3D11;
using SlimDX.DirectWrite;
using SlimDX.DXGI;
using SlimDX.Windows;
using System.Collections.Generic;
using System.Windows.Forms;

namespace FunAndGamesWithSlimDX.Engine
{
    /// <summary>
    /// Engine class : main class responsible for doing all engine related work
    /// </summary>
    public abstract class Engine
    {
        protected RenderForm Form = new RenderForm();
        protected Renderer Renderer = new Renderer();
        protected IShader Shader;
        protected readonly GameTimer Timer = new GameTimer();
        protected Camera Camera;
        private bool _hasInitialized = false;
        protected int MouseSensitivity;
        protected Console _console;
        protected Frustrum _frustrum;
        protected int _meshRenderedCount = 0;

        static private int _frameCount = 0;
        static private float _timeElapsedForStats = 0.0f;
        static private float _timeElapsedForDisplay = 0.0f;

        public event MouseEventHandler OnMouseUp;

        public event MouseEventHandler OnMouseDown;

        public event MouseEventHandler OnMouseMove;

        public event KeyEventHandler OnKeyDown;

        public struct FrameRateStats
        {
            public float FramesPerSecond;
            public float FrameTime;
        }

        private FrameRateStats _frameRateStats = new FrameRateStats();

        public void HandleOptionsKeyPress(object sender, KeyEventArgs e)
        {
            if (ApplicationStateEngine.CurrentState != ApplicationStateEnum.Normal)
                return;

            if (e.KeyCode == Keys.Escape)
                ApplicationStateEngine.CurrentState = ApplicationStateEnum.Shutdown;

            if (e.KeyCode == Keys.F1)
                ApplicationStateEngine.CurrentState = ApplicationStateEnum.OpenSettings;
        }

        public void Initialize()
        {
            Form.StartPosition = FormStartPosition.CenterScreen;
            Form.KeyDown += HandleOptionsKeyPress;
            Form.KeyDown += OnKeyDown;

            Form.MouseMove += OnMouseMove;
            Form.MouseDown += OnMouseDown;
            Form.MouseUp += OnMouseUp;

            if (!ConfigManager.FullScreen)
            {
                Form.Width = ConfigManager.ScreenWidth;
                Form.Height = ConfigManager.ScreenHeight;
                Form.StartPosition = FormStartPosition.CenterScreen;
            }

            MouseSensitivity = ConfigManager.MouseSensitivity;
            Renderer.ScreenNear = ConfigManager.ScreenNear;
            Renderer.ScreenFar = ConfigManager.ScreenFar;
            Renderer.Width = ConfigManager.ScreenWidth;
            Renderer.Height = ConfigManager.ScreenHeight;
            Renderer.Use4XMSAA = ConfigManager.Use4XMSAA;
            Renderer.FullScreen = ConfigManager.FullScreen;
            Renderer.Initialize(Form.Handle);

            _frustrum = new Frustrum();

            Shader = new Shader(Renderer.Device);
            Shader.Initialize(Renderer.Device);

            SpriteRenderer.Initialize(Renderer.Device);
            FontRenderer.Initialize(Renderer.Device, "Arial", FontWeight.Normal, FontStyle.Normal, FontStretch.Normal, 12);

            _console = new Console(null, new Vector2(1600, -250), new Vector2(400, 400), 100, Colors.White);

            ApplicationStateEngine.CurrentState = ApplicationStateEnum.Normal;
            _hasInitialized = true;
        }

        public virtual void Run()
        {
            if (!_hasInitialized)
                Initialize();

            InitializeScene();

            Timer.Start();

            MessagePump.Run(Form, MainLoop);

            Timer.Stop();

            Shutdown();
        }

        private void MainLoop()
        {
            switch (ApplicationStateEngine.CurrentState)
            {
                case ApplicationStateEnum.Shutdown:
                    Form.Close();
                    return;

                case ApplicationStateEnum.Normal:
                    Timer.Tick();
                    _frameRateStats = CalculateFrameRateStats();
                    Renderer.Context.ClearRenderTargetView(Renderer.RenderTarget, Colors.Black);
                    Renderer.Context.ClearDepthStencilView(Renderer.DepthStencilView, DepthStencilClearFlags.Depth, 1.0f, 0);
                    UpdateScene();
                    DrawScene();
                    DisplayConsoleInformation();
                    FontRenderer.FinalizeDraw();
                    SpriteRenderer.FinalizeDraw();
                    Renderer.SwapChain.Present(0, PresentFlags.None);
                    break;

                case ApplicationStateEnum.OpenSettings:
                    ShowOptionsMenu();
                    break;
            }
        }

        /// <summary>
        /// Override point to render at.
        /// </summary>
        protected abstract void DrawScene();

        /// <summary>
        /// Extension point : Provide custom update method that updates the scene.
        /// </summary>
        protected abstract void UpdateScene();

        /// <summary>
        /// Extension point : Provide custom initialize method that initializes the scene.
        /// </summary>
        protected abstract void InitializeScene();

        /// <summary>
        /// Returns meshes.
        /// </summary>
        /// <returns></returns>
        protected abstract List<Mesh> GetSceneMeshes();

        /// <summary>
        /// Start the engine's timer.
        /// </summary>
        public void StartGameTimer()
        {
            Timer.Start();
        }

        /// <summary>
        /// Stops the engine's timer.
        /// </summary>
        public void StopGameTimer()
        {
            Timer.Stop();
        }

        public FrameRateStats CalculateFrameRateStats()
        {
            _frameCount++;

            if ((Timer.TotalTime() - _timeElapsedForStats) >= 1.0f)
            {
                _frameRateStats.FramesPerSecond = _frameCount;
                _frameRateStats.FrameTime = 1000.0f / _frameCount;

                _frameCount = 0;
                _timeElapsedForStats += 1.0f;

                //_console.WriteLine(string.Format("Camera Position: X: {0}, Y: {0}, Z: {0}", cameraPosition.X, cameraPosition.Y, cameraPosition.Z));
            }

            return _frameRateStats;
        }

        protected void DisplayConsoleInformation()
        {
            if ((Timer.TotalTime() - _timeElapsedForDisplay) >= 1.0f)
            {
                _timeElapsedForDisplay += 1.00f; //0.25 quarter of a second, 1.0 = second

                Form.Text = $"FPS : {_frameRateStats.FramesPerSecond} Game Time : {(int)Timer.TotalTime()}";
                _console.WriteLine(Form.Text);
                _console.WriteLine("Mesh rendered : " + _meshRenderedCount + " from " + GetSceneMeshes().Count);
                var cameraPosition = Camera.GetPosition();
                _console.WriteLine($"Camera Position: X: {cameraPosition.X}, Y: {cameraPosition.Y}, Z: {cameraPosition.Z}");
                var lookat = Vector3.Normalize(Camera.LookAt);
                _console.WriteLine($"Camera Lookat: X : {lookat.X}, Y: {lookat.Y}, Z: {lookat.Z}");

                var meshPosition = GetSceneMeshes()[0].VertexData[0].Position;
                _console.WriteLine($"Mesh 0 Position: X: {meshPosition.X}, Y:{meshPosition.Y}, Z: {meshPosition.Z}");
            }
            _console.Draw();
        }

        private void ShowOptionsMenu()
        {
            ApplicationStateEngine.CurrentState = ApplicationStateEnum.EditSettings;
            var optionsWindow = new OptionsWindow(Renderer, Shader, _frameRateStats, Camera, MouseSensitivity);
            optionsWindow.Show();
        }

        public virtual void Shutdown()
        {
            SpriteRenderer.Dispose();
            Renderer.Dispose();
            Shader.Dispose();
        }
    }
}