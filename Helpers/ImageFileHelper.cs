using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace ImageBrowser.Helpers
{
  public static  class ImageFileHelper
    {
        public static async Task<ImageFileInfo> LoadImageInfo(StorageFile file)
        {
            var properties = await file.Properties.GetImagePropertiesAsync();
            ImageFileInfo info = new ImageFileInfo(
                file.DisplayName, file, properties,
                 file.DisplayType);
            await info.GetImageSourceAsync();
            return info;
        }
    }
}
