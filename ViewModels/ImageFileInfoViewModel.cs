using ImageBrowser.Common;
using ImageBrowser.Common.RelayCommandProviders;
using ImageBrowser.Helpers;
using ImageBrowser.Models;
using ImageBrowser.Services;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Uwp.UI.Animations;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Search;
using Windows.UI.Xaml;

namespace ImageBrowser.ViewModels
{
	internal class ImageFileInfoViewModel : DependencyObject, INotifyPropertyChanged
	{
		private static readonly string EmptyOneDrive = LocalizationHelper.GetLocalizedStrings("oneDriveDownloadedInfoDefault");
		private static readonly string CountedFiles = LocalizationHelper.GetLocalizedStrings("CountFiles/Text");

		private ObservableCollection<ImageFileInfo> observableCollection = new ObservableCollection<ImageFileInfo>();
		public IList<ImageFileInfo> ObservableCollection { get => observableCollection; }
		public ObservableCollection<GroupInfoList<object>> GroupedImagesInfos { get; } = new ObservableCollection<GroupInfoList<object>>();

		private FoldersItemsCollection foldersItem = new FoldersItemsCollection();
		public FoldersItemsCollection FoldersItem { get => foldersItem; }

		public List<FolderInfoModel> MirorData = new List<FolderInfoModel>();

		public string CurrentFolderPath { get => currentFolderPath; set => SetProperty(ref currentFolderPath, value); }
		private string currentFolderPath;

		public event PropertyChangedEventHandler PropertyChanged;

		public ICommand OneDriveOpenCommand { get; set; }
		public ICommand SignOutCommand { get; set; }
		public ICommand SignInCommand { get; set; }
		public ICommand OpenCLickCommand { get; set; }
		public ICommand OpenFoldersCommand { get; set; }
		public ICommand RefreshCommand { get; set; }
		public ICommand ThemeChangeCommand { get; set; }
		public ICommand SettingsNavigateCommand { get; set; }


		#region DependecyProperties


		public static readonly DependencyProperty StatusProperty = DependencyProperty.Register(
		   nameof(IsUserSignedOut),
		   typeof(bool),
		   typeof(ImageFileInfoViewModel),
		   new PropertyMetadata(false, new PropertyChangedCallback(OnStatusChanged)));

		public static readonly DependencyProperty SubFoldersProperty = DependencyProperty.Register(nameof(IsFolderDived), typeof(bool), typeof(ImageFileInfoViewModel), new PropertyMetadata(false, new PropertyChangedCallback(OnSubFoldersDiveInto)));

		public static readonly DependencyProperty ItemsToShowProperty = DependencyProperty.Register(nameof(IsNoItemsToShow), typeof(bool), typeof(ImageFileInfoViewModel), new PropertyMetadata(false, new PropertyChangedCallback(OnItemsToShow)));


		public static readonly DependencyProperty OneDriveInfoTextProperty = DependencyProperty.Register(
		 nameof(OneDriveInfoText),
		 typeof(string),
		 typeof(ImageFileInfoViewModel),
		 new PropertyMetadata(EmptyOneDrive, new PropertyChangedCallback(OnOneDriveInfoTextChanged)));

		public static readonly DependencyProperty AnyObservableItemsProperty = DependencyProperty.Register(
			nameof(IsAnyObservableItem), typeof(bool), typeof(ImageFileInfoViewModel), new PropertyMetadata(false, new PropertyChangedCallback(OnObservableItemsCountChanged)));

		#endregion

		/// <summary>
		/// Initializes a new instance of the <see cref="ImageFileInfoViewModel"/> class,
		/// signup event and initializes commands.
		/// </summary>
		public ImageFileInfoViewModel()
		{
			OneDriveOpenCommand = new RelayCommand(OneDriveOpenAsyncExecute);

			SignOutCommand = new RelayCommand(SigningOutAsyncExecute);
			SignInCommand = new RelayCommand(SigningInAsyncExecute);
			OpenCLickCommand = new RelayCommand(OpenClickAsyncExecute);
			OpenFoldersCommand = new RelayCommand(OpenFoldersAsyncExecute);
			RefreshCommand = new RelayCommand(RefreshAreaItemsAsyncExecute);
			ThemeChangeCommand = new RelayCommand<ThemeSendingArgs>(CommandInstantiator.DefineClickedThemeExecute);

			SettingsNavigateCommand = new RelayCommand(() => { Services.NavigationService.Instance.Navigate(typeof(Settings)); });

			MSGraphQueriesHelper.PropertyChanged += SigningStatusViewModel_OnStatusChanged;
		}

