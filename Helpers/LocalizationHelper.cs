using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageBrowser.Helpers
{
    class LocalizationHelper
    {
        public static string GetLocalizedStrings(string resourceName)
        {
            if (Windows.UI.Core.CoreWindow.GetForCurrentThread() != null)
            {
                var resourceLoader = Windows.ApplicationModel.Resources.ResourceLoader.GetForCurrentView();
                return resourceLoader.GetString(resourceName).ToString();
            }
            return "";
        }
    }
}
