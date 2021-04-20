using ImageBrowser.Common;
using ImageBrowser.Helpers;
using ImageBrowser.ViewModels;
using Microsoft.Graph;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Search;
using Windows.UI.Core;
using Windows.UI.Popups;
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
        ImageFileInfo persistedItem;
        string defaultWinTheme = string.Empty;
        public FoldersViewModel FoldersToDisplay { get; set; }

        public MainPage()
        {
            InitializeComponent();
            Current = this;
            SizeChanged += CoreWindow_SizeChanged;
           
            NavigationCacheMode = NavigationCacheMode.Enabled;

          //  DataContext = new MSGraphQueriesHelper();

            var DefaultTheme = new Windows.UI.ViewManagement.UISettings();
            var uiTheme = DefaultTheme.GetColorValue(Windows.UI.ViewManagement.UIColorType.Background).ToString();
            if (uiTheme == "#FF000000")
            {
                defaultWinTheme = "Dark";
            }
            else if (uiTheme == "#FFFFFFFF")
            {
                defaultWinTheme = "Light";
            }
        }

        // TODO: making resisable layout
        private void CoreWindow_SizeChanged(object sender, SizeChangedEventArgs args)
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

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility =
                AppViewBackButtonVisibility.Collapsed;

            if (!imageFileInfoViewModel.HaveAnyItems())
            {
                // initialize blank state
                startingGreetingScreen.Visibility = Visibility.Visible;
                 
            }

            imageFileInfoViewModel.InitializeGroupingOfViewModel();

            base.OnNavigatedTo(e);
        }

        //TODO: make folders upload into app

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
            return await imageFileInfoViewModel.PopulateObservableCollectionOfImages(files);
        }

        // TODO: updating number of  <XAML> Pictures-in-grid columns
        private void GroupedGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var panel = (ItemsWrapGrid)GroupedGrid.ItemsPanelRoot;
            int gridColumnNumber = 3;

            //VisualState actual = VisualStateGroup.CurrentState;

            //switch (_actual.Name)
            //{
            //    case "medium":
            //        {
            //            gridColumnNumber = 2;
            //            break;
            //        }
            //    case "large":
            //        {
            //            gridColumnNumber = 3;
            //            break;
            //        }
            //    default:
            //        {
            //            gridColumnNumber = 1;
            //            break;
            //        }
            //}
            panel.ItemWidth = e.NewSize.Width / gridColumnNumber;
        }

        private void GroupedGrid_ItemClick(object sender, ItemClickEventArgs e)
        {
            persistedItem = e.ClickedItem as ImageFileInfo;
            Frame.Navigate(typeof(DetailPage), e.ClickedItem);
        }

        private void ItemImage_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            if (imageFileInfoViewModel.HaveAnyItems())
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
            using (var RefreshcompletingDeferral = args.GetDeferral())
            {
                ICollection<StorageFile> files = new Collection<StorageFile>();

                for (int i = 0; i < imageFileInfoViewModel.ObservableCollection.Count; i++)
                {

                    files.Add(imageFileInfoViewModel.ObservableCollection[i].ImageFile);
                }

                IReadOnlyCollection<StorageFile> filesReadOnly = (IReadOnlyCollection<StorageFile>)files;
                await imageFileInfoViewModel.PopulateObservableCollectionOfImages(filesReadOnly);
            }
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            RefreshArea.RequestRefresh();
        }

        private async void SigningOneDrive_ClickAsync(object sender, RoutedEventArgs e)
        {
            try
            {
                // Sign-in user using MSAL and obtain an access token for MS Graph
                GraphServiceClient graphClient = await MSGraphQueriesHelper.SignInAndInitializeGraphServiceClient();
                // Call the /me endpoint of Graph
                User graphUser = await graphClient.Me.Request().GetAsync();

                // Go back to the UI thread to make changes to the UI
                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    ResultText.Text = "Display Name: " + graphUser.UserPrincipalName + "\nid: " + graphUser.Id;

                    //SignOutButton.Visibility = Visibility.Visible;
                    OpenOneDrive.Visibility = Visibility.Visible;
                });
            }
            catch (MsalException msalEx)
            {
                await DisplayMessageAsync($"Error Acquiring Token:{System.Environment.NewLine}{msalEx}");
            }
            catch (Exception ex)
            {
                await DisplayMessageAsync($"Error Acquiring Token Silently:{System.Environment.NewLine}{ex}");
                return;
            }
        }

        /// <summary>
        /// Displays a message in the ResultText. Can be called from any thread.
        /// </summary>
        private async Task DisplayMessageAsync(string message)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal,
                   () =>
                   {
                       new MessageDialog(message);
                   });
        }

        /// <summary>
        /// Sign out the current user.
        /// </summary>
        private async void SignOutButton_ClickAsync(object sender, RoutedEventArgs e)
        {
            IEnumerable<IAccount> accounts = await MSGraphQueriesHelper.GetMSGraphAccouts();
            if (accounts == null)
                return;
            IAccount firstAccount = accounts.FirstOrDefault();

            try
            {
                await MSGraphQueriesHelper.SingOutMSGraphAccount(firstAccount).ConfigureAwait(false);
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    ResultText.Text = "User has signed-out";
                    signingOneDrive.Visibility = Visibility.Visible;
                    OpenOneDrive.Visibility = /*SignOutButton.Visibility =*/ Visibility.Collapsed;
                    OneDriveInfo.Text = "";
                    imageFileInfoViewModel.FlushObservableCollectionOfImages();
                });
            }
            catch (MsalException ex)
            {
                ResultText.Text = $"Error signing-out user: {ex.Message}";
            }
        }

        private async void OpenOneDrive_Click(object sender, RoutedEventArgs e)
        {

            List<StorageFile> downloadedFiles = await MSGraphQueriesHelper.DownloadAllFilesFromOneDrive();

            if (Windows.UI.Core.CoreWindow.GetForCurrentThread() != null)
            {
                var resourceLoader = Windows.ApplicationModel.Resources.ResourceLoader.GetForCurrentView();

                OneDriveInfo.Text = resourceLoader.GetString("CountFiles/Text").ToString() + MSGraphQueriesHelper.CountFiles();
            }

            await imageFileInfoViewModel.PopulateObservableCollectionOfImages(downloadedFiles);
        }

        private void ThemeButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedTheme = ((Button)sender)?.Tag?.ToString();
            DefineClickedTheme(sender, selectedTheme);
        }

        /// <summary>
        /// Imlemented switching between <see cref="ElementTheme"/> .
        /// </summary>
        /// <param name="sender">Button.</param>
        /// <param name="selectedTheme">Button`s <see cref="string"/> tag property.</param>
        private void DefineClickedTheme(object sender, string selectedTheme)
        {
            if (selectedTheme != null)
            {
                if (selectedTheme == "Default")
                {
                    ((sender as Button).XamlRoot.Content as Frame).RequestedTheme = EnumHelper.GetEnum<ElementTheme>(defaultWinTheme);
                }
                else
                {
                    ((sender as Button).XamlRoot.Content as Frame).RequestedTheme = EnumHelper.GetEnum<ElementTheme>(selectedTheme);
                }
            }
        }

        private async void OpenFolders_Click(object sender, RoutedEventArgs e)
        {
            FolderPicker folderPicker = new FolderPicker();
            folderPicker.SuggestedStartLocation = PickerLocationId.Downloads;
            folderPicker.ViewMode = PickerViewMode.Thumbnail;
            folderPicker.FileTypeFilter.Add("*");
            StorageFolder folder = await folderPicker.PickSingleFolderAsync();
            if (folder != null)
            {
                FoldersToDisplay = new FoldersViewModel(folder);
            }
        }
    }
}