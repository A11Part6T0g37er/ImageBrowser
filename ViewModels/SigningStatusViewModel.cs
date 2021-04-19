using ImageBrowser.Helpers;
using System;
using Windows.UI.Xaml;

namespace ImageBrowser.ViewModels
{
    public class SigningStatusViewModel : BindableBase /* DependencyObject*/
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

        }

        private bool _isUserSignedOut;/* = MSGraphQueriesHelper.UserSignedOut;*/
        public bool IsUserSignedOut
        {
            get
            {
                // return MSGraphQueriesHelper.UserSignedOut;
                //return (bool)GetValue(StatusProperty);
                return _isUserSignedOut;

            }
            set
            {
                base.SetProperty(ref _isUserSignedOut, MSGraphQueriesHelper.UserSignedOut);
                //SetValue(StatusProperty, MSGraphQueriesHelper.UserSignedOut);
            }
        }

        public SigningStatusViewModel()
        {
            // RegisterPropertyChangedCallback(IsUserSignedOut, OnStatusChanged);
            //  IsUserSignedOut = MSGraphQueriesHelper.UserSignedOut;
        }
        
    }
}
