using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;

namespace ImageBrowser.Helpers
{
    public static class CallUIThreadHelper
    {
        /// <summary>
        /// Runs the specified handler on the UI thread at Normal priority.
        /// </summary>
        public static async Task CallOnUiThreadAsync(DispatchedHandler handler) => await
            CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, handler);
    }
}
