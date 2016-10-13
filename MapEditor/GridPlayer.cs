using SlimDX;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace MapEditor
{
    public class GridPlayer
    {
        private Canvas _canvas;
        private Point _currentPosition;
        private Line line1;
        private Line line2;
        private SolidColorBrush _lineBrush;

        public GridPlayer(Canvas canvas)
        {
            _lineBrush = new SolidColorBrush(Color.FromRgb(0, 128, 0));
            _canvas = canvas;
        }

        public void MovePlayer(Point newPosition)
        {
            _currentPosition = newPosition;

            _canvas.Children.Remove(line1);
            _canvas.Children.Remove(line2);

            line1 = new Line();
            line1.Stroke = _lineBrush;
            line1.X1 = newPosition.X - 5;
            line1.Y1 = newPosition.Y - 5;
            line1.X2 = newPosition.X + 5;
            line1.Y2 = newPosition.Y + 5;

            line2 = new Line();
            line2.Stroke = _lineBrush;
            line2.X1 = newPosition.X + 5;
            line2.Y1 = newPosition.Y - 5;
            line2.X2 = newPosition.X - 5;
            line2.Y2 = newPosition.Y + 5;

            _canvas.Children.Add(line1);
            _canvas.Children.Add(line2);
        }

        public Vector3 TranslateToRealSpace(float scaleFactor, float midWidth, float midHeight)
        {
            return new Vector3(
                    ((float)_currentPosition.X * scaleFactor) - midWidth
                    , 16.0f
                    , midHeight - ((float)_currentPosition.Y * scaleFactor));
        }
    }
}