using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace ImageBrowser.Helpers
{
   public static class ImageDownloadHelper
    {
        /// <summary>
        /// Get images from Web
        /// </summary>
        /// <param name="url">Web link</param>
        /// <param name="fileName">Unique filename</param>
        /// <returns>Returns Uri path for FileStorage</returns>
        public static async Task<String> DownloadImage(string url, String fileName)
        {
            const String imagesSubdirectory = "DownloadedImages";
            var rootFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync(imagesSubdirectory, CreationCollisionOption.OpenIfExists);

            var storageFile = await rootFolder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);

            using (HttpClient client = new HttpClient())
            {
                byte[] buffer = await client.GetByteArrayAsync(url);
                using (Stream stream = await storageFile.OpenStreamForWriteAsync())
                    stream.Write(buffer, 0, buffer.Length);
            }

            // Use this path to load image
            String newPath = String.Format("ms-appdata:///local/{0}/{1}", imagesSubdirectory, fileName);

            return newPath;
        }
    }
}
