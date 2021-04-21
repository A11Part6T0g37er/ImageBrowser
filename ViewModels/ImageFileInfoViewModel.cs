using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using ImageBrowser.Common;
using ImageBrowser.Helpers;
using ImageBrowser.Models;
using Microsoft.Identity.Client;
using Windows.Storage;
using Windows.UI.Xaml;

namespace ImageBrowser.ViewModels
{
    internal class ImageFileInfoViewModel: DependencyObject, INotifyPropertyChanged
    {

        private IList<ImageFileInfo> observableCollection = new List<ImageFileInfo>();

        public IList<ImageFileInfo> ObservableCollection { get => observableCollection; }

        public static readonly DependencyProperty ResultTextProperty = DependencyProperty.Register(
          nameof(ResultText),
          typeof(string),
          typeof(SigningStatusViewModel),
          null);

        public static readonly DependencyProperty StatusProperty = DependencyProperty.Register(
           nameof(IsUserSignedOut),
           typeof(bool),
           typeof(SigningStatusViewModel),
           new PropertyMetadata(false, new PropertyChangedCallback(OnStatusChanged)));

        public ImageFileInfoViewModel()
        {
            OneDriveOpenCommand = new RelayCommand(OneDriveOpenAction());

            SignOutCommand = new RelayCommand(SigningOutAsync());

            MSGraphQueriesHelper.PropertyChanged += SigningStatusViewModel_OnStatusChanged;
        }


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
        private static void OnStatusChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            bool oldValue = (bool)e.OldValue;
            bool newValue = (bool)e.NewValue;
            SigningStatusViewModel signingStatus = d as SigningStatusViewModel;
            signingStatus?.OnStatusChanged(oldValue, newValue);
            
        }

        public virtual void OnStatusChanged(bool oldValue, bool newValue)
        {
            if (oldValue != newValue)
                IsUserSignedOut = newValue;
            OnPropertyChanged("IsUserSignedOut");
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null) =>
               PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

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

                    
                    ResultText = message;
                    Trace.WriteLine("From ImageViewModel");
                    /*  await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                      {
                          
                          OneDriveInfo.Text = "";
                          imageFileInfoViewModel.FlushObservableCollectionOfImages();
                      });
               */
                }
                catch (MsalException ex)
                {

                    ResultText = $"Error signing-out user: {ex.Message}";
                }
            };
        }

        private void SigningStatusViewModel_OnStatusChanged(object sender, PropertyChangedEventArgs e)
        {
            var newValue = (bool)sender;
            IsUserSignedOut = newValue;
            OnPropertyChanged("IsUserSignedOut");
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

        private   Action OneDriveOpenAction()
        {
            return async () =>
            {
                List<StorageFile> downloadedFiles = await MSGraphQueriesHelper.DownloadAllFilesFromOneDrive();

                if (Windows.UI.Core.CoreWindow.GetForCurrentThread() != null)
                {
                    var resourceLoader = Windows.ApplicationModel.Resources.ResourceLoader.GetForCurrentView();

                 //   OneDriveInfo.Text = resourceLoader.GetString("CountFiles/Text").ToString() + MSGraphQueriesHelper.CountFiles();
                }

                await this.PopulateObservableCollectionOfImages(downloadedFiles);
            };
        }

        public void ChangeObservCollection(ObservableCollection<ImageFileInfo> images)
        {
            observableCollection = images;
        }

        private ObservableCollection<GroupInfoList<object>> groupedImagesInfos = new ObservableCollection<GroupInfoList<object>>();

        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<GroupInfoList<object>> GroupedImagesInfos { get => groupedImagesInfos; }

        public void GenerateByDateGroup(IList<ImageFileInfo> lisOfImages)
        {
            var query = from item in lisOfImages
                        group item by new { yy = item.ImageProperties.DateTaken.Year, mm = item.ImageProperties.DateTaken.Month } into dateKey
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
            }
        }
        public bool HaveAnyItems()
        {
            return this.ObservableCollection.Count > 0;
        }



       public ICommand OneDriveOpenCommand { get; set; }

        public ICommand SignOutCommand { get; set; }
    }
}
