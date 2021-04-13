using Microsoft.Graph;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace ImageBrowser.Helpers
{
  public static  class MSGraphQueriesHelper
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


        static async public Task<List<StorageFile>>  DownloadAllFilesFromOneDrive()
        {
            GraphServiceClient grSC = new GraphServiceClient(MSGraphURL,
               new DelegateAuthenticationProvider(async (requestMessage) =>
               {
                   await Task.Run(() => requestMessage.Headers.Authorization = new AuthenticationHeaderValue("bearer", authResult.AccessToken));
               }));

            var queryOptions = new List<QueryOption>()
            {
                new QueryOption("select", "*")
            };

            var search = await grSC.Me.Drive.Root.ItemWithPath("/Pictures")
                .Search("jpg")
                .Request(queryOptions)
                .GetAsync();

            if (Windows.UI.Core.CoreWindow.GetForCurrentThread() != null)
            {
                var resourceLoader = Windows.ApplicationModel.Resources.ResourceLoader.GetForCurrentView();

                string v = resourceLoader.GetString("CountFiles/Text").ToString();

                string v1 = search.Count.ToString();
                string v2 = v + v1;
             //   OneDriveInfo.Text = v2;
                // make tulpe here
            }

            List<DriveItem> oneDriveItems = search.CurrentPage.Select(x => x).ToList();
            StorageFile storageFile;
            String newPath = String.Empty;
            List<StorageFile> downloadedFiles = new List<StorageFile>();

            foreach (var item in oneDriveItems)
            {
                var itemUrl = item.AdditionalData.Values.FirstOrDefault().ToString();
                var itemName = item.Name;
                newPath = await ImageDownloadHelper.DownloadImage(itemUrl,
                  itemName);
                storageFile = await StorageFile.GetFileFromApplicationUriAsync(new Uri(newPath));
                downloadedFiles.Add(storageFile);
            }
            return downloadedFiles;
        }
    }
}
