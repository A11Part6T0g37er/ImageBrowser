using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.Storage.FileProperties;
using Windows.UI.Xaml.Media.Imaging;

namespace ImageBrowser
{
    internal class ImageFileInfo : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public string ImageName { get; } = "DEBUG";

        //public ImageProperties ImageProperties { get; }

        private BitmapImage _imageSource = null;

        public ImageFileInfo()
        {
        }

        public ImageFileInfo(string imageName,/* ImageProperties imageProperties,*/ BitmapImage imageSource, string imageTitle)
        {
            ImageName = imageName;
            //ImageProperties = imageProperties;
            ImageSource = imageSource;
            ImageTitle = imageTitle;
        }

        public BitmapImage ImageSource { get => _imageSource; set => SetProperty(ref _imageSource, value); }

        public string ImageTitle
        {
            /*   get => String.IsNullOrEmpty(ImageProperties.Title) ? ImageName : ImageProperties.Title;
               set
               {
                   if (ImageProperties.Title != value)
                   {
                       ImageProperties.Title = value;
                       var ignoreResult = ImageProperties.SavePropertiesAsync();
                       OnPropertyChanged();
                   }
               }*/

            get;set;
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
