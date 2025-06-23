using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;

namespace Textract
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Point _startPoint;
        private bool _isDragging = false;

        public MainWindow()
        {
            InitializeComponent();
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

        private void RunOcr_Click(object sender, RoutedEventArgs e)
        {

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