using ImageBrowser.Common;
using ImageBrowser.ViewModels;
using Microsoft.Graph;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Storage;
using Windows.UI.Popups;
using Windows.UI.Xaml;

namespace ImageBrowser.Helpers
{
    public  class MSGraphQueriesHelper 
    {
        #region MSGraphAPI

        // The MSAL Public client app
        private const string ClientId = "c6e3c937-e10d-4e7c-94d7-bbaaafc514aa";
        private const string Tenant = "consumers";
        private const string Authority = "https://login.microsoftonline.com/" + Tenant;
        private static readonly string[] Scopes = new string[] { "user.read Files.Read" };
        private static readonly string MSGraphURL = "https://graph.microsoft.com/v1.0/";
        private static IPublicClientApplication publicClientApp;
        private static AuthenticationResult authResult;

        #endregion
        private static IDriveItemSearchCollectionPage search;
        public static bool UserSignedOut;
        public bool UserDefenitlySignedOut { get; set; }
        public static event PropertyChangedEventHandler PropertyChanged;


        /// <summary>
        /// Establish connection to OneDrive, get authentication, obtains links of files with query option, downloads files in temporary place.
        /// </summary>
        /// <returns>Returnsl List of files.</returns>
        public static async Task<List<StorageFile>> DownloadAllFilesFromOneDrive()
        {
            GraphServiceClient grSC = await SignInAndInitializeGraphServiceClient();

            search = await GetFilesByQuery(grSC);

            List<DriveItem> oneDriveItems = search.CurrentPage.Select(x => x).ToList();
            StorageFile storageFile;
            string newPath = string.Empty;
            List<StorageFile> downloadedFiles = new List<StorageFile>();

            foreach (var item in oneDriveItems)
            {
                var itemUrl = item.AdditionalData.Values.FirstOrDefault().ToString();
                var itemName = item.Name;
                newPath = await ImageDownloadHelper.DownloadImage(
                    itemUrl,
                    itemName);
                storageFile = await StorageFile.GetFileFromApplicationUriAsync(new Uri(newPath));
                downloadedFiles.Add(storageFile);
            }

            return downloadedFiles;
        }

       /* public MSGraphQueriesHelper()
        {
         //   SignOutStatus = new RelayCommand(Hello, IsSignedOut);
            UserSignedOut = false;
        }*/
     
        //public ICommand SignOutStatus { get; set; }

        public  bool IsSignedOut()
        {
            return UserSignedOut;
        }

        //public  bool IsUserSignedOut
        //{
        //    get
        //    {
        //        return UserSignedOut;
        //    }
        //    set
        //    {
        //       SetProperty(ref UserSignedOut, value);
        //    }
        //}

        //public void Hello()
        //{
        //    ;
        //}

        public static string CountFiles()
        {
            return GetFilesCount(search);
        }

        public static async Task<IEnumerable<IAccount>> GetMSGraphAccouts()
        {
            if (publicClientApp!=null)
            {
                return await publicClientApp.GetAccountsAsync().ConfigureAwait(false);

            }
            return null;
        }

        public static async Task SingOutMSGraphAccount(IAccount firstAccount)
        {
            SetProperty(ref UserSignedOut,false);
          //  UserSignedOut =false;
            await publicClientApp.RemoveAsync(firstAccount).ConfigureAwait(false);
        }

        /// <summary>
        /// Sign in user using MSAL and obtain a token for Microsoft Graph.
        /// </summary>
        /// <returns>GraphServiceClient.</returns>
        public static async Task<GraphServiceClient> SignInAndInitializeGraphServiceClient()
        {
            SetProperty(ref UserSignedOut, false);
            SetProperty(ref UserSignedOut, true);
           // UserSignedOut = true;
            GraphServiceClient graphClient = new GraphServiceClient(MSGraphURL,
                new DelegateAuthenticationProvider(async (requestMessage) =>
                {
                    await Task.Run(async () => requestMessage.Headers.Authorization = new AuthenticationHeaderValue("bearer", await SignInUserAndGetTokenUsingMSAL(Scopes)));
                }));
            return await Task.FromResult(graphClient);
        }

        /// <summary>
        /// Signs in the user and obtains an Access token for MS Graph.
        /// </summary>
        /// <param name="scopes">Used default const values.</param>
        /// <returns> Access Token.</returns>
        public static async Task<string> SignInUserAndGetTokenUsingMSAL(string[] scopes)
        {
            // Initialize the MSAL library by building a public client application
            publicClientApp = PublicClientApplicationBuilder.Create(ClientId)
                .WithAuthority(Authority)
                .WithUseCorporateNetwork(false)
                .WithRedirectUri(DefaultRedirectUri.Value)
                 .WithLogging(
                     (level, message, containsPii) =>
                     {
                         Debug.WriteLine($"MSAL: {level} {message} ");
                     }, LogLevel.Warning, enablePiiLogging: false, enableDefaultPlatformLogging: true)
                    .Build();

            // It's good practice to not do work on the UI thread, so use ConfigureAwait(false) whenever possible.
            IEnumerable<IAccount> accounts = await publicClientApp.GetAccountsAsync().ConfigureAwait(false);
            IAccount firstAccount = accounts.FirstOrDefault();

            try
            {
                authResult = await publicClientApp.AcquireTokenSilent(scopes, firstAccount)
                                                  .ExecuteAsync();
            }
            catch (MsalUiRequiredException ex)
            {
                // A MsalUiRequiredException happened on AcquireTokenSilentAsync. This indicates you need to call AcquireTokenAsync to acquire a token
                Debug.WriteLine($"MsalUiRequiredException: {ex.Message}");

                authResult = await publicClientApp.AcquireTokenInteractive(scopes)
                                                  .ExecuteAsync()
                                                  .ConfigureAwait(false);
            }

            return authResult.AccessToken;
        }

        private static async Task<IDriveItemSearchCollectionPage> GetFilesByQuery(GraphServiceClient grSC)
        {
            var queryOptions = new List<QueryOption>()
            {
                new QueryOption("select", "*")
            };

            var search = await grSC.Me.Drive.Root.ItemWithPath("/Pictures")
                .Search("jpg")
                .Request(queryOptions)
                .GetAsync();
            return search;
        }

        private static string GetFilesCount(IDriveItemSearchCollectionPage search)
        {
            return search.Count.ToString();
        }

        protected static void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(/*SigningStatusViewModel.StatusProperty*/ MSGraphQueriesHelper.UserSignedOut, new PropertyChangedEventArgs(propertyName));
        }

       protected static bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            if (object.Equals(storage, value))
            {
                return false;
            }
            else
            {
                storage = value;
                OnPropertyChanged(propertyName);
                return true;
            }
        }



    }
}
