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
        private ObservableCollection<ImageFileInfo> observableCollection = new ObservableCollection<ImageFileInfo>();
        public ImageFileInfo DefaultImageFileInfo { get { return this.defaultImageInfo; }  }

        public ObservableCollection<ImageFileInfo> ObservableCollection { get => observableCollection;  }

        public ImageFileInfoViewModel()
        {
            observableCollection.Add(new ImageFileInfo("NAme1",null,"affsf"));
            observableCollection.Add(new ImageFileInfo("Some Pict", null, "Pict 2 Title"));
            observableCollection.Add(new ImageFileInfo("Another One", null, "Picture Title"));
        }
    }
}
