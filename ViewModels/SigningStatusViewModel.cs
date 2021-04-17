using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using ImageBrowser.Helpers;
using System.Threading.Tasks;

namespace ImageBrowser.ViewModels
{
    public class SigningStatusViewModel : BindableBase
    {

        private bool _isUserSignedOut = MSGraphQueriesHelper.UserSignedOut;
       public  bool IsUserSignedOut
        {
            get { return MSGraphQueriesHelper.UserSignedOut;  }
            set { base.SetProperty(ref _isUserSignedOut, MSGraphQueriesHelper.UserSignedOut); }
        }

        public SigningStatusViewModel()
        {
            IsUserSignedOut = MSGraphQueriesHelper.UserSignedOut;
        }
    }
}
