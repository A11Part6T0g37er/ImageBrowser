﻿using ImageBrowser.ViewModels;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Windows.Storage;

namespace ImageBrowser.Models
{
    class FolderInfoModel : BindableBase
    {
        private string _displayName;
        private string _path;

        private Collection<StorageFolder> folderList;

        public string FolderDisplayName { get { return _displayName; } set { base.SetProperty(ref _displayName, value); } }
        public string FolderPath { get { return _path; } set { base.SetProperty(ref _path, value); } }

        public Collection<StorageFolder> FolderList { get => folderList; set => base.SetProperty(ref folderList, value); }
    }
}
