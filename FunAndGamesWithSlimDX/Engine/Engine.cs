using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Forms;
using DungeonHack.DirectX;
using DungeonHack.Entities;
using DungeonHack.OcclusionCulling;
using FunAndGamesWithSharpDX.DirectX;
using FunAndGamesWithSharpDX.Engine;
using FunAndGamesWithSharpDX.Entities;
using FunAndGamesWithSharpDX.Menus;
using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.DirectWrite;
using SharpDX.DXGI;
using SharpDX.Windows;

namespace DungeonHack.Engine
{
    /// <summary>
    /// Engine class : main class responsible for doing all engine related work
    /// </summary>
    public abstract class Engine
    {
        protected RenderForm Form = new RenderForm();
        protected Renderer Renderer = new Renderer();
        protected Renderer2D Renderer2D = new Renderer2D();
        protected DeferredShadingRenderer DeferredShadingRenderer = new DeferredShadingRenderer();
        protected InteractiveEngine InteractiveEngine = new InteractiveEngine();
        protected Shader Shader;
        protected readonly GameTimer Timer = new GameTimer();
        protected Camera Camera;
        private bool _hasInitialized = false;
        protected int MouseSensitivity;
        protected FunAndGamesWithSharpDX.Entities.Console _console;
        protected Frustrum _frustrum;
        protected int _meshRenderedCount = 0;

        static private int _frameCount = 0;
        static private float _timeElapsedForStats = 0.0f;
        static private float _timeElapsedForDisplay = 0.0f;

        private long _updateTime;
        private long _drawTime;

        private bool ShutDownAllTasks;

        protected Stopwatch _stopwatch = new Stopwatch();

        public event MouseEventHandler OnMouseUp;

        public event MouseEventHandler OnMouseDown;

        public event MouseEventHandler OnMouseMove;

        public event KeyEventHandler OnKeyDown;

        private Sprite sprite;

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
            Renderer.Initialize(Form.Handle, 4);

            Renderer2D.Initialize(Renderer);

            DeferredShadingRenderer.Width = ConfigManager.ScreenWidth;
            DeferredShadingRenderer.Height = ConfigManager.ScreenHeight;

            DeferredShadingRenderer.Initialize(Renderer);

            InteractiveEngine.Initialize(Renderer);

            _frustrum = new Frustrum();

            Shader = new Shader(Renderer, DeferredShadingRenderer, Camera);
            Shader.Initialize();

            SpriteRenderer.Initialize(Renderer.Device);
            FontRenderer.Initialize(Renderer.Device, "Arial", FontWeight.Normal, FontStyle.Normal, FontStretch.Normal, 12);

            _console = new FunAndGamesWithSharpDX.Entities.Console(null, new Vector2(Form.Width - 300, -250),
                new Vector2(400, 400), 100, Colors.White, Renderer2D);

            ApplicationStateEngine.CurrentState = ApplicationStateEnum.Normal;
            _hasInitialized = true;

            sprite = new Sprite(Renderer.Device, "crate2_diffuse.bmp", 0, 0, 100, 100);
        }

        public virtual void Run()
        {
            if (!_hasInitialized)
                Initialize();

            InitializeScene();

            DepthBufferRenderer.Setup(Renderer);

            Timer.Start();

            Task PreRenderTask = new Task(PreRenderLoop);
            Task UpdateSceneTask = new Task(UpdateSceneLoop);

            PreRenderTask.Start();

            RenderLoop.Run(Form, MainLoop);

            Task.WaitAll(PreRenderTask); 

            Timer.Stop();

            Shutdown();
        }

        private void PreRenderLoop()
        {
            while (!ShutDownAllTasks)
            {
                switch (ApplicationStateEngine.CurrentState)
                {
                    case ApplicationStateEnum.Normal:
                        PreRenderScene();
                        break;
                    default:
                        break;
                }
            }
        }

        private void UpdateSceneLoop()
        {
            while (!ShutDownAllTasks)
            {
                switch (ApplicationStateEngine.CurrentState)
                {
                    case ApplicationStateEnum.Normal:
                        UpdateScene();
                        DepthBufferRenderer.RenderToScreen(Renderer2D);
                        break;
                    default:
                        break;
                }
            }
        }

        private void MainLoop()
        {
            switch (ApplicationStateEngine.CurrentState)
            {
                case ApplicationStateEnum.Shutdown:
                    Form.Close();
                    ShutDownAllTasks = true;
                    return;

                case ApplicationStateEnum.Normal:
                    //Timer.Tick();
                    //_frameCount++;
                    //_frameRateStats = CalculateFrameRateStats();

                    //Task updateTask = new Task(() => UpdateScene());

                    //Task drawSceneTask = new Task(() =>
                    {
                        Timer.Tick();
                        _frameCount++;
                        _frameRateStats = CalculateFrameRateStats();

                        UpdateScene();

                      //  Renderer.ImmediateContext.ClearRenderTargetView(Renderer.RenderTarget, Colors.Black);
                      //  Renderer.ImmediateContext.ClearDepthStencilView(Renderer.DepthStencilView, DepthStencilClearFlags.Depth, 1.0f, 0);

                        _stopwatch.Restart();

                        DrawScene();

                        DisplayConsoleInformation();
                        DepthBufferRenderer.RenderToScreen(Renderer2D);
                        Renderer.SwapChain.Present(ConfigManager.VSync, PresentFlags.None);
                        // DepthBufferRenderer.RenderToScreen(Renderer2D);
                        // Renderer.SwapChain.Present(ConfigManager.VSync, PresentFlags.None);

                        _stopwatch.Stop();
                    }
                   // );

                    //updateTask.Start();
                    //drawSceneTask.Start();

                    //Task.WaitAll(drawSceneTask);//updateTask, drawSceneTask);

                    _updateTime = _stopwatch.ElapsedMilliseconds;
                    break;

                case ApplicationStateEnum.OpenSettings:
                    ShowOptionsMenu();
                    break;
                case ApplicationStateEnum.Interactive:
                    Timer.Tick();
                    break;
            }
        }

        private void ShowMouseCursor()
        {
            Cursor.Show();
        }

        public abstract void PreRenderScene();

        /// <summary>
        /// Override point to render at.
        /// </summary>
        public abstract void DrawScene();

        /// <summary>
        /// Extension point : Provide custom update method that updates the scene.
        /// </summary>
        public abstract void UpdateScene();

        /// <summary>
        /// Extension point : Provide custom initialize method that initializes the scene.
        /// </summary>
        public abstract void InitializeScene();

        /// <summary>
        /// Returns meshes.
        /// </summary>
        /// <returns></returns>
        public abstract List<Polygon> GetSceneMeshes();

        public abstract RenderedItems GetRenderedItems();

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

                //var cameraPosition = Camera.GetPosition();
                //_console.WriteLine($"Camera Position: X: {cameraPosition.X}, Y: {cameraPosition.Y}, Z: {cameraPosition.Z}");

                //var lookat = Vector3.Normalize(Camera.LookAt);
                // _console.WriteLine($"Camera Lookat: X : {lookat.X}, Y: {lookat.Y}, Z: {lookat.Z}");

                //var meshPosition = GetSceneMeshes()[0].VertexData[0].Position;
                //_console.WriteLine($"Mesh 0 Position: X: {meshPosition.X}, Y:{meshPosition.Y}, Z: {meshPosition.Z}");

                //_console.WriteLine($"Update time in ticks: {_updateTime}");
                //_console.WriteLine($"Draw time in ticks: {_drawTime}");
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