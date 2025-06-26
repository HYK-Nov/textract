using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace Textract.Models
{
    public class ImageItem:INotifyPropertyChanged
    {
        public string Path { get; set; }
        public string Name { get; set; }
        private BitmapImage _thumbnail;

        public BitmapImage Thumbnail
        {
            get => _thumbnail;
            set
            {
                _thumbnail = value;
                OnPropertyChanged(nameof(Thumbnail));
            }
        }

        public ImageItem(string path, int index)
        {
            Path = path;
            Name = $"{index}";
            LoadThumbnailAsync();
        }

        private async void LoadThumbnailAsync()
        {
            await Task.Run(() =>
            {
                var thumb = new BitmapImage();
                thumb.BeginInit();
                thumb.UriSource = new Uri(Path);
                thumb.DecodePixelWidth = 100;
                thumb.CacheOption = BitmapCacheOption.OnLoad;
                thumb.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
                thumb.EndInit();
                thumb.Freeze(); // UI 요소 사용하기 위함

                Application.Current.Dispatcher.Invoke(() =>
                {
                    Thumbnail = thumb;
                });
            });
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
