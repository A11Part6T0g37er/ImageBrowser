using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace ImageBrowser.Common.Converters
{
	public class BooleanToVisibilityConverter : IValueConverter
	{
		public Visibility OnTrue { get; set; }

		public Visibility OnFalse { get; set; }

		public BooleanToVisibilityConverter()
		{
			OnFalse = Visibility.Collapsed;
			OnTrue = Visibility.Visible;
		}

		public object Convert(object value, Type targetType, object parameter, string language)
		{
			bool? visibile = null;
			if (value is bool)
			{

				visibile = (bool)value;


			}
			return (bool)visibile ? OnTrue : OnFalse;
		}

		public object ConvertBack(object value, Type targetType, object parameter, string language)
		{
			if (value is Visibility == false)
			{
				return DependencyProperty.UnsetValue;
			}


			return ((Visibility)value == OnTrue);

		}
	}
}
