using ImageBrowser.Helpers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace ImageBrowser.Common.RelayCommandProviders
{
    public sealed class CommandInstantiator
    {
        /// <summary>
        /// Imlemented switching between <see cref="ElementTheme"/> .
        /// </summary>
        /// <param name="obj">Object of button that is clicked and its tag property as a string.</param>	
        internal static void DefineClickedThemeExecute(ThemeSendingArgs obj)
        {
            ((obj.sender as Button).XamlRoot.Content as Frame).RequestedTheme = obj.Selection == "Default"
                    ? EnumHelper.GetEnum<ElementTheme>(GetDefaulTheme())
                    : EnumHelper.GetEnum<ElementTheme>(obj.Selection);
        }

        private static string GetDefaulTheme()
        {
            string defaultWinTheme = string.Empty;
            var DefaultTheme = new Windows.UI.ViewManagement.UISettings();
            var uiTheme = DefaultTheme.GetColorValue(Windows.UI.ViewManagement.UIColorType.Background).ToString();
            if (uiTheme == "#FF000000")
            {
                defaultWinTheme = "Dark";
            }
            else if (uiTheme == "#FFFFFFFF")
            {
                defaultWinTheme = "Light";
            }
            return defaultWinTheme;
        }
    }
}
