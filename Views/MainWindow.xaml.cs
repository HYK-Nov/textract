using Microsoft.Win32;
using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using System.Collections.ObjectModel;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Xps.Packaging;
using Tesseract;
using Textract.Models;
using Textract.Services;
using static System.Net.Mime.MediaTypeNames;

namespace Textract
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : System.Windows.Window
    {
        private System.Windows.Point _startPoint;
        private bool _isDragging;
        private readonly ZoomService _zoomService;
        private SelectionService _selectionService;
        private readonly OcrService ocrService = new OcrService(@"./tessdata");

        public static RoutedUICommand LoadImageCommand = new RoutedUICommand(
            "Load Image", "LoadImage", typeof(MainWindow));

        public MainWindow()
        {
            InitializeComponent();
            _isDragging = false;

            _zoomService = new ZoomService(ZoomTransform);
            _selectionService = new SelectionService(SelectionRect, _zoomService.Zoom);
        }

        private Int32Rect GetSelectedImageRegion(BitmapSource source)
        {
            // 실제 이미지가 Canvas에 그려진 위치 계산
            double imageWidth = MainImage.ActualWidth;
            double imageHeight = MainImage.ActualHeight;
            double canvasWidth = OverlayCanvas.ActualWidth;
            double canvasHeight = OverlayCanvas.ActualHeight;

            // 이미지가 캔버스 안에서 차지하는 여백
            double offsetX = (canvasWidth - imageWidth) / 2;
            double offsetY = (canvasHeight - imageHeight) / 2;

            // SelectionRect의 실제 좌표
            double selX = Canvas.GetLeft(SelectionRect);
            double selY = Canvas.GetTop(SelectionRect);
            double selW = SelectionRect.Width;
            double selH = SelectionRect.Height;

            // 이미지 바깥 드래그 막기
            double interLeft = Math.Max(selX, offsetX);
            double interTop = Math.Max(selY, offsetY);
            double interRight = Math.Min(selX + selW, offsetX + imageWidth);
            double interBottom = Math.Min(selY + selH, offsetY + imageHeight);

            double clippedW = interRight - interLeft;
            double clippedH = interBottom - interTop;

            if (clippedW <= 0 || clippedH <= 0)
                return new Int32Rect(0, 0, 1, 1); // 최소값 보장

            // 비율로 실제 픽셀 좌표 계산
            double scaleX = source.PixelWidth / imageWidth;
            double scaleY = source.PixelHeight / imageHeight;

            int rectX = (int)((interLeft - offsetX) * scaleX);
            int rectY = (int)((interTop - offsetY) * scaleY);
            int rectW = (int)(clippedW * scaleX);
            int rectH = (int)(clippedH * scaleY);

            // 범위 보정
            if (rectX + rectW > source.PixelWidth) rectW = source.PixelWidth - rectX;
            if (rectY + rectH > source.PixelHeight) rectH = source.PixelHeight - rectY;
            if (rectW <= 0) rectW = 1;
            if (rectH <= 0) rectH = 1;

            return new Int32Rect(rectX, rectY, rectW, rectH);
        }

        private void RunOcr_Click(object sender, RoutedEventArgs e)
        {
            if (MainImage.Source is not BitmapImage source) return;

            // 선택 영역 -> 이미지 좌표 변환
            var selection = GetSelectedImageRegion(source);
            if (selection.Width <= 0 || selection.Height <= 0) return;

            OcrProgressBar.IsIndeterminate = true;

            // 선택 영역만 잘라내기
            var cropped = new CroppedBitmap(source, selection);

            // OCR
            
            string text = ocrService.OCRProcess(cropped);

            OCRResultDataGrid.Items.Add(new OcrResult{Text = text});

            OcrProgressBar.IsIndeterminate = false;
        }

        private void FitImageToScrollViewer()
        {
            if (MainImage.Source is not BitmapSource bmp) return;

            double imageWidth = bmp.PixelWidth;
            double imageHeight = bmp.PixelHeight;

            double viewportWidth = ImageScrollViewer.ViewportWidth;
            double viewportHeight = ImageScrollViewer.ViewportHeight;

            // 스크롤뷰어와 이미지의 비율 비교
            double widthRatio = viewportWidth / imageWidth;
            double heightRatio = viewportHeight / imageHeight;

            double scale = Math.Min(widthRatio, heightRatio); // 더 작은 쪽 기준으로 맞추기

            // 실제 적용할 너비/높이 계산
            MainImage.Width = imageWidth * scale;
            MainImage.Height = imageHeight * scale;
        }

        private void LoadImage_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog
            {
                Title = "이미지 선택",
                Filter = "Image Files (*.png;*jpg;*.jpeg;*bmp)|*.png;*jpg;*.jpeg;*bmp",
                Multiselect = true
            };

            if (ofd.ShowDialog() == true)
            {
                var items = new List<ImageItem>();

                int index = 1;

                foreach (var path in ofd.FileNames)
                {
                    items.Add(new ImageItem(path, index++));
                }

                ImageListBox.ItemsSource = items;

                MainImage.Source = new BitmapImage(new Uri(items[0].Path));
                FitImageToScrollViewer();

                // 선택 영역 초기화
                _selectionService.Reset();

                // Zoom 초기화
                _zoomService.ResetZoom();
            }
        }

        private void LoadImage_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            LoadImage_Click(sender, e); // 기존 이미지 불러오기 재사용
        }

        private void OverlayCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _isDragging = true;

            var pos = e.GetPosition(OverlayCanvas);
            _selectionService.Start(pos);
        }

        private void OverlayCanvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _isDragging = false;
        }

        private void OverlayCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (!_isDragging) return;

            var pos = e.GetPosition(OverlayCanvas);
            _selectionService.Update(pos);
        }

        private void ImageListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ImageListBox.SelectedItem is ImageItem item)
            {
                var image = new BitmapImage(new Uri(item.Path));
                MainImage.Source = image;
                FitImageToScrollViewer();

                // 선택 영역 초기화
                _selectionService.Reset();

                // Zoom 초기화
                _zoomService.ResetZoom();
            }
        }

        private void LanguageMenuItem_Checked(object sender, RoutedEventArgs e)
        {
            var clicked = (MenuItem)sender;
            var parent = (MenuItem)clicked.Parent;

            foreach (object obj in parent.Items)
            {
                if (obj is MenuItem mi && mi != clicked)
                {
                    mi.IsChecked = false;
                }
            }

            if (clicked.Tag is string lang)
            {
                ocrService.ChangeLanguage(lang);
            }
        }

        private void OverlayCanvas_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                if (e.Delta > 0) _zoomService.ZoomIn();
                else _zoomService.ZoomOut();

                e.Handled = true;   // 스크롤 막고 줌만 실행
            }
            else
            {
                e.Handled = false;  // Ctrl 없으면 스크롤 동작 유지
            }
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }

        private void OCRResultDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (OCRResultDataGrid.SelectedItem is OcrResult selected)
            {
                Clipboard.SetText(selected.Text);
                MessageBox.Show("텍스트가 복사되었습니다.");
            }
        }
    }
}