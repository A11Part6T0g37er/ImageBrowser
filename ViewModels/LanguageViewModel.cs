using ImageBrowser.LocalizationModels;
using System.Collections.ObjectModel;

namespace ImageBrowser.ViewModels
{
    class LanguageViewModel
    {
        private ObservableCollection<Language> _languages;

        public ObservableCollection<Language> Languages
        {
            get => _languages;
            set { _languages = value; }
        }

        public LanguageViewModel()
        {

            Languages = new ObservableCollection<Language>
                {
                    new Language { DisplayName = "English", LanguageCode = "en-US" },
                    new Language { DisplayName = "Deutschland", LanguageCode = "de-DE" }

                };
        }
    }
}
