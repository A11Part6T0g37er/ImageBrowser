using ImageBrowser.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageBrowser.ViewModels
{
    class FoldersItemsCollection
    {
        private ObservableCollection<FolderInfoModel> _foldersPath = new ObservableCollection<FolderInfoModel>();
        public ObservableCollection<FolderInfoModel> foldersPath { get { return _foldersPath; } set { _foldersPath = value; } }

        private ObservableCollection<ImageFileInfo> pictCollection = new ObservableCollection<ImageFileInfo>();

        public IList<ImageFileInfo> PictsFromFolders { get => pictCollection; }

        public void AddPicts(ObservableCollection<ImageFileInfo> images)
        {
            this.pictCollection =images;
        }
    }
}
