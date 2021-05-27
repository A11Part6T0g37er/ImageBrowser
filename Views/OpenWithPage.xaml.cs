using ImageBrowser.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace ImageBrowser.Views
{
	/// <summary>
	/// Page to render file with picture extensions is clicked from file Explorer.
	/// </summary>
	public sealed partial class OpenWithPage : Page
	{
		public OpenWithPage()
		{
			this.InitializeComponent();
		}

		protected override async void OnNavigatedTo(NavigationEventArgs e)
		{

			// when opened by file-extension with arguments
			base.OnNavigatedTo(e);
			var args = e?.Parameter as Windows.ApplicationModel.Activation.IActivatedEventArgs;

			if (args?.Kind == Windows.ApplicationModel.Activation.ActivationKind.File)
			{

				var fileArgs = args as Windows.ApplicationModel.Activation.FileActivatedEventArgs;
				string strFilePath = fileArgs.Files[0].Path;
				StorageFile firstFile = (StorageFile)fileArgs.Files[0];

				using (IRandomAccessStream fileStream = await firstFile.OpenReadAsync())
				{
					// Create a bitmap to be the image source.
					var imageSource = new BitmapImage();
					imageSource.SetSource(fileStream);

					targetImage.Source = imageSource;
				}

				TargetName.Text = strFilePath;
			}
		}

		private void GoHome_Click(object sender, RoutedEventArgs e)
		{
			this.Frame.Navigate(typeof(MainPage), null);
		}
	}
}
