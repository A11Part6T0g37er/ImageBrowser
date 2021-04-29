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
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
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
        }

        // TODO: making resisable layout, not works yet
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

            base.OnNavigatedTo(e);
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
    }
}