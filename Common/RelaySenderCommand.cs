using System;
using System.Windows.Input;
using Windows.UI.Xaml;

namespace ImageBrowser.Common
{
	internal class RelaySenderCommand : ICommand
	{
		private readonly Func<bool> canExecute;
		private Action execute;
		public Action<object, string> DefineTheme { get; }


		/// <summary>
		/// Initializes a new instance of the <see cref="RelaySenderCommand"/> class.
		/// Creates a new command.
		/// </summary>
		/// <param name="execute">The execution logic.</param>
		/// <param name="canExecute">The execution status logic.</param>
		public RelaySenderCommand(Action execute, Func<bool> canExecute)
		{
			if (execute == null)
				throw new ArgumentNullException("execute");
			this.execute = execute;
			this.canExecute = canExecute;
		}

		public RelaySenderCommand(Action<object, string> defineTheme)
		{
			DefineTheme = defineTheme;
		}


		public event EventHandler CanExecuteChanged;

		public bool CanExecute(object parameter)
		{
			return canExecute == null ? true : canExecute();
		}
		public void Execute(object parameter)
		{
			if (parameter != null)
			{
				var p = parameter as Windows.UI.Xaml.FrameworkElement;
				DefineTheme(parameter, p?.Tag.ToString());
			}
		}
		public void Execute(object sender, string parameter)
		{
			var p = parameter;
			//var e = sender as UIElement;
			DefineTheme(sender, p);
		}
	}
}