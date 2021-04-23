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
            imageFileInfoViewModel = new ImageFileInfoViewModel();

            Current = this;
            SizeChanged += CoreWindow_SizeChanged;
            NavigationCacheMode = NavigationCacheMode.Enabled;

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
            //            gridColumnNumber = 3;
            //            break;
            //        }
            //    case "large":
            //        {
            //            gridColumnNumber = 4;
            //            break;
            //        }
            //    default:
            //        {
            //            gridColumnNumber = 2;
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

    }
}