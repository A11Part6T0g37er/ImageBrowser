using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;

namespace ImageBrowser.Helpers
{
    public static class ImageFileHelper
    {
        public static async Task<ImageFileInfo> LoadImageInfo(StorageFile file)
        {
            // Open a stream for the selected file.
            // The 'using' block ensures the stream is disposed
            // after the image is loaded.
            using (IRandomAccessStream fileStream = await file.OpenReadAsync())
            {
                // Create a bitmap to be the image source.
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.SetSource(fileStream);

                var properties = await file.Properties.GetImagePropertiesAsync();
                ImageFileInfo info = new ImageFileInfo(
                    file.DisplayName, file, properties,
                     file.DisplayType);
                await info.GetImageSourceAsync();
                return info;
            }
        }
    }
}
