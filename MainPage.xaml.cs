﻿using ImageBrowser.Common;
using Microsoft.Graph;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Windows.Storage;
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
        #region MSGraphAPI

        // The MSAL Public client app
        public static IPublicClientApplication PublicClientApp;
        private const string ClientId = "c6e3c937-e10d-4e7c-94d7-bbaaafc514aa";
        private string[] scopes = new string[] { "user.read" };
        private const string Tenant = "consumers";
        private const string Authority = "https://login.microsoftonline.com/" + Tenant;
        private static string MSGraphURL = "https://graph.microsoft.com/v1.0/";
        private static AuthenticationResult authResult;

        #endregion

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
            NavigationCacheMode = NavigationCacheMode.Enabled;


        }



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

        private void InitializeGroupingOfViewModel()

        {

            if (imageFileInfoViewModel.ObservableCollection.Count > 0)
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
            StorageFolder appInstalledFolder = Windows.ApplicationModel.Package.Current.InstalledLocation;
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
            var _panel = (ItemsWrapGrid)GroupedGrid.ItemsPanelRoot;
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
            Frame.Navigate(typeof(DetailPage), e.ClickedItem);
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
            using (var RefreshcompletingDeferral = args.GetDeferral())
            {
                ICollection<StorageFile> files = new Collection<StorageFile>();

                for (int i = 0; i < imageFileInfoViewModel.ObservableCollection.Count; i++)
                {

                    files.Add(imageFileInfoViewModel.ObservableCollection[i].ImageFile);
                }

                IReadOnlyCollection<StorageFile> filesReadOnly = (IReadOnlyCollection<StorageFile>)files;
                await PopulateObservableCollectionOfImages(filesReadOnly);
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
                GraphServiceClient graphClient = await SignInAndInitializeGraphServiceClient(scopes);

                // Call the /me endpoint of Graph
                User graphUser = await graphClient.Me.Request().GetAsync();
                               
                // Go back to the UI thread to make changes to the UI
                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    ResultText.Text = "Display Name: " + graphUser.UserPrincipalName + "\nid: " + graphUser.Id;

                    SignOutButton.Visibility = Visibility.Visible;
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
        /// Sign in user using MSAL and obtain a token for Microsoft Graph
        /// </summary>
        /// <returns>GraphServiceClient</returns>
        private static async Task<GraphServiceClient> SignInAndInitializeGraphServiceClient(string[] scopes)
        {
            GraphServiceClient graphClient = new GraphServiceClient(MSGraphURL,
                new DelegateAuthenticationProvider(async (requestMessage) =>
                {
                    requestMessage.Headers.Authorization = new AuthenticationHeaderValue("bearer", await SignInUserAndGetTokenUsingMSAL(scopes));
                }));

            return await Task.FromResult(graphClient);
        }

        /// <summary>
        /// Signs in the user and obtains an Access token for MS Graph
        /// </summary>
        /// <param name="scopes"></param>
        /// <returns> Access Token</returns>
        private static async Task<string> SignInUserAndGetTokenUsingMSAL(string[] scopes)
        {
            // Initialize the MSAL library by building a public client application
            PublicClientApp = PublicClientApplicationBuilder.Create(ClientId)
                .WithAuthority(Authority)
                .WithUseCorporateNetwork(false)
                .WithRedirectUri(DefaultRedirectUri.Value)
                 .WithLogging((level, message, containsPii) =>
                 {
                     Debug.WriteLine($"MSAL: {level} {message} ");
                 }, LogLevel.Warning, enablePiiLogging: false, enableDefaultPlatformLogging: true)
                .Build();

            // It's good practice to not do work on the UI thread, so use ConfigureAwait(false) whenever possible.
            IEnumerable<IAccount> accounts = await PublicClientApp.GetAccountsAsync().ConfigureAwait(false);
            IAccount firstAccount = accounts.FirstOrDefault();

            try
            {
                authResult = await PublicClientApp.AcquireTokenSilent(scopes, firstAccount)
                                                  .ExecuteAsync();
            }
            catch (MsalUiRequiredException ex)
            {
                // A MsalUiRequiredException happened on AcquireTokenSilentAsync. This indicates you need to call AcquireTokenAsync to acquire a token
                Debug.WriteLine($"MsalUiRequiredException: {ex.Message}");

                authResult = await PublicClientApp.AcquireTokenInteractive(scopes)
                                                  .ExecuteAsync()
                                                  .ConfigureAwait(false);

            }
            return authResult.AccessToken;
        }

        /// <summary>
        /// Sign out the current user
        /// </summary>
        private async void SignOutButton_ClickAsync(object sender, RoutedEventArgs e)
        {
            IEnumerable<IAccount> accounts = await PublicClientApp.GetAccountsAsync().ConfigureAwait(false);
            IAccount firstAccount = accounts.FirstOrDefault();

            try
            {
                await PublicClientApp.RemoveAsync(firstAccount).ConfigureAwait(false);
                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    ResultText.Text = "User has signed-out";
                    signingOneDrive.Visibility = Visibility.Visible;
                    SignOutButton.Visibility = Visibility.Collapsed;
                });
            }
            catch (MsalException ex)
            {
                ResultText.Text = $"Error signing-out user: {ex.Message}";
            }
        }

        private async void OpenOneDrive_Click(object sender, RoutedEventArgs e)
        {
            
            string[] scopeOneDrive = { "Files.Read"};
            GraphServiceClient graphService =   new GraphServiceClient(new DelegateAuthenticationProvider(async (requestMessage) => {

                IEnumerable<IAccount> accounts = await PublicClientApp.GetAccountsAsync().ConfigureAwait(false);
                IAccount firstAccount = accounts.FirstOrDefault();


                // Retrieve an access token for Microsoft Graph (gets a fresh token if needed).
                var authResult = await PublicClientApp.AcquireTokenSilent(scopeOneDrive, firstAccount)
                                                   .ExecuteAsync();

                // Add the access token in the Authorization header of the API request.
                requestMessage.Headers.Authorization =
                    new AuthenticationHeaderValue("Bearer", authResult.AccessToken);
            })
    );

          //  api://c6e3c937-e10d-4e7c-94d7-bbaaafc514aa/Files.Read

            // THAT!S IT
            var children = await graphService.Me.Drive.Root.Children
                .Request()
                .GetAsync();
            OneDriveInfo.Text = children.Count.ToString();


            // The Azure AD tenant ID or a verified domain (e.g. contoso.onmicrosoft.com) 
            var tenantId = "{tenant-id-or-domain-name}";

            // The client ID of the app registered in Azure AD
            var clientId = "{client-id}";

            // *Never* include client secrets in source code!
            var clientSecret = await GetClientSecretFromKeyVault(); // Or some other secure place.

            // The app registration should be configured to require access to permissions
            // sufficient for the Microsoft Graph API calls the app will be making, and
            // those permissions should be granted by a tenant administrator.
            var scopes = new string[] { "https://graph.microsoft.com/.default" };

            // Configure the MSAL client as a confidential client
            var confidentialClient = ConfidentialClientApplicationBuilder
                .Create(clientId)
                .WithAuthority($"https://login.microsoftonline.com/$tenantId/v2.0")
                .WithClientSecret(clientSecret)
                .Build();

            // Build the Microsoft Graph client. As the authentication provider, set an async lambda
            // which uses the MSAL client to obtain an app-only access token to Microsoft Graph,
            // and inserts this access token in the Authorization header of each API request. 
            GraphServiceClient graphServiceClient =
                new GraphServiceClient(new DelegateAuthenticationProvider(async (requestMessage) => {

        // Retrieve an access token for Microsoft Graph (gets a fresh token if needed).
        var authResult = await confidentialClient
            .AcquireTokenForClient(scopes)
            .ExecuteAsync();

        // Add the access token in the Authorization header of the API request.
        requestMessage.Headers.Authorization =
            new AuthenticationHeaderValue("Bearer", authResult.AccessToken);
                })
                );

            // Make a Microsoft Graph API query
            var users = await graphServiceClient.Users.Request().GetAsync();

        }

       
    }
}