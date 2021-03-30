using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageBrowser.ViewModels
{
   internal class ImageFileInfoViewModel
    {
        private ImageFileInfo defaultImageInfo = new ImageFileInfo();
        public ObservableCollection<ImageFileInfo> observableCollection = new ObservableCollection<ImageFileInfo>();
        public ImageFileInfo DefaultImageFileInfo { get { return this.defaultImageInfo; }  }

        public ObservableCollection<ImageFileInfo> ObservableCollection { get => observableCollection;  }

        public ImageFileInfoViewModel()
        {
            
        }
    }
}
