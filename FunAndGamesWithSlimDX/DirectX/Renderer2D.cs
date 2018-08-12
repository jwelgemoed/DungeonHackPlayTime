using FunAndGamesWithSharpDX.DirectX;
using SharpDX.Direct2D1;
using SharpDX.DirectWrite;
using SharpDX.DXGI;

namespace DungeonHack.DirectX
{
    public class Renderer2D
    {
        private RenderTarget _renderTarget;

        private SharpDX.Direct2D1.Device _device;

        private SharpDX.Direct2D1.DeviceContext _deviceContext;

        private Bitmap1 _target;
        private SolidColorBrush _redBrush;
        private SharpDX.DirectWrite.Factory _factoryDW;
        private Surface _surface;

        private BitmapProperties1 _bitmapProperties1;

        private bool _beginDrawCalled = false;
        private bool _endDrawCalled = false;

        public void Initialize(Renderer renderer)
        {
            _device = new SharpDX.Direct2D1.Device(renderer.DXGIDevice);
            _deviceContext = new SharpDX.Direct2D1.DeviceContext(_device, DeviceContextOptions.None);
            _surface = renderer.DXGISwapChain.GetBackBuffer<Surface>(0);

            var factory = new SharpDX.Direct2D1.Factory(SharpDX.Direct2D1.FactoryType.MultiThreaded, DebugLevel.Information);

            _renderTarget = new RenderTarget(factory, _surface,
                new RenderTargetProperties(
                    new PixelFormat(Format.R8G8B8A8_UNorm, SharpDX.Direct2D1.AlphaMode.Premultiplied)));

            _bitmapProperties1 = new BitmapProperties1(
                new PixelFormat(Format.R8G8B8A8_UNorm,
                        SharpDX.Direct2D1.AlphaMode.Premultiplied), 120, 120, BitmapOptions.Target | BitmapOptions.CannotDraw);

            _target = new Bitmap1(_deviceContext, _surface, _bitmapProperties1);

            _deviceContext.Target = _target;

            _redBrush = new SolidColorBrush(_deviceContext, new SharpDX.Mathematics.Interop.RawColor4(10, 0, 0, 10));

            _factoryDW = new SharpDX.DirectWrite.Factory();
        }

        public void BeginDraw()
        {
            if (_beginDrawCalled)
                return;

            _beginDrawCalled = true;
            _endDrawCalled = false;

            _deviceContext.BeginDraw();
        }

        public void EndDraw()
        {
            if (_endDrawCalled)
                return;

            _endDrawCalled = true;
            _beginDrawCalled = false;

            _deviceContext.EndDraw();
        }

        public void RenderText(string text, float posX, float posY)
        {
            string fontFamily = "Arial";
            float fontSize = 10;

            _deviceContext.DrawTextLayout(
                new SharpDX.Mathematics.Interop.RawVector2(posX, posY),
                new TextLayout(_factoryDW, text,
                new TextFormat(_factoryDW, fontFamily, fontSize), 1000, 500), _redBrush);
        }

        public void RenderBitmap(Bitmap bitmap)
        {
            _deviceContext.DrawBitmap(bitmap, 1.0f, BitmapInterpolationMode.Linear);
        }
    }
}
