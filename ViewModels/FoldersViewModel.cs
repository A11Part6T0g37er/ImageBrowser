using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace ImageBrowser.ViewModels
{
    public class FoldersViewModel
    {
        public ObservableCollection<StorageFolder> FoldersToDisplay { get; set; }

        public FoldersViewModel(StorageFolder storageFolder)
        {
            FoldersToDisplay = new ObservableCollection<StorageFolder>();
            FoldersToDisplay.Add(storageFolder);
        }
    }
}