		#region XamlListningProperties
		public bool IsFolderDived
		{
			get
			{
				return (bool)GetValue(SubFoldersProperty);
			}
			set
			{
				SetValue(SubFoldersProperty, value);
			}
		}

		public bool IsNoItemsToShow
		{
			get
			{
				return (bool)GetValue(ItemsToShowProperty);
			}
			set
			{
				SetValue(ItemsToShowProperty, value);
			}
		}
		public bool IsUserSignedOut
		{
			get
			{
				return (bool)GetValue(StatusProperty);
			}
			set
			{
				SetValue(StatusProperty, value);
			}
		}
		public string OneDriveInfoText
		{
			get
			{
				return (string)GetValue(OneDriveInfoTextProperty);
			}
			set
			{
				SetValue(OneDriveInfoTextProperty, value);
			}
		}

		private string resultText;
		public string ResultText { get => resultText; set => SetProperty(ref resultText, value); }

		public bool IsAnyObservableItem
		{
			get => (bool)GetValue(AnyObservableItemsProperty);

			set => SetValue(AnyObservableItemsProperty, value);
		}

		private bool longLoad;
		public bool LongLoad { get => longLoad; set => SetProperty(ref longLoad, value); }

		#endregion
		#region DependecyProperties handlers

		private static void OnItemsToShow(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			bool oldValue = (bool)e.OldValue;
			bool newValue = (bool)e.NewValue;

			var viewModel = d as ImageFileInfoViewModel;
			viewModel?.OnItemsToShow(oldValue, newValue);
		}
		public virtual void OnItemsToShow(bool oldValue, bool newValue)
		{
			if (oldValue != newValue)
			{
				IsNoItemsToShow = newValue;
				OnPropertyChanged(nameof(IsNoItemsToShow));
			}
		}
		private static void OnSubFoldersDiveInto(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			bool oldValue = (bool)e.OldValue;
			bool newValue = (bool)e.NewValue;

			var viewModel = d as ImageFileInfoViewModel;
			viewModel?.OnSubFoldersDiveInto(oldValue, newValue);

		}
		public virtual void OnSubFoldersDiveInto(bool oldValue, bool newValue)
		{
			if (oldValue != newValue)
			{
				IsFolderDived = newValue;
				OnPropertyChanged(nameof(IsFolderDived));
			}
		}

		private static void OnOneDriveInfoTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			string oldValue = (string)e.OldValue;
			string newValue = (string)e.NewValue;
			ImageFileInfoViewModel signingStatus = d as ImageFileInfoViewModel;
			signingStatus?.OnOneDriveInfoTextChanged(oldValue, newValue);
		}

		public virtual void OnOneDriveInfoTextChanged(string oldString, string newString)
		{
			if (oldString != newString)
				OneDriveInfoText = newString;
			OnPropertyChanged(nameof(OneDriveInfoText));
		}

