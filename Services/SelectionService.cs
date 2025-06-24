using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Point = System.Windows.Point;
using Rectangle = System.Windows.Shapes.Rectangle;

namespace Textract.Services
{
    public class SelectionService
    {
        private Point _startPoint;
        private readonly System.Windows.Shapes.Rectangle _rect;
        private readonly double _zoom;

        public SelectionService(Rectangle selectionRect, double zoom)
        {
            _rect = selectionRect;
            _zoom = zoom;
        }

        public void Start(Point pos)
        {
            _startPoint = new System.Windows.Point(pos.X / _zoom, pos.Y / _zoom);
            _rect.Visibility = Visibility.Visible;
        }

        public void Update(Point current)
        {
            double x1 = _startPoint.X;
            double y1 = _startPoint.Y;
            double x2 = current.X / _zoom;
            double y2 = current.Y / _zoom;


            double x = Math.Min(x1, x2);
            double y = Math.Min(y1, y2);
            double w = Math.Abs(x2 - x1);
            double h = Math.Abs(y2 - y1);

            Canvas.SetLeft(_rect, x * _zoom);
            Canvas.SetTop(_rect, y * _zoom);
            _rect.Width = w * _zoom;
            _rect.Height = h * _zoom;
        }

        public void Stop()
        {
            // TODO: 선택 해제 기능
        }
    }
}
