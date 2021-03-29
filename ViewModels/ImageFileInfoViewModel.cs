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
            observableCollection.Add(new ImageFileInfo("NAme1",new Windows.UI.Xaml.Media.Imaging.BitmapImage(new Uri(@"Assets\backlit-photography-10.jpg")),"affsf"));
            observableCollection.Add(new ImageFileInfo("Some Pict", new Windows.UI.Xaml.Media.Imaging.BitmapImage(new Uri(@"Assets\rule_of_thirds_landscape_2048x2048.jpg")), "Pict 2 Title"));
            observableCollection.Add(new ImageFileInfo("Another One", new Windows.UI.Xaml.Media.Imaging.BitmapImage(new Uri(@"Assets\stock-photo-104292369.jpg")), "Picture Title"));
        }
    }
}