		private static void OnStatusChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			bool oldValue = (bool)e.OldValue;
			bool newValue = (bool)e.NewValue;
			ImageFileInfoViewModel signingStatus = d as ImageFileInfoViewModel;
			signingStatus?.OnStatusChanged(oldValue, newValue);
		}

		public virtual void OnStatusChanged(bool oldValue, bool newValue)
		{
			if (oldValue != newValue)
				IsUserSignedOut = newValue;
			OnPropertyChanged(nameof(IsUserSignedOut));
		}

		private void SigningStatusViewModel_OnStatusChanged(object sender, PropertyChangedEventArgs e)
		{
			var newValue = (bool)sender;
			IsUserSignedOut = newValue;
			OnPropertyChanged(nameof(IsUserSignedOut));
		}

		private static void OnObservableItemsCountChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			bool oldValue = (bool)e.OldValue;
			bool newValue = (bool)e.NewValue;
			ImageFileInfoViewModel obsevableCountChange = d as ImageFileInfoViewModel;
			obsevableCountChange?.OnObservableItemsCountChanged(oldValue, newValue);
		}
		public virtual void OnObservableItemsCountChanged(bool oldValue, bool newValue)
		{
			if (oldValue != newValue)
				IsAnyObservableItem = newValue;
			OnPropertyChanged(nameof(IsAnyObservableItem));
		}
		#endregion

		public async void ClickPictInGrid(object sender, object parameter)
		{
			var arg = parameter as Windows.UI.Xaml.Controls.ItemClickEventArgs;
			var item = arg.ClickedItem as ImageFileInfo;
			NavigationServiceMain.Frame.SetListDataItemForNextConnectedAnimation(item);
			Services.NavigationService.Instance.Navigate(typeof(DetailPage), item);
		}

		public async void RestoreFoldersInGrid(object sender, object parameter)
		{
			IsFolderDived = false;
			// foldersItem.foldersPath.Clear();
			FoldersItem.ResetCollection();
			foreach (var item in MirorData)
			{
				FoldersItem.FoldersPath.Add(item);
			}
			IsNoItemsToShow = false;

		}

		public async void ClickFoldersInGrid(object sender, object parameter)
		{
			var arg = parameter as Windows.UI.Xaml.Controls.ItemClickEventArgs;
			var item = arg.ClickedItem as FolderInfoModel;
			IsNoItemsToShow = false;
			IsFolderDived = true;
			FoldersItem.FoldersPath.Clear();
			CurrentFolderPath = item.FolderPath;
			FolderInfoModel folderNew;
			foreach (var folder in item.FolderList)
			{
				folderNew = new FolderInfoModel() { FolderDisplayName = folder.DisplayName, FolderList = await folder.GetFoldersAsync(), FolderPath = folder.Path };
				FoldersItem.FoldersPath.Add(folderNew);
			}
			StorageFolder parentFolder = await FileRetrieveHelper.GetParentFolder(item).ConfigureAwait(true);
			//string folderName = parentFolder.DisplayName;
			if (parentFolder == null)
				return;
			IReadOnlyList<StorageFile> listFiles = await ImageDownloadHelper.ExtractFromFolderPicts(parentFolder).ConfigureAwait(true);
			FoldersItem.PictsFromFolders.Clear();
			if (listFiles.Any())
			{
				IsNoItemsToShow = false;
				foreach (var pictOfFolder in listFiles)
				{
					FoldersItem.PictsFromFolders.Add(await ImageFileHelper.LoadImageInfo(pictOfFolder));
				}

			}
			else
			{
				if (FoldersItem.FoldersPath.Count <= 0)
					IsNoItemsToShow = true;
			}
		}

		#region Actions for commands in ctor


		/// <summary>
		/// Reload collections of user added files.
		/// </summary>
		private async void RefreshAreaItemsAsyncExecute()
		{
			ICollection<StorageFile> files = new Collection<StorageFile>();
			LongLoad = true;

			for (int i = 0; i < ObservableCollection.Count; i++)
			{

				files.Add(ObservableCollection[i].ImageFile);

			}

			IReadOnlyCollection<StorageFile> filesReadOnly = (IReadOnlyCollection<StorageFile>)files;

			Trace.WriteLine("REFRESHED by button in command");
			await PopulateObservableCollectionOfImages(filesReadOnly);
			GC.Collect();
			LongLoad = false;
		}

		private async void SigningInAsyncExecute()
		{
			LongLoad = true;
			(ResultText, IsUserSignedOut) = await MSGraphQueriesHelper.TrySignInUser();
			LongLoad = false;

		}

		private async void SigningOutAsyncExecute()
		{
			LongLoad = true;
			(ResultText, IsUserSignedOut, OneDriveInfoText) = await MSGraphQueriesHelper.TrySignOutUser(OneDriveInfoText).ConfigureAwait(false);
			FlushObservableCollectionOfImages();
			LongLoad = false;

		}

		private async void OneDriveOpenAsyncExecute()
		{
			LongLoad = true;

			List<StorageFile> downloadedFiles = await MSGraphQueriesHelper.DownloadAllFilesFromOneDrive();


			OneDriveInfoText = CountedFiles + MSGraphQueriesHelper.CountFiles();


			await PopulateObservableCollectionOfImages(downloadedFiles).ConfigureAwait(true);
			LongLoad = false;

		}

		private async void OpenClickAsyncExecute()
		{
			LongLoad = true;
			await PickMultiplePictures();
			LongLoad = false;

		}

		private async Task<ObservableCollection<ImageFileInfo>> PickMultiplePictures()
		{
			var picker = new FileOpenPicker
			{
				ViewMode = PickerViewMode.Thumbnail,
				SuggestedStartLocation = PickerLocationId.Desktop
			};

			picker.FileTypeFilter.Add(".jpg");
			picker.FileTypeFilter.Add(".jpeg");
			picker.FileTypeFilter.Add(".png");
			IReadOnlyCollection<StorageFile> files = await picker.PickMultipleFilesAsync();
			return await PopulateObservableCollectionOfImages(files).ConfigureAwait(false);
		}

		private async void OpenFoldersAsyncExecute()
		{
			LongLoad = true;
			await OpenFoldersButtonHandler().ConfigureAwait(true);
			LongLoad = false;

		}

		#endregion

		private async Task OpenFoldersButtonHandler()
		{

			FolderPicker folderPicker = new FolderPicker
			{
				SuggestedStartLocation = PickerLocationId.Downloads,
				ViewMode = PickerViewMode.Thumbnail
			};

			folderPicker.FileTypeFilter.Add(".jpg");
			folderPicker.FileTypeFilter.Add(".jpeg");
			folderPicker.FileTypeFilter.Add(".png");
			StorageFolder folder = await folderPicker.PickSingleFolderAsync();

			QueryOptions queryOptions = new QueryOptions(CommonFolderQuery.DefaultQuery);
			queryOptions.FileTypeFilter.Add(".jpg");
			queryOptions.FileTypeFilter.Add(".jpeg");
			queryOptions.FileTypeFilter.Add(".png");
			// queryOptions.FileTypeFilter.Add("*");
			queryOptions.FolderDepth = FolderDepth.Shallow;
			queryOptions.IndexerOption = IndexerOption.UseIndexerWhenAvailable;
			var queryResult = folder?.CreateFileQueryWithOptions(queryOptions);
			if (folder != null)
			{

				//queryResult.ContentsChanged += OnContentsChanged;

				// Get token for recently used list
				var mru = Windows.Storage.AccessCache.StorageApplicationPermissions.MostRecentlyUsedList.Add(folder, "PickedFolderToken");

				var Resultsubfolders = folder.CreateFolderQueryWithOptions(queryOptions);
				var subFolders = await Resultsubfolders.GetFoldersAsync();

				FolderInfoModel folderAdded = new FolderInfoModel() { FolderPath = folder.Path, FolderDisplayName = folder.DisplayName, FolderList = subFolders };
				FoldersItem.FoldersPath.Add(folderAdded);
				FoldersItem.CurentFolder = folder;

				queryOptions.FolderDepth = FolderDepth.Deep;


				queryResult = folder.CreateFileQueryWithOptions(queryOptions);

				IReadOnlyCollection<StorageFile> storageFiles = await queryResult.GetFilesAsync();

				// create savePoint
				// MirorData = FoldersItem.FoldersPath.ToList();
				MirorData.Add(folderAdded);
				//if(MirorData is null)
				//{
				//	MirorData = new List<FolderInfoModel>();
				//MirorData.AddRange(FoldersItem.FoldersPath.ToList());

				//}
				//else{
				//	MirorData.AddRange(FoldersItem.FoldersPath.ToList()); }

				await PopulateObservableCollectionOfImages(storageFiles).ConfigureAwait(true);
			}

		}

		public void ChangeObservCollection(ObservableCollection<ImageFileInfo> images)
		{
			observableCollection = images;
		}

		public void GenerateByDateGroup(IList<ImageFileInfo> lisOfImages)
		{
			var query = from item in lisOfImages
						group item by new { yy = item.ImageFile.DateCreated.Year, mm = item.ImageFile.DateCreated.Month } into dateKey
						orderby dateKey.Key.yy descending
						select new { GroupName = dateKey.Key, Items = dateKey };
			if (GroupedImagesInfos.Count > 0)
			{
				GroupedImagesInfos.Clear();
			}

			foreach (var item in query)
			{
				GroupInfoList<object> infoList = new GroupInfoList<object>
				{
					Key = item.GroupName.mm + "/" + item.GroupName.yy + " (" + item.Items.Count() + ")"
				};
				foreach (var something in item.Items)
				{
					infoList.Add(something);
				}

				GroupedImagesInfos.Add(infoList);
			}
		}

		private void Initialize()
		{
			var data = ObservableCollection;

			GenerateByDateGroup(data);
		}

		public void InitializeGroupingOfViewModel()

		{

			if (ObservableCollection.Count > 0)
				Initialize();
		}

		public async Task<ObservableCollection<ImageFileInfo>> PopulateObservableCollectionOfImages(IReadOnlyCollection<StorageFile> files)
		{
			if (files.Count <= 0)
			{ return null; }


			ObservableCollection.Clear();
			GroupedImagesInfos.Clear();
			foreach (var file in files)
			{
				ImageFileInfo item = await ImageFileHelper.LoadImageInfo(file);

				ObservableCollection.Add(item);
			}

			IsAnyObservableItem = HaveAnyItems();
			InitializeGroupingOfViewModel();

			GC.Collect();
			return observableCollection;
		}

		public void FlushObservableCollectionOfImages()
		{
			if (ObservableCollection.Count <= 0)
			{ }
			else
			{

				ObservableCollection.Clear();
				GroupedImagesInfos.Clear();
				IsAnyObservableItem = HaveAnyItems();
			}
		}

		public bool HaveAnyItems()
		{

			return ObservableCollection.Count > 0;
		}

		protected void OnPropertyChanged([CallerMemberName] string propertyName = null) =>
			   PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
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