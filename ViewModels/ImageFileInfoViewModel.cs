using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using ImageBrowser.Common;
using ImageBrowser.Helpers;
using ImageBrowser.Models;
using Microsoft.Graph;
using Microsoft.Identity.Client;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Search;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;

namespace ImageBrowser.ViewModels
{
    internal class ImageFileInfoViewModel : DependencyObject, INotifyPropertyChanged
    {
        private readonly static string EmptyOneDrive = LocalizationHelper.GetLocalizedStrings("oneDriveDownloadedInfoDefault");
        private readonly static string UserSignedOutNormal = LocalizationHelper.GetLocalizedStrings("normalSignOut");

        private ObservableCollection<ImageFileInfo> observableCollection = new ObservableCollection<ImageFileInfo>();

        public IList<ImageFileInfo> ObservableCollection { get => observableCollection; }
        public ObservableCollection<FolderInfoModel> foldersPath = new ObservableCollection<FolderInfoModel>();
        public FoldersViewModel foldersView = new FoldersViewModel();

        public event PropertyChangedEventHandler PropertyChanged;

        public ICommand OneDriveOpenCommand { get; set; }

        public ICommand SignOutCommand { get; set; }

        public ICommand SignInCommand { get; set; }
        public ICommand OpenCLickCommand { get; set; }
        public ICommand OpenFoldersCommand { get; set; }
        public ICommand RefreshCommand { get; set; }
        public ICommand ThemeChangeCommand { get; set; }
        public ICommand SettingsNavigateCommand { get; set; }
        public ICommand GridViewSizeChangeCommand { get; set; }

        #region DependecyProperties
        public static readonly DependencyProperty ResultTextProperty = DependencyProperty.Register(
        nameof(ResultText),
        typeof(string),
        typeof(ImageFileInfoViewModel),
       new PropertyMetadata(null, new PropertyChangedCallback(OnResultTextChanged)));

        public static readonly DependencyProperty StatusProperty = DependencyProperty.Register(
           nameof(IsUserSignedOut),
           typeof(bool),
           typeof(ImageFileInfoViewModel),
           new PropertyMetadata(false, new PropertyChangedCallback(OnStatusChanged)));

        public static readonly DependencyProperty OneDriveInfoTextProperty = DependencyProperty.Register(
         nameof(OneDriveInfoText),
         typeof(string),
         typeof(ImageFileInfoViewModel),
         new PropertyMetadata(EmptyOneDrive, new PropertyChangedCallback(OnOneDriveInfoTextChanged)));

        public static readonly DependencyProperty AnyObservableItemsProperty = DependencyProperty.Register(
            nameof(IsAnyObservableItem), typeof(bool), typeof(ImageFileInfoViewModel), new PropertyMetadata(false, new PropertyChangedCallback(OnObservableItemsCountChanged)));

        #endregion

        string defaultWinTheme = string.Empty;
        /// <summary>
        /// Initializes a new instance of the <see cref="ImageFileInfoViewModel"/> class,
        /// signup event and initializes commands.
        /// </summary>
        public ImageFileInfoViewModel()
        {
            OneDriveOpenCommand = new RelayCommand(OneDriveOpenAction());

            SignOutCommand = new RelayCommand(SigningOutAsync());
            SignInCommand = new RelayCommand(SigningInAsync());
            OpenCLickCommand = new RelayCommand(OpenClickAsync());
            OpenFoldersCommand = new RelayCommand(OpenFoldersAsync());
            RefreshCommand = new RelayCommand(RefreshAreaItemsAsync());
            ThemeChangeCommand = new RelayCommand(DefineClickedTheme);
            SettingsNavigateCommand = new RelayCommand(() => { Services.NavigationService.Instance.Navigate(typeof(Settings)); });
            GridViewSizeChangeCommand = new RelayMultipleCommand(GroupedGrid_SizeChanged);

            MSGraphQueriesHelper.PropertyChanged += SigningStatusViewModel_OnStatusChanged;

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

        #region XamlListningProperties
        public bool IsAnyItemsToShow;

        public bool IsUserSignedOut
        {
            get
            {
                return (bool)GetValue(StatusProperty);
            }

            set
            {

                SetValue(StatusProperty, MSGraphQueriesHelper.UserSignedOut);
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

        public string ResultText
        {
            get
            {
                return (string)GetValue(ResultTextProperty);
            }

            set
            {
                SetValue(ResultTextProperty, value);
            }
        }

        public bool IsAnyObservableItem
        {
            get => (bool)GetValue(AnyObservableItemsProperty);

            set => SetValue(AnyObservableItemsProperty, value);
        }

        #endregion
        #region DependecyProperties handlers
        private static void OnResultTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            string oldValue = (string)e.OldValue;
            string newValue = (string)e.NewValue;
            ImageFileInfoViewModel resultTextCallBack = d as ImageFileInfoViewModel;
            resultTextCallBack?.OnResultTextChanged(oldValue, newValue);
        }

        public virtual void OnResultTextChanged(string oldString, string newString)
        {
            if (oldString != newString)
                ResultText = newString;
            OnPropertyChanged(nameof(ResultText));
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

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null) =>
               PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));


        public async void ClickPictInGrid(object sender, object parameter)
        {
            var arg = parameter as Windows.UI.Xaml.Controls.ItemClickEventArgs;
            var item = arg.ClickedItem as ImageFileInfo;
            Services.NavigationService.Instance.Navigate(typeof(DetailPage), item);
        }

        #region Actions for commands in ctor
        private void GroupedGrid_SizeChanged(object sender, double e)
        {
            var panel = (ItemsWrapGrid)((sender as GridView).ItemsPanelRoot);
            /*if (e <= 180)
            {
                panel.ItemWidth = 180;
            }
            else
            {*/
                panel.ItemWidth = e / 3;
            //}
            Trace.WriteLine("From RElay multiple command");
        }

        /// <summary>
        /// Imlemented switching between <see cref="ElementTheme"/> .
        /// </summary>
        /// <param name="sender">Button that is clicked.</param>
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

        /// <summary>
        /// Reload collections of user added files.
        /// </summary>
        private Action RefreshAreaItemsAsync()
        {
            ICollection<StorageFile> files = new Collection<StorageFile>();
            ICollection<StorageFile> filesReal = new Collection<StorageFile>();
            ICollection<string> fileWithPaths = new Collection<string>();
            for (int i = 0; i < this.ObservableCollection.Count; i++)
            {

                files.Add(this.ObservableCollection[i].ImageFile);
                fileWithPaths.Add(this.ObservableCollection[i].ImagePath);

                // Get BitmapImage
                var p = this.ObservableCollection[i].GetImageSourceAsync();
            }

            IReadOnlyCollection<StorageFile> filesReadOnly = (IReadOnlyCollection<StorageFile>)files;
            return async () =>
            {
                Trace.WriteLine("REFRESHED by button in command");
                await this.PopulateObservableCollectionOfImages(filesReadOnly);
            };
        }

        private Action SigningInAsync()
        {
            return async () =>
            {
                try
                {
                    // Sign-in user using MSAL and obtain an access token for MS Graph
                    GraphServiceClient graphClient = await MSGraphQueriesHelper.SignInAndInitializeGraphServiceClient();
                    // Call the /me endpoint of Graph
                    User graphUser = await graphClient.Me.Request().GetAsync();

                    ResultText = "Display Name: " + graphUser.UserPrincipalName + "\nid: " + graphUser.Id;
                }
                catch (MsalException msalEx)
                {
                    Trace.WriteLine($"Error Acquiring Token:{System.Environment.NewLine}{msalEx}");
                    ResultText = $"Error Acquiring Token:{System.Environment.NewLine}{msalEx}";
                }
                catch (Exception ex)
                {
                    Trace.WriteLine($"Error Acquiring Token Silently:{System.Environment.NewLine}{ex}");
                    ResultText = $"Error Acquiring Token Silently:{System.Environment.NewLine}{ex}";
                    return;
                }
            };
        }

        private Action SigningOutAsync()
        {
            return async () =>
            {
                IEnumerable<IAccount> accounts = await MSGraphQueriesHelper.GetMSGraphAccouts();
                if (accounts == null)
                    return;
                IAccount firstAccount = accounts.FirstOrDefault();

                try
                {
                    await MSGraphQueriesHelper.SingOutMSGraphAccount(firstAccount).ConfigureAwait(false);
                    string message = LocalizationHelper.GetLocalizedStrings("normalSignOut");

                    ResultText = UserSignedOutNormal;
                    Trace.WriteLine("From ImageViewModel");

                    OneDriveInfoText = EmptyOneDrive;
                    this.FlushObservableCollectionOfImages();
                }
                catch (MsalException ex)
                {
                    Trace.WriteLine(ex.ToString());
                    ResultText = $"Error signing-out user: {ex.Message}";
                }
            };
        }

        private Action OneDriveOpenAction()
        {
            return async () =>
            {
                List<StorageFile> downloadedFiles = await MSGraphQueriesHelper.DownloadAllFilesFromOneDrive();

                if (Windows.UI.Core.CoreWindow.GetForCurrentThread() != null)
                {
                    var resourceLoader = Windows.ApplicationModel.Resources.ResourceLoader.GetForCurrentView();

                    OneDriveInfoText = resourceLoader.GetString("CountFiles/Text").ToString() + MSGraphQueriesHelper.CountFiles();
                }

                await this.PopulateObservableCollectionOfImages(downloadedFiles);
            };
        }

