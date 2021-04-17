using ImageBrowser.Helpers;
using System;
using Windows.UI.Xaml;

namespace ImageBrowser.ViewModels
{
    public class SigningStatusViewModel : DependencyObject
    {
        public static readonly DependencyObject dependencyObject;
        public static readonly DependencyProperty StatusProperty = DependencyProperty.Register(
  "IsUserSignedOut",
  typeof(Boolean),
  typeof(MSGraphQueriesHelper),
  new PropertyMetadata(null, new PropertyChangedCallback(OnStatusChanged))
);
        // TODO silving falling down issue  
        private static void OnStatusChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            SigningStatusViewModel iwlc = (SigningStatusViewModel)d ; //null checks omitted
            bool s = (bool)e.NewValue ; //null checks omitted
            if (s == false)
            {
                iwlc.IsUserSignedOut = false;
            }
            else
            {
                iwlc.IsUserSignedOut = true;
            }
        }

        private bool _isUserSignedOut = MSGraphQueriesHelper.UserSignedOut;
        public bool IsUserSignedOut
        {
            get { /*return MSGraphQueriesHelper.UserSignedOut;*/return (bool)GetValue(StatusProperty); }
            set
            { /*base.SetProperty(ref _isUserSignedOut, MSGraphQueriesHelper.UserSignedOut);*/
                SetValue(StatusProperty,MSGraphQueriesHelper.UserSignedOut);
            }
        }

        public SigningStatusViewModel()
        {
            //RegisterPropertyChangedCallback(StatusProperty, IsUserSignedOut);
            IsUserSignedOut = MSGraphQueriesHelper.UserSignedOut;
        }

    }
}
