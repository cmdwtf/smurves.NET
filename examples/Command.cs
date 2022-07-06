using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace cmdwtf.Smurves.Example;

class Command<T> : ICommand
{
	public event EventHandler? CanExecuteChanged;

	private readonly Action<T?> _execute;
	private readonly Func<T?, bool> _canExecute;

	public bool CanExecute(object? parameter) => _canExecute((T?)parameter);
	public void Execute(object? parameter) => _execute((T?)parameter);

	public Command(Action<T?> execute, Func<T?, bool> canExecute)
	{
		_execute = execute;
		_canExecute = canExecute;
	}

	public Command(Action<T?> execute) : this(execute, o => true)
	{ }

	public Command(Action execute, Func<bool> canExecute)
	{
		_execute = o => execute();
		_canExecute = o => canExecute();
	}

	public Command(Action execute) : this(execute, () => true)
	{

	}
}
