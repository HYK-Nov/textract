using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Textract.Models;

namespace Textract.Services
{
    public class ImageViewerService
    {
        private readonly Image _mainImage;
        private readonly ScrollViewer _imageScrollViewer;
        private readonly SelectionService _selectionService;
        private readonly ZoomService _zoomService;

        public ImageViewerService(Image mainImage, ScrollViewer imageScrollViewer, SelectionService selectionService, ZoomService zoomService)
        {
            _mainImage = mainImage;
            _imageScrollViewer = imageScrollViewer;
            _selectionService = selectionService;
            _zoomService = zoomService;
        }

        public void Load(ImageItem item)
        {
            if (item == null) return;

            var image = new BitmapImage(new Uri(item.Path));
            _mainImage.Source = image;
            FitImageToScrollViewer();

            // 선택 영역 초기화
            _selectionService.Reset();

            // Zoom 초기화
            _zoomService.ResetZoom();
        }

        public void FitImageToScrollViewer()
        {
            if (_mainImage.Source is not BitmapSource bmp) return;

            double imageWidth = bmp.PixelWidth;
            double imageHeight = bmp.PixelHeight;

            double viewportWidth = _imageScrollViewer.ViewportWidth;
            double viewportHeight = _imageScrollViewer.ViewportHeight;

            // 스크롤뷰어와 이미지의 비율 비교
            double widthRatio = viewportWidth / imageWidth;
            double heightRatio = viewportHeight / imageHeight;

            double scale = Math.Min(widthRatio, heightRatio); // 더 작은 쪽 기준으로 맞추기

            // 실제 적용할 너비/높이 계산
            _mainImage.Width = imageWidth * scale;
            _mainImage.Height = imageHeight * scale;
        }
    }
}
