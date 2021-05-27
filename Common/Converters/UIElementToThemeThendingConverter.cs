using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace ImageBrowser.Common.Converters
{
	public class UIElementToThemeThendingConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, string language)
		{
			ThemeSendingArgs sendingArgs = null;
			if (value != null)
			{
				var p = value as Windows.UI.Xaml.FrameworkElement;
				string Theme =  p?.Tag.ToString();
				sendingArgs = new ThemeSendingArgs() { sender = p, Selection = Theme };
			}
			return sendingArgs;
		}

		public object ConvertBack(object value, Type targetType, object parameter, string language)
		{
			throw new NotImplementedException();
		}
	}
}
