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

		private ObservableCollection<FolderInfoModel> foldersPath = new ObservableCollection<FolderInfoModel>();
		public ObservableCollection<FolderInfoModel> FoldersPath { get { return foldersPath; } /*set { foldersPath = value; }*/ }

		private ObservableCollection<ImageFileInfo> pictCollection = new ObservableCollection<ImageFileInfo>();

		public ObservableCollection<ImageFileInfo> PictsFromFolders { get => pictCollection; }

		public void AddPicts(ObservableCollection<ImageFileInfo> images)
		{
			this.pictCollection = images;
		}
		public void ResetCollection()
		{
			this.FoldersPath.Clear();
			this.PictsFromFolders.Clear();
		}
	}
}
