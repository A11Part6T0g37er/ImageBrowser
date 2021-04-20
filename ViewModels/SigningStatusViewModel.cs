using ImageBrowser.Helpers;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Windows.UI.Xaml;

namespace ImageBrowser.ViewModels
{
    public class SigningStatusViewModel :/* BindableBase */ DependencyObject, INotifyPropertyChanged
    {
        public static readonly DependencyObject dependencyObject;
        public static readonly DependencyProperty StatusProperty = DependencyProperty.Register(
  nameof(IsUserSignedOut),
  typeof(Boolean),
  typeof(SigningStatusViewModel),
  new PropertyMetadata(false, new PropertyChangedCallback(OnStatusChanged))
);
        public event Action ChangeStatusUser = new Action(OnStatusChangedFromHelper);

        private static void OnStatusChangedFromHelper()
        {
            Trace.WriteLine("See That?");
        }

      

        // TODO catch property from helper 
        private static void OnStatusChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            bool oldValue = (bool)e.OldValue;
            bool newValue = MSGraphQueriesHelper.UserSignedOut;
            SigningStatusViewModel signingStatus = d as SigningStatusViewModel;
            signingStatus?.OnStatusChanged(oldValue,newValue);
            ;
        }

        public virtual void OnStatusChanged(bool oldValue, bool newValue)
        {
            if (oldValue != newValue)
                IsUserSignedOut = newValue;
            OnPropertyChanged("IsUserSignedOut");
        }

        private bool _isUserSignedOut;/* for accessor and mutator */

        public event PropertyChangedEventHandler PropertyChanged;

        public bool IsUserSignedOut
        {
            get
            {
                //return MSGraphQueriesHelper.UserSignedOut;
                return (bool)GetValue(StatusProperty);
                //return _isUserSignedOut;

            }
            set
            {
                //base.SetProperty(ref _isUserSignedOut, MSGraphQueriesHelper.UserSignedOut);
                SetValue(StatusProperty, MSGraphQueriesHelper.UserSignedOut);
            }
        }

        public SigningStatusViewModel()
        {
            // RegisterPropertyChangedCallback(IsUserSignedOut, OnStatusChanged);
            //  IsUserSignedOut = MSGraphQueriesHelper.UserSignedOut;
            MSGraphQueriesHelper.PropertyChanged += SigningStatusViewModel_OnStatusChanged;
        }

        private void SigningStatusViewModel_OnStatusChanged(object sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged("IsUserSignedOut");
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null) =>
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