        private Action OpenClickAsync()
        {
            return async () => PickMultiplePictures();
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
            return await this.PopulateObservableCollectionOfImages(files);
        }

        private Action OpenFoldersAsync()
        {
            return async () => OpenFoldersButtonHandler();
        }

        private async Task<ObservableCollection<ImageFileInfo>> OpenFoldersButtonHandler()
        {
            FolderPicker folderPicker = new FolderPicker();
            folderPicker.SuggestedStartLocation = PickerLocationId.Downloads;
            folderPicker.ViewMode = PickerViewMode.Thumbnail;

            folderPicker.FileTypeFilter.Add(".jpg");
            folderPicker.FileTypeFilter.Add(".jpeg");
            folderPicker.FileTypeFilter.Add(".png");
            StorageFolder folder = await folderPicker.PickSingleFolderAsync();

            QueryOptions queryOptions = new QueryOptions(CommonFolderQuery.DefaultQuery);
            queryOptions.FileTypeFilter.Add(".jpg");
            queryOptions.FileTypeFilter.Add(".jpeg");
            queryOptions.FileTypeFilter.Add(".png");
            var queryResult = folder?.CreateFileQueryWithOptions(queryOptions);
            if (folder != null)
            {
                //  foldersView.FoldersToDisplay.Add(folder);
                foldersPath.Add(new FolderInfoModel() { FolderPath = folder.Path, FolderDisplayName = folder.DisplayName });
                IReadOnlyList<StorageFile> fileList = await folder.GetFilesAsync();
                IReadOnlyCollection<StorageFile> storageFiles = await queryResult.GetFilesAsync();

                return await this.PopulateObservableCollectionOfImages(storageFiles);
            }
            return null;
        }

        #endregion

        // TODO: catch main UI thread and extract into helper class
        private async Task ShowPopUpMessage(string message)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal,
          () =>
          {
              new MessageDialog(@"ERROR occures! {0}", message);
          });
        }

        public void ChangeObservCollection(ObservableCollection<ImageFileInfo> images)
        {
            observableCollection = images;
        }

        private ObservableCollection<GroupInfoList<object>> groupedImagesInfos = new ObservableCollection<GroupInfoList<object>>();

        public ObservableCollection<GroupInfoList<object>> GroupedImagesInfos { get => groupedImagesInfos; }

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
                GroupInfoList<object> infoList = new GroupInfoList<object>();
                infoList.Key = item.GroupName.mm + "/" + item.GroupName.yy + " (" + item.Items.Count() + ")";
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

            if (this.ObservableCollection.Count > 0)
                this.Initialize();
        }

        public async Task<ObservableCollection<ImageFileInfo>> PopulateObservableCollectionOfImages(IReadOnlyCollection<StorageFile> files)
        {
            if (files.Count <= 0)
            { return null; }
            else
            {

                this.ObservableCollection.Clear();
                this.GroupedImagesInfos.Clear();
                foreach (var file in files)
                {
                    ImageFileInfo item = await ImageFileHelper.LoadImageInfo(file);

                    this.ObservableCollection.Add(item);
                }
            }
            IsAnyObservableItem = HaveAnyItems();
            this.InitializeGroupingOfViewModel();

            return null;
        }

        public void FlushObservableCollectionOfImages()
        {
            if (this.ObservableCollection.Count <= 0)
            { }
            else
            {

                this.ObservableCollection.Clear();
                this.GroupedImagesInfos.Clear();
                IsAnyObservableItem = HaveAnyItems();
            }
        }

        public bool HaveAnyItems()
        {

            return this.ObservableCollection.Count > 0;
        }



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

        public async void RefreshArea_RefreshRequested(RefreshContainer sender, RefreshRequestedEventArgs args)
        {
            using (var RefreshcompletingDeferral = args.GetDeferral())
            {
                ICollection<StorageFile> files = new Collection<StorageFile>();

                for (int i = 0; i < this.ObservableCollection.Count; i++)
                {

                    files.Add(this.ObservableCollection[i].ImageFile);
                }

                IReadOnlyCollection<StorageFile> filesReadOnly = (IReadOnlyCollection<StorageFile>)files;
                await this.PopulateObservableCollectionOfImages(filesReadOnly);
                Trace.WriteLine("From VievModel execution");
            }
        }

        private object imageFileInfoViewModel1;

        public object imageFileInfoViewModel { get =>  imageFileInfoViewModel1; set => SetProperty(ref imageFileInfoViewModel1, value); }
    }
}