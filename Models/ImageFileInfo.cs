using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;

namespace ImageBrowser
{
    public class ImageFileInfo : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public string ImageName { get; }

        public ImageProperties ImageProperties { get; }

        public StorageFile ImageFile { get; }

        private string imagePath;
        public string ImagePath { get /*{ return ImageFile.Path.ToString()  }*/; set; }
        private StorageItemThumbnail _thumbnail;
        public StorageItemThumbnail thumbnail
        {
            get { return _thumbnail; }
            set
            {
                SetProperty(ref _thumbnail, value);
            }
        }


        public string ImageFileType { get; private set; }

        private BitmapImage imageSource = null;

        public BitmapImage ImageSource
        {
            get => imageSource;
            set => SetProperty(ref imageSource, value);
        }

        public string ImageDimensions => $"{ImageProperties.Width} x {ImageProperties.Height}";

        private string imageTitle;

        public string ImageTitle
        {
            get => string.IsNullOrEmpty(imageTitle) ? ImageName : ImageProperties.Title;
            set
            {
                if (ImageProperties.Title != value)
                {
                    ImageProperties.Title = value;

                    SetProperty(ref imageTitle, value);
                }
            }
        }

        public ImageFileInfo()
        {
        }

        public ImageFileInfo(string imageName, StorageFile storageFile, ImageProperties imageProperties, string type, StorageItemThumbnail thumbnail)
        {
            ImageName = imageName;
            ImageProperties = imageProperties;
            this.thumbnail = thumbnail;
            ImageFileType = type;
            ImageFile = storageFile;
        }

        public async Task<BitmapImage> GetImageSourceAsync()
        {
            using (IRandomAccessStream fileStream = await ImageFile.OpenReadAsync())
            {
                // Create a bitmap to be the image source.
                var imageSource = new BitmapImage();
                imageSource.SetSource(fileStream);

                return imageSource;
            }
        }
        public async Task<BitmapImage> GetImageThumbnailAsync()
        {
            var thumbnail = await ImageFile.GetThumbnailAsync(ThumbnailMode.PicturesView);
            // Create a bitmap to be the image source.
            BitmapImage bitmapImage = new BitmapImage();
            bitmapImage.SetSource(thumbnail);
            thumbnail.Dispose();

            return bitmapImage;
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            if (object.Equals(storage, value))
            {
                return false;
            }
            else
            {
                storage = value;
                OnPropertyChanged(propertyName);
                return true;
            }
        }
    }
}
