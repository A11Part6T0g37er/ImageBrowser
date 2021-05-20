using ImageBrowser.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace ImageBrowser.ViewModels
{
    class FoldersItemsCollection
    {
        public StorageFolder CurentFolder { get; set; }

        private ObservableCollection<FolderInfoModel> _foldersPath = new ObservableCollection<FolderInfoModel>();
        public ObservableCollection<FolderInfoModel> foldersPath { get { return _foldersPath; }/* set { _foldersPath = value; }*/ }

        private ObservableCollection<ImageFileInfo> pictCollection = new ObservableCollection<ImageFileInfo>();

        public ObservableCollection<ImageFileInfo> PictsFromFolders { get => pictCollection; /*set => pictCollection = (ObservableCollection<ImageFileInfo>)value;*/ }

        public void AddPicts(ObservableCollection<ImageFileInfo> images)
        {
            this.pictCollection =images;
        }
        public void ResetCollection()
        {
            this.foldersPath.Clear();
            this.PictsFromFolders.Clear();
        }
    }
}
