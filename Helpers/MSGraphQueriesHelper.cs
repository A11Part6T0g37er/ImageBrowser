using ImageBrowser.Common;
using Microsoft.Graph;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml;

namespace ImageBrowser.Helpers
{
    public  class MSGraphQueriesHelper
    {
        #region MSGraphAPI

        // The MSAL Public client app
        public static IPublicClientApplication PublicClientApp;
        private const string ClientId = "c6e3c937-e10d-4e7c-94d7-bbaaafc514aa";
        private static string[] scopes = new string[] { "user.read Files.Read" };
        private const string Tenant = "consumers";
        private const string Authority = "https://login.microsoftonline.com/" + Tenant;
        private static readonly string MSGraphURL = "https://graph.microsoft.com/v1.0/";
        private static AuthenticationResult authResult;

        #endregion
        private static IDriveItemSearchCollectionPage search;

         

        /// <summary>
        /// Establish connection to OneDrive, get authentication, obtains links of files with query option, downloads files in temporary place.
        /// </summary>
        /// <returns>Returnsl List of files.</returns>
        public static async Task<List<StorageFile>> DownloadAllFilesFromOneDrive()
        {
            GraphServiceClient grSC = await SignInAndInitializeGraphServiceClient(scopes);

            search = await GetFilesByQuery(grSC);

            List<DriveItem> oneDriveItems = search.CurrentPage.Select(x => x).ToList();
            StorageFile storageFile;
            String newPath = String.Empty;
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

        public static string CountFiles()
        {
            return GetFilesCount(search);
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
                    await Task.Run(async () => requestMessage.Headers.Authorization = new AuthenticationHeaderValue("bearer", await SignInUserAndGetTokenUsingMSAL(scopes)));
                }));

            return await Task.FromResult(graphClient);
        }

        /// <summary>
        /// Signs in the user and obtains an Access token for MS Graph
        /// </summary>
        /// <param name="scopes"></param>
        /// <returns> Access Token</returns>
        public static async Task<string> SignInUserAndGetTokenUsingMSAL(string[] scopes)
        {
            // Initialize the MSAL library by building a public client application
            PublicClientApp = PublicClientApplicationBuilder.Create(ClientId)
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


       


    }
}
