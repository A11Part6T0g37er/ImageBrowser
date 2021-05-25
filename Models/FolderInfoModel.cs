using ImageBrowser.ViewModels;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Windows.Storage;

namespace ImageBrowser.Models
{
  public  class FolderInfoModel : BindableBase
    {
        private string displayName;
        private string path;

        private IReadOnlyList<StorageFolder> folderList;

        public string FolderDisplayName { get { return displayName; } set { base.SetProperty(ref displayName, value); } }
        public string FolderPath { get { return path; } set { base.SetProperty(ref path, value); } }

        public IReadOnlyList<StorageFolder> FolderList { get => folderList; set => base.SetProperty(ref folderList,value); }
    }
}
