using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace ImageBrowser
{
	public class AppSettings : INotifyPropertyChanged
	{
		public bool ShowTimeSheetSetting
		{
			get
			{
				return ReadSettings(nameof(ShowTimeSheetSetting), true);
			}
			set
			{
				SaveSettings(nameof(ShowTimeSheetSetting), value);
				NotifyPropertyChanged();
			}
		}

		public ApplicationDataContainer LocalSettings { get; set; }

		public AppSettings()
		{
			LocalSettings = ApplicationData.Current.LocalSettings;
		}

		private void SaveSettings(string key, object value)
		{
			LocalSettings.Values[key] = value;
		}

		private T ReadSettings<T>(string key, T defaultValue)
		{
			return LocalSettings.Values.ContainsKey(key) ? (T)LocalSettings.Values[key] : null != defaultValue ? defaultValue : default(T);
		}

		public event PropertyChangedEventHandler PropertyChanged;

		protected void NotifyPropertyChanged([CallerMemberName] string propName = "")
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
		}
	}
}
