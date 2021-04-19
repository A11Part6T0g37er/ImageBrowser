using ImageBrowser.Helpers;
using System;
using System.ComponentModel;
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
        // TODO silving falling down issue  
        private static void OnStatusChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            bool oldValue = (bool)e.OldValue;
            bool newValue = MSGraphQueriesHelper.UserSignedOut;
            SigningStatusViewModel signingStatus = d as SigningStatusViewModel;
            signingStatus?.OnStatusChanged();
        }

        protected virtual void OnStatusChanged()
        {
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

     /*   public SigningStatusViewModel()
        {
            // RegisterPropertyChangedCallback(IsUserSignedOut, OnStatusChanged);
            //  IsUserSignedOut = MSGraphQueriesHelper.UserSignedOut;
        }*/
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null) =>
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
