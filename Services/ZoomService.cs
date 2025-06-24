using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Textract.Services
{
    public class ZoomService
    {
        private readonly ScaleTransform ZoomTransform;
        public double Zoom { get; private set; } = 1.0;
        private const double ZoomStep = 0.1;
        private const double ZoomMin = 0.5;
        private const double ZoomMax = 5.0;

        public ZoomService(ScaleTransform transform)
        {
            ZoomTransform = transform;
            ApplyZoom();
        }

        public void ZoomIn()
        {
            Zoom = Math.Min(Zoom + ZoomStep, ZoomMax);
            ZoomTransform.ScaleX = Zoom;
            ZoomTransform.ScaleY = Zoom;
        }

        public void ZoomOut()
        {
            Zoom = Math.Max(Zoom - ZoomStep, ZoomMin);
            ZoomTransform.ScaleX = Zoom;
            ZoomTransform.ScaleY = Zoom;
        }

        public void ResetZoom()
        {
            Zoom = 1.0;
            ZoomTransform.ScaleX = Zoom;
            ZoomTransform.ScaleY = Zoom;
        }

        private void ApplyZoom()
        {
            ZoomTransform.ScaleX = Zoom;
            ZoomTransform.ScaleY = Zoom;
        }
    }
}
