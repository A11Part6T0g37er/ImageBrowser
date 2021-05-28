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
using Windows.Storage;

namespace ImageBrowser.Helpers
{
	public class MSGraphQueriesHelper
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
		private static readonly string EmptyOneDrive = LocalizationHelper.GetLocalizedStrings("oneDriveDownloadedInfoDefault");
		private static readonly string UserSignedOutNormal = LocalizationHelper.GetLocalizedStrings("normalSignOut");

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
			GraphServiceClient grSC = await SignInAndInitializeGraphServiceClient().ConfigureAwait(false);

			search = await GetFilesByQuery(grSC).ConfigureAwait(false);

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
					itemName).ConfigureAwait(false);
				storageFile = await StorageFile.GetFileFromApplicationUriAsync(new Uri(newPath));
				downloadedFiles.Add(storageFile);
			}

			return downloadedFiles;
		}

		public bool IsSignedOut()
		{
			return UserSignedOut;
		}

		public static string CountFiles()
		{
			return GetFilesCount(search);
		}

		public static async Task<IEnumerable<IAccount>> GetMSGraphAccouts()
		{
			return publicClientApp != null ? await publicClientApp.GetAccountsAsync().ConfigureAwait(false) : null;
		}

		public static async Task SingOutMSGraphAccount(IAccount firstAccount)
		{
			SetProperty(ref UserSignedOut, false);

			await publicClientApp.RemoveAsync(firstAccount).ConfigureAwait(false);
		}

		/// <summary>
		/// Sign in user using MSAL and obtain a token for Microsoft Graph.
		/// </summary>
		/// <returns>GraphServiceClient.</returns>
		public static async Task<GraphServiceClient> SignInAndInitializeGraphServiceClient()
		{

			GraphServiceClient graphClient = new GraphServiceClient(MSGraphURL,
				new DelegateAuthenticationProvider(async (requestMessage) =>
				{
					await Task.Run(async () => requestMessage.Headers.Authorization = new AuthenticationHeaderValue("bearer", await SignInUserAndGetTokenUsingMSAL(Scopes).ConfigureAwait(false))).ConfigureAwait(false);
				}));
			// SetProperty(ref UserSignedOut, true);
			return await Task.FromResult(graphClient).ConfigureAwait(false);
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
												  .ExecuteAsync().ConfigureAwait(false);

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

			var FoundPicts = await grSC.Me.Drive.Root.ItemWithPath("/Pictures")
				.Search("jpg")
				.Request(queryOptions)
				.GetAsync().ConfigureAwait(false);
			return FoundPicts;
		}

		private static string GetFilesCount(IDriveItemSearchCollectionPage search)
		{
			return search.Count.ToString();
		}

		protected static void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(MSGraphQueriesHelper.UserSignedOut, new PropertyChangedEventArgs(propertyName));
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

		public static async Task<Tuple<string, bool>> TrySignInUser()
		{
			bool IsUserSignedOut;
			string ResultText;
			try
			{
				// Sign-in user using MSAL and obtain an access token for MS Graph
				GraphServiceClient graphClient = await MSGraphQueriesHelper.SignInAndInitializeGraphServiceClient();
				// Call the /me endpoint of Graph
				User graphUser = await graphClient.Me.Request().GetAsync();
				ResultText = "Display Name: " + graphUser.UserPrincipalName + "\nid: " + graphUser.Id;
				IsUserSignedOut = true;
				return Tuple.Create(ResultText, IsUserSignedOut);
			}
			catch (MsalException msalEx)
			{
				Trace.WriteLine($"Error Acquiring Token:{Environment.NewLine}{msalEx}");
				ResultText = $"Error Acquiring Token:{Environment.NewLine}{msalEx}";
				IsUserSignedOut = false;
				return Tuple.Create(ResultText, IsUserSignedOut);
			}
			catch (Exception ex)
			{
				Trace.WriteLine($"Error Acquiring Token Silently:{Environment.NewLine}{ex}");
				ResultText = $"Error Acquiring Token Silently:{Environment.NewLine}{ex}";
				IsUserSignedOut = false;
				return Tuple.Create(ResultText, IsUserSignedOut);
			}
		}


		public static async Task<Tuple<string, bool, string>> TrySignOutUser(string oneDriveInfoText)
		{
			string ResultText, OneDriveInfoText;
			bool IsUserSignedOut;
			IEnumerable<IAccount> accounts = await MSGraphQueriesHelper.GetMSGraphAccouts();
			if (accounts == null)
				throw new Exception("No user found!");
			IAccount firstAccount = accounts.FirstOrDefault();

			try
			{
				await MSGraphQueriesHelper.SingOutMSGraphAccount(firstAccount).ConfigureAwait(false);
				string message = LocalizationHelper.GetLocalizedStrings("normalSignOut");
				IsUserSignedOut = false;
				ResultText = UserSignedOutNormal;
				Trace.WriteLine("From ImageViewModel");

				OneDriveInfoText = EmptyOneDrive;



				return Tuple.Create(ResultText, IsUserSignedOut, OneDriveInfoText);
			}
			catch (MsalException ex)
			{
				Trace.WriteLine(ex.ToString());
				IsUserSignedOut = true;
				ResultText = $"Error signing-out user: {ex.Message}";
				return Tuple.Create(ResultText, IsUserSignedOut, oneDriveInfoText);
			}
		}

	}
}
