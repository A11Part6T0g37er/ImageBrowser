using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Storage;
using Windows.Storage.Search;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace ImageBrowser
{

    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public static MainPage Current;


        internal ImageBrowser.ViewModels.ImageFileInfoViewModel imageFileInfoViewModel = new ViewModels.ImageFileInfoViewModel(); 
       // internal ObservableCollection<ImageFileInfo> Images { get; set; } = new ObservableCollection<ImageFileInfo>();
        private ImageFileInfo persistedItem;

        public MainPage()
        {
            InitializeComponent();
            Current = this;
 //imageFileInfoViewModel= new ViewModels.ImageFileInfoViewModel();
       
            SizeChanged += CoreWindow_SizeChanged;
            DataContext = imageFileInfoViewModel.ObservableCollection;
            this.NavigationCacheMode = NavigationCacheMode.Enabled;
            
        }

        

        private  void CoreWindow_SizeChanged(object sender, SizeChangedEventArgs args)
        {
            var appView = ApplicationView.GetForCurrentView();
            if (args.NewSize.Width > 1008)
            {
                VisualStateManager.GoToState(this, "LargeWindowBreakpoint", true);
            }

            if (args.NewSize.Width < 1008 && args.NewSize.Width > 641)
            {
                VisualStateManager.GoToState(this, "MediumWindowBreakpoint", true);
            }
            if (args.NewSize.Width < 641)
            {
                VisualStateManager.GoToState(this, "MinWindowBreakpoint", true);
            }

        }

        private void InitializeGroupingOfViewModel()

        {
          
          if(imageFileInfoViewModel.ObservableCollection.Count > 0)
            imageFileInfoViewModel.Initialize();

        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            App.TryGoBack();
        }

        

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility =
                AppViewBackButtonVisibility.Collapsed;

            if (imageFileInfoViewModel.ObservableCollection.Count == 0)
            {
                // initialize blank state
                startingGreetingScreen.Visibility = Visibility.Visible;
               // await GetItemsAsync();
            }
           

            InitializeGroupingOfViewModel();
           
            base.OnNavigatedTo(e);
        }

        //TODO: make folders upload into app

        // TODO: under heawy tefactoring it`s gone be eliminating
        #region deleting
        private async Task GetItemsAsync(string path = "Assets\\")
        {
            QueryOptions options = new QueryOptions();
            options.FolderDepth = FolderDepth.Deep;
            options.FileTypeFilter.Add(".jpg");
            options.FileTypeFilter.Add(".png");
            options.FileTypeFilter.Add(".gif");

            // Get the Pictures library. (Requires 'Pictures Library' capability.)

            //Windows.Storage.StorageFolder picturesFolder = Windows.Storage.KnownFolders.PicturesLibrary;

            // OR
            // Get the Sample pictures.
            StorageFolder appInstalledFolder = Package.Current.InstalledLocation;
            StorageFolder picturesFolder = await appInstalledFolder.GetFolderAsync(path);

            var result = picturesFolder.CreateFileQueryWithOptions(options);

            IReadOnlyList<StorageFile> imageFiles = await result.GetFilesAsync();
            bool unsupportedFilesFound = false;
            foreach (StorageFile file in imageFiles)
            {
                // Only files on the local computer are supported. 
                // Files on OneDrive or a network location are excluded.
                if (file.Provider.Id == "computer")
                {
                    // it`s gone be dissapeared
                    //Images.Add(await LoadImageInfo(file));
                }
                else
                {
                    unsupportedFilesFound = true;
                }
            }

            if (unsupportedFilesFound == true)
            {
                ContentDialog unsupportedFilesDialog = new ContentDialog
                {
                    Title = "Unsupported images found",
                    Content = "This sample app only supports images stored locally on the computer. We found files in your library that are stored in OneDrive or another network location. We didn't load those images.",
                    CloseButtonText = "Ok"
                };

                ContentDialogResult resultNotUsed = await unsupportedFilesDialog.ShowAsync();
            }
            InitializeGroupingOfViewModel();
        } 
        #endregion
        public static async Task<ImageFileInfo> LoadImageInfo(StorageFile file)
        {
            var properties = await file.Properties.GetImagePropertiesAsync();
            ImageFileInfo info = new ImageFileInfo(
                file.DisplayName, file, properties,
                 file.DisplayType);
            await info.GetImageSourceAsync();
            return info;
        }

        private async void ButtonOpen_Click(object sender, RoutedEventArgs e)
        {
            await PickMultiplePictures();
        }

        private async Task<ObservableCollection<ImageFileInfo>> PickMultiplePictures()
        {
            var picker = new Windows.Storage.Pickers.FileOpenPicker();
            picker.ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail;
            picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.Desktop;

            picker.FileTypeFilter.Add(".jpg");
            picker.FileTypeFilter.Add(".jpeg");
            picker.FileTypeFilter.Add(".png");
            IReadOnlyCollection<StorageFile> files = await picker.PickMultipleFilesAsync();
            return await PopulateObservableCollectionOfImages(files);
        }

        private async Task<ObservableCollection<ImageFileInfo>> PopulateObservableCollectionOfImages(IReadOnlyCollection<StorageFile> files)
        {
            if (files.Count <= 0)
            { return null; }
            else
            {

                imageFileInfoViewModel.ObservableCollection.Clear();
                imageFileInfoViewModel.GroupedImagesInfos.Clear();
                foreach (var file in files)
                {
                    ImageFileInfo item = await LoadImageInfo(file);


                    imageFileInfoViewModel.ObservableCollection.Add(item);
                }
            }
            InitializeGroupingOfViewModel();

            return null;
        }


        // TODO: making resisable layout
        private void Page_SizeChanged(object sender, WindowSizeChangedEventArgs e)
        {
            if (e.Size.Width > 1000)
                VisualStateManager.GoToState(this, "LargeWindowBreakpoint", false);
            else
                VisualStateManager.GoToState(this, "MinWindowBreakpoint", false);
        }
        // TODO: updating number of  <XAML> Pictures-in-grid columns
        private void GroupedGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var _panel = (ItemsWrapGrid) GroupedGrid.ItemsPanelRoot;
            int _gridColumnNumber = 3;

            //VisualState _actual = VisualStateGroup.CurrentState;
           
            //switch (_actual.Name)
            //{
            //    case "medium":
            //        {
            //            _gridColumnNumber = 2;
            //            break;
            //        }
            //    case "large":
            //        {
            //            _gridColumnNumber = 3;
            //            break;
            //        }
            //    default:
            //        {
            //            _gridColumnNumber = 1;
            //            break;
            //        }
            //}
            _panel.ItemWidth = e.NewSize.Width / _gridColumnNumber;
        }

        private void GroupedGrid_ItemClick(object sender, ItemClickEventArgs e)
        {
            persistedItem = e.ClickedItem as ImageFileInfo;
            this.Frame.Navigate(typeof(DetailPage), e.ClickedItem);
        }

        private void ItemImage_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            if (imageFileInfoViewModel.ObservableCollection.Count != 0)
            {
                startingGreetingScreen.Visibility = Visibility.Collapsed;

            }
            else
            {
                startingGreetingScreen.Visibility = Visibility.Visible;
            }
           
        }

        private async void RefreshArea_RefreshRequested(RefreshContainer sender, RefreshRequestedEventArgs args)
        {
            using(var RefreshcompletingDeferral = args.GetDeferral())
            {
                ICollection<StorageFile> files = new Collection<StorageFile>();

                for (int i = 0; i < imageFileInfoViewModel.ObservableCollection.Count; i++)
                {

                    files.Add(imageFileInfoViewModel.ObservableCollection[i].ImageFile);  
                }
                
                IReadOnlyCollection<StorageFile> filesReadOnly = (IReadOnlyCollection<StorageFile>)files;
                await  PopulateObservableCollectionOfImages(filesReadOnly);
            }
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            RefreshArea.RequestRefresh();
        }
    }

}
