using Microsoft.Win32;
using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
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
using Textract.Services;
using static System.Net.Mime.MediaTypeNames;

namespace Textract
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : System.Windows.Window
    {
        private string LANGUAGE = "jpn+eng";

        private System.Windows.Point _startPoint;
        private bool _isDragging = false;

        public MainWindow()
        {
            InitializeComponent();
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

            Debug.WriteLine($"Image: {source.PixelWidth}x{source.PixelHeight}, Rect: {rectX},{rectY},{rectW},{rectH}");

            return new Int32Rect(rectX, rectY, rectW, rectH);
        }

        private void RunOcr_Click(object sender, RoutedEventArgs e)
        {
            if (MainImage.Source is not BitmapImage source) return;

            // 선택 영역 -> 이미지 좌표 변환
            var selection = GetSelectedImageRegion(source);
            if (selection.Width <= 0 || selection.Height <= 0) return;

            // 선택 영역만 잘라내기
            var cropped = new CroppedBitmap(source, selection);

            // OCR
            using var ocrService = new OcrService(@"./tessdata");
            string text = ocrService.OCRProcess(cropped).Trim();

            if (LANGUAGE.Contains("jpn"))
            {
                text = text.Replace(" ", "");
            }

            OcrResultTxtBox.AppendText(text+"\r\n");
        }

        private void LoadImage_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog
            {
                Title = "이미지 선택",
                Filter = "Image Files (*.png;*jpg;*.jpeg;*bmp)|*.png;*jpg;*.jpeg;*bmp"
            };

            if (ofd.ShowDialog() == true)
            {
                MainImage.Source = new BitmapImage(new Uri(ofd.FileName));

                // 선택 영역 초기화
                SelectionRect.Visibility = Visibility.Collapsed;
                SelectionRect.Width = 0;
                SelectionRect.Height = 0;
            }
        }

        private void OverlayCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _startPoint = e.GetPosition(OverlayCanvas);
            _isDragging = true;
            SelectionRect.Visibility = Visibility.Visible;
        }

        private void OverlayCanvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _isDragging = false;
        }

        private void OverlayCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (!_isDragging) return;

            var pos = e.GetPosition(OverlayCanvas);

            double x = Math.Min(pos.X, _startPoint.X);
            double y = Math.Min(pos.Y, _startPoint.Y);
            double w = Math.Abs(pos.X - _startPoint.X);
            double h = Math.Abs(pos.Y - _startPoint.Y);

            Canvas.SetLeft(SelectionRect, x);
            Canvas.SetTop(SelectionRect, y);
            SelectionRect.Width = w;
            SelectionRect.Height = h;
        }
    }
}