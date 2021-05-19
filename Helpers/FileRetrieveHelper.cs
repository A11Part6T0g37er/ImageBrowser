using ImageBrowser.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace ImageBrowser.Helpers
{
    public static class FileRetrieveHelper
    {
        public static async Task<StorageFolder> GetParentFolder(FolderInfoModel folderItem)
        {
            StorageFolder outerFolder;
            if (folderItem.FolderList.Any())
            {

                outerFolder = await folderItem?.FolderList.First().GetParentAsync();
            }
            else
            {
                var currentFolder = await StorageFolder.GetFolderFromPathAsync(folderItem.FolderPath);
                outerFolder = currentFolder;
            }

            return outerFolder;
        }
    }
}
