using ImageBrowser.ViewModels;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace ImageBrowser
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            InitializeComponent();
            ViewModel = new ImageFileInfoViewModel();
            imageFile = new ImageFileInfo();
        }
        internal ImageFileInfoViewModel ViewModel { get; set; }
        internal ImageFileInfo imageFile { get; set; }
        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            App.TryGoBack();
        }

        private void PicturesInGrid_ItemClick(object sender, ItemClickEventArgs e)
        {

        }
    }

}
