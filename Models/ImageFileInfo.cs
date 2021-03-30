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

        public string ImageName { get; } = "DEBUG";

        public ImageProperties ImageProperties { get; }
        public StorageFile ImageFile { get; }

        public string ImageFileType { get; private set; }
        

        public ImageFileInfo()
        {
        }

        public ImageFileInfo(string imageName,StorageFile storageFile, ImageProperties imageProperties, string type)
        {
            ImageName = imageName;
            ImageProperties = imageProperties;
            
           
            ImageFileType = type;
            ImageFile = storageFile;
        }

        public string ImageDimensions => $"{ImageProperties.Width} x {ImageProperties.Height}";

        public string ImageTitle
        {
            get => String.IsNullOrEmpty(ImageProperties.Title) ? ImageName : ImageProperties.Title;
            set
            {
                if (ImageProperties.Title != value)
                {
                    ImageProperties.Title = value;
                    var ignoreResult = ImageProperties.SavePropertiesAsync();
                    OnPropertyChanged();
                }
            }

           
        }
        public async Task<BitmapImage> GetImageSourceAsync()
        {
            using (IRandomAccessStream fileStream = await ImageFile.OpenReadAsync())
            {
                // Create a bitmap to be the image source.
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.SetSource(fileStream);

                return bitmapImage;
            }
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
