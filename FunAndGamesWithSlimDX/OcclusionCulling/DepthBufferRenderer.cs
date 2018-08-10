using FunAndGamesWithSharpDX.DirectX;
using FunAndGamesWithSharpDX.Engine;
using SharpDX.Direct3D11;
using SharpDX.Direct2D1;

using System;
using SharpDX.DXGI;
using SharpDX.DirectWrite;

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

        public static void SaveToFile()
        {
            string outputFileName = "c:\\textures\\output.bmp";
            // Create Drawing.Bitmap
            var bitmap = new System.Drawing.Bitmap(DepthBuffer.Width, DepthBuffer.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            var boundsRect = new System.Drawing.Rectangle(0, 0, DepthBuffer.Width, DepthBuffer.Height);

            // Copy pixels from screen capture Texture to GDI bitmap
            for (int y = 0; y < DepthBuffer.Height; y++)
                for (int x = 0; x < DepthBuffer.Width; x++)
                {
                    int depthBufferEntry = (y * DepthBuffer.Width) + x;
                    byte value = (byte) ((255 - (Math.Abs(DepthBuffer.Buffer[depthBufferEntry]) / 10)));
                    //value = (byte) (value / 3);

                    var color = System.Drawing.Color.FromArgb(value, 0, 0);

                    bitmap.SetPixel(x, y, color);
                }

            // Release source and dest locks

            // Save the output
            bitmap.Save(outputFileName);
        }

        public static void RenderToScreen(Renderer renderer)
        {
            var buffer = new byte[DepthBuffer.Width * DepthBuffer.Height * 4];
            var backBufferBmp = new Bitmap(_deviceContext, new SharpDX.Size2(DepthBuffer.Width, DepthBuffer.Height), 
                new BitmapProperties(_deviceContext.PixelFormat));

            // Copy pixels from screen capture Texture to GDI bitmap
            for (int y = 0; y < DepthBuffer.Height; y++)
                for (int x = 0; x < DepthBuffer.Width; x++)
                {
                    int depthBufferEntry = (y * DepthBuffer.Width) + (x);
                    int bufferLocation = depthBufferEntry * 4;
                    byte value = (byte)(((Math.Abs(DepthBuffer.Buffer[depthBufferEntry]) / 1)));
                    //value = (byte) (value / 3);

                    var color = System.Drawing.Color.FromArgb(value, 0, 0);

                    buffer[bufferLocation] = color.R;
                    buffer[bufferLocation + 1] = color.G;
                    buffer[bufferLocation + 2] = color.B;
                    buffer[bufferLocation + 3] = color.A;
                }

            //var bitmap = new Bitmap1(_deviceContext, new SharpDX.Size2(DepthBuffer.Width, DepthBuffer.Height));
            backBufferBmp.CopyFromMemory(buffer, DepthBuffer.Width * 4);

            _deviceContext.Target = _target;
            _deviceContext.BeginDraw();
            _deviceContext.DrawText("HALLO WORLD", new TextFormat(_factoryDW, "Arial", 20),
                new SharpDX.Mathematics.Interop.RawRectangleF(0, 0, 500, 500), _redBrush);
            // _deviceContext.FillRectangle(new SharpDX.Mathematics.Interop.RawRectangleF(0, 0, 100, 100), _redBrush);
            _deviceContext.DrawBitmap(backBufferBmp, 1.0f, BitmapInterpolationMode.Linear);
            _deviceContext.EndDraw();

            //SpriteRenderer.Draw(new ShaderResourceView(renderer.Device, texture2d),
            //    new SharpDX.Vector2(0, 0), new SharpDX.Vector2(DepthBuffer.Width, DepthBuffer.Height));

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
        }
    }
}
