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
    }
}
