using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace MazeEditor
{
    public class MyRectangle
    {
        private Rectangle _rectangle;

        public double Width { get { return _rectangle.Width; } set { _rectangle.Width = value; } }
        public double Height { get { return _rectangle.Height; } set { _rectangle.Height = value; } }
        public int PositionX { get; set; }
        public int PositionY { get; set; }

        public MyRectangle()
        {
            var randomizer = new Random();

            _rectangle = new Rectangle();

            _rectangle.Fill = new SolidColorBrush(
                                Color.FromRgb(
                                      (byte)randomizer.Next(0, 255),
                                      (byte)randomizer.Next(0, 255),
                                      (byte)randomizer.Next(0, 255)));
        }

        public void ChangeFill(Brush fill)
        {
            _rectangle.Fill = fill;
        }

        public void ChangeStroke(Brush stroke)
        {
            _rectangle.Stroke = stroke;
        }

        public void AddRectangleToCanvas(Canvas canvas)
        {
            if (canvas == null)
                throw new ArgumentNullException(nameof(canvas));

            canvas.Children.Add(_rectangle);
            Canvas.SetLeft(_rectangle, PositionX);
            Canvas.SetTop(_rectangle, PositionY);
        }

        public bool Overlaps(MyRectangle r2)
        {
            var r1 = this;

            return !(r1.PositionX > r2.PositionX + r2.Width) &&
                    !(r1.PositionX + r1.Width < r2.PositionX) &&
                    !(r1.PositionY > r2.PositionY + r2.Height) &&
                    !(r1.PositionY + r1.Height < r2.PositionY);
        }

        public Tuple<int, int> MidPoint()
        {
            int midX, midY;

            midX = PositionX + (int)(Width / 2);

            midY = PositionY + (int)(Height / 2);

            return new Tuple<int, int>(midX, midY);
        }

        public double Distance(MyRectangle rect1)
        {
            Tuple<int, int> midPointStart = MidPoint();
            Tuple<int, int> midPointEnd = rect1.MidPoint();

            return Math.Sqrt(
                        Math.Pow(midPointStart.Item1 - midPointEnd.Item1, 2) +
                        Math.Pow(midPointStart.Item2 - midPointEnd.Item2, 2)
                        );
        }

    }
}
