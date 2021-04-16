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
       public  bool IsUserSignedOut
        {
            get { return MSGraphQueriesHelper.UserSignedOut; OnPropertyChanged("IsUserSignedOut"); }
            set { base.SetProperty(ref MSGraphQueriesHelper.UserSignedOut, value); }
        }
    }
}
