using System;
using System.Windows.Input;

namespace ImageBrowser.Common
{
	/// <summary>
	/// A command whose sole purpose is to relay its functionality
	/// to other objects by invoking delegates.
	/// The default return value for the CanExecute method is 'true'.
	/// RaiseCanExecuteChanged needs to be called whenever
	/// CanExecute is expected to return a different value.
	/// </summary>
	public class RelayCommand: ICommand
	{
		private readonly Action execute;
		private readonly Action<object> command;
		private readonly Func<bool> canExecute;
		private Action<object, string> defineClickedTheme;

		/// <summary>
		/// Raised when RaiseCanExecuteChanged is called.
		/// </summary>
		public event EventHandler CanExecuteChanged;

		/// <summary>
		/// Initializes a new instance of the <see cref="RelayCommand"/> class.
		/// Creates a new command that can always execute.
		/// </summary>
		/// <param name="execute">The execution logic.</param>
		public RelayCommand(Action execute)
			: this(execute, null)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="RelayCommand"/> class.
		/// Creates a new command.
		/// </summary>
		/// <param name="execute">The execution logic.</param>
		/// <param name="canExecute">The execution status logic.</param>
		public RelayCommand(Action execute, Func<bool> canExecute)
		{
			if (execute == null)
				throw new ArgumentNullException("execute");
			this.execute = execute;
			this.canExecute = canExecute;
		}
		public RelayCommand(Action<object> commandAction, Func<bool> canExecute = null)
		{
			command = commandAction;
			this.canExecute = canExecute;
		}

		public RelayCommand(Action<object, string> defineClickedTheme)
		{
			this.defineClickedTheme = defineClickedTheme;
		}

		/// <summary>
		/// Determines whether this RelayCommand can execute in its current state.
		/// </summary>
		/// <param name="parameter">
		/// Data used by the command. If the command does not require data to be passed,
		/// this object can be set to null.
		/// </param>
		/// <returns>true if this command can be executed; otherwise, false.</returns>
		public bool CanExecute(object parameter)
		{
			return canExecute == null ? true : canExecute();
		}

		/// <summary>
		/// Executes the RelayCommand on the current command target.
		/// </summary>
		/// <param name="parameter">
		/// Data used by the command. If the command does not require data to be passed,
		/// this object can be set to null.
		/// </param>
		public void Execute(object parameter)
		{
			if (command != null)
			{
				command(parameter);
			}
			if (execute != null)
			{

				execute();
			}
			if (defineClickedTheme != null)
			{
				var p = parameter as Windows.UI.Xaml.FrameworkElement;
				defineClickedTheme(parameter, p?.Tag.ToString());
			}
		}

		/// <summary>
		/// Method used to raise the CanExecuteChanged event
		/// to indicate that the return value of the CanExecute
		/// method has changed.
		/// </summary>
		public void RaiseCanExecuteChanged()
		{
			var handler = CanExecuteChanged;
			if (handler != null)
			{
				handler(this, EventArgs.Empty);
			}
		}
	}
}
