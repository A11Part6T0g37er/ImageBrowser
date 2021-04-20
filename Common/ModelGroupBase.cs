using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageBrowser.Models
{
    using System.Collections.ObjectModel;
    using System.ComponentModel;

    /// <summary>
    /// For use with XAML controls showing grouped collections.
    /// </summary>
    /// <typeparam name="T">Type of data item.</typeparam>
    public class ModelGroupBase<T> : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public ModelGroupBase()
        {
            Items = new ObservableCollection<T>();
            Title = string.Empty;
            HasGroupDetails = false;

            Items.CollectionChanged += (sender, e) =>
            {
                HasGroupDetails = Items.Count > 0;
                OnStateChanged(nameof(HasGroupDetails));
            };
        }

        public ObservableCollection<T> Items { get; set; }

        public string Title { get; set; }

        public bool HasGroupDetails { get; set; }

        private void OnStateChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public override string ToString()
        {
            return Title;
        }
    }
}

