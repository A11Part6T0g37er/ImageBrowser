using ImageBrowser.Common;
using ImageBrowser.Helpers;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Windows.UI.Core;
using Windows.UI.Xaml;

namespace ImageBrowser.ViewModels
{
    public class SigningStatusViewModel :/* BindableBase */ DependencyObject, INotifyPropertyChanged
    {
        public static readonly DependencyObject dependencyObject;
        public static readonly DependencyProperty StatusProperty = DependencyProperty.Register(
            nameof(IsUserSignedOut),
            typeof(bool),
            typeof(SigningStatusViewModel),
            new PropertyMetadata(false, new PropertyChangedCallback(OnStatusChanged)));

        public event Action ChangeStatusUser = new Action(OnStatusChangedFromHelper);

        private static void OnStatusChangedFromHelper()
        {
            Trace.WriteLine("See That?");
        }

        private static void OnStatusChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            bool oldValue = (bool)e.OldValue;
            bool newValue = (bool)e.NewValue;
            SigningStatusViewModel signingStatus = d as SigningStatusViewModel;
            signingStatus?.OnStatusChanged(oldValue, newValue);
            ;
        }

        public virtual void OnStatusChanged(bool oldValue, bool newValue)
        {
            if (oldValue != newValue)
                IsUserSignedOut = newValue;
            OnPropertyChanged("IsUserSignedOut");
        }

        public event PropertyChangedEventHandler PropertyChanged;

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

        public SigningStatusViewModel()
        {
         /*   SignOutCommand = new RelayCommand(async()=> {
                IEnumerable<IAccount> accounts = await MSGraphQueriesHelper.GetMSGraphAccouts();
                if (accounts == null)
                    return;
                IAccount firstAccount = accounts.FirstOrDefault();

                try
                {
                    await MSGraphQueriesHelper.SingOutMSGraphAccount(firstAccount).ConfigureAwait(false);
                    await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        ResultText.Text = "User has signed-out";
                        signingOneDrive.Visibility = Visibility.Visible;
                        OpenOneDrive.Visibility = Visibility.Collapsed;
                        OneDriveInfo.Text = "";
                        imageFileInfoViewModel.FlushObservableCollectionOfImages();
                    });
                }
                catch (MsalException ex)
                {
                    ResultText.Text = $"Error signing-out user: {ex.Message}";
                }
            });*/




            MSGraphQueriesHelper.PropertyChanged += SigningStatusViewModel_OnStatusChanged;
        }

        private void SigningStatusViewModel_OnStatusChanged(object sender, PropertyChangedEventArgs e)
        {
            var newValue = (bool)sender;
            IsUserSignedOut = newValue;
            OnPropertyChanged("IsUserSignedOut");
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null) =>
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));


        ICommand SignOutCommand { get; set; }


    }
}
