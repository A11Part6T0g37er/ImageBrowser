using System;
using System.Windows.Input;
using Windows.UI.Xaml;

namespace ImageBrowser.ViewModels
{
    internal class RelayMultipleCommand : ICommand
    {
        private readonly Func<bool> _canExecute;
        private Action _execute;
        public Action<object, double> GroupedGrid_SizeChanged { get; }


        /// <summary>
        /// Initializes a new instance of the <see cref="RelayCommand"/> class.
        /// Creates a new command.
        /// </summary>
        /// <param name="execute">The execution logic.</param>
        /// <param name="canExecute">The execution status logic.</param>
        public RelayMultipleCommand(Action execute, Func<bool> canExecute)
        {
            if (execute == null)
                throw new ArgumentNullException("execute");
            _execute = execute;
            _canExecute = canExecute;
        }

        public RelayMultipleCommand(Action<object, double> groupedGrid_SizeChanged)
        {
            GroupedGrid_SizeChanged = groupedGrid_SizeChanged;
        }


        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return _canExecute == null ? true : _canExecute();
        }
        public void Execute(object parameter)
        {
            if (parameter != null)
            {
                var p = parameter as SizeChangedEventArgs;
                var e = parameter as FrameworkElement;
                
                GroupedGrid_SizeChanged(e, e.ActualWidth);
            }
        }
        public void Execute(object sender, object parameter)
        {
            var p = parameter as SizeChangedEventArgs;
            var e = sender as FrameworkElement;
        }
    }
}