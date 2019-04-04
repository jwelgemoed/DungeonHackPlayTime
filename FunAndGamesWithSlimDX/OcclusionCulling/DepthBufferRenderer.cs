using FunAndGamesWithSharpDX.DirectX;
using FunAndGamesWithSharpDX.Engine;
using SharpDX.Direct3D11;
using SharpDX.Direct2D1;

using System;
using SharpDX.DXGI;
using SharpDX.DirectWrite;
using DungeonHack.DirectX;

namespace DungeonHack.OcclusionCulling
{
    public static class DepthBufferRenderer
    {
        public static DepthBuffer DepthBuffer { get; set; }

        private static RenderTarget _renderTarget;

        private static SharpDX.Direct2D1.Device _device;

        private static SharpDX.Direct2D1.DeviceContext _deviceContext;

        private static Bitmap1 _target;
        private static SolidColorBrush _redBrush;
        private static SharpDX.DirectWrite.Factory _factoryDW;
        private static Surface _surface;

        private static BitmapProperties1 _bitmapProperties1;
        private static byte[] _buffer;

        private static Bitmap _backBufferBmp;

        public static void RenderToScreen(Renderer2D renderer)
        {
            // Copy pixels from screen capture Texture to GDI bitmap
            for (int y = 0; y < DepthBuffer.Height; y++)
            for (int x = 0; x < DepthBuffer.Width; x++)
            {
                int depthBufferEntry = (y * DepthBuffer.Width) + (x);
                int bufferLocation = depthBufferEntry * 4;
                byte value = (byte) (((Math.Abs(DepthBuffer.ShadowBuffer[depthBufferEntry]) / 1)));
                //value = (byte) (value / 3);

                var color = System.Drawing.Color.FromArgb(value, 0, 0);

                _buffer[bufferLocation] = color.R;
                _buffer[bufferLocation + 1] = color.G;
                _buffer[bufferLocation + 2] = color.B;
                _buffer[bufferLocation + 3] = color.A;
            }

            _backBufferBmp.CopyFromMemory(_buffer, DepthBuffer.Width * 4);

            _deviceContext.Target = _target;
            _deviceContext.BeginDraw();
            _deviceContext.DrawBitmap(_backBufferBmp, 1.0f, BitmapInterpolationMode.Linear);
            _deviceContext.EndDraw();
        }

        public static void Setup(Renderer renderer)
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

            _buffer = new byte[DepthBuffer.Width * DepthBuffer.Height * 4];

            _backBufferBmp = new Bitmap(_deviceContext,
                new SharpDX.Size2(DepthBuffer.Width, DepthBuffer.Height),
                new BitmapProperties(_deviceContext.PixelFormat));
        }
    }
}
