using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.FileProperties;
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
			using (IRandomAccessStream fileStream = await file?.OpenReadAsync())
			{

				/*  StorageItemThumbnail thumbnail = await file.GetThumbnailAsync(Windows.Storage.FileProperties.ThumbnailMode.SingleItem, 184);*/

				BitmapImage thumbnailImage = await GetImageThumbnailAsync(file).ConfigureAwait(false);
				var properties = await file.Properties.GetImagePropertiesAsync();
				ImageFileInfo info = new ImageFileInfo(
					file.DisplayName, file, properties,
					 file.DisplayType, thumbnailImage);
				thumbnailImage = null;
				properties = null;
				return info;
			}
		}
		public static async Task<BitmapImage> GetImageSourceAsync(StorageFile ImageFile)
		{
			using (IRandomAccessStream fileStream = await ImageFile?.OpenReadAsync())
			{
				// Create a bitmap to be the image source.
				var imageSource = new BitmapImage();
				imageSource.SetSource(fileStream);

				return imageSource;
			}
		}

		//TODO: use it as property of ImageFileInfo to eliminate Thumbnail converter.
		/// <summary>
		/// Generate BitmapImage as thubnail. No in use yet.
		/// </summary>
		/// <param name="ImageFile"></param>
		/// <returns></returns>
		public static async Task<BitmapImage> GetImageThumbnailAsync(StorageFile ImageFile)
		{
			var thumbnail = await ImageFile?.GetThumbnailAsync(ThumbnailMode.SingleItem, 200, ThumbnailOptions.ResizeThumbnail);
			// Create a bitmap to be the image source.
			BitmapImage bitmapImage = new BitmapImage();
			bitmapImage.SetSource(thumbnail);
			thumbnail.Dispose();

			return bitmapImage;
		}
	}
}
