using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

using OxyPlot;


namespace cmdwtf.Smurves.Example;


/// <summary>
/// Represents the view-model for the main window.
/// </summary>
public class MainViewModel : INotifyPropertyChanged
{
	/// <summary>
	/// Initializes a new instance of the <see cref="MainViewModel" /> class.
	/// </summary>
	public MainViewModel()
	{
		//_activeModel = new TestModel();
		//_activeModel = new SmurvesModel(true, true);

		GenerateCurvesCommand = new Command<bool>(log =>
		{
			GenerateNewCurves(log);
		});
	}

	private IModel? _activeModel;

	/// <summary>
	/// Gets the plot model.
	/// </summary>
	public PlotModel Model => _activeModel?.Model ?? new PlotModel();

	private bool _showRejects = true;
	public bool ShowRejects
	{
		get => _showRejects;
		set => SetProperty(ref _showRejects, value);
	}

	private bool _logarithmicXAxis = true;
	public bool LogarithmicXAxis
	{
		get => _logarithmicXAxis;
		set => SetProperty(ref _logarithmicXAxis, value);
	}

	public ICommand GenerateCurvesCommand { protected set; get; }

	internal void GenerateNewCurves(bool doLogarithmic)
	{
		_activeModel = new SurgeBinderModel(doLogarithmic, LogarithmicXAxis, ShowRejects);
		OnPropertyChanged(nameof(Model));
	}

	public event PropertyChangedEventHandler? PropertyChanged;

	protected virtual bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string? propertyName = null)
	{
		if (Equals(storage, value))
		{
			return false;
		}

		storage = value;

		OnPropertyChanged(propertyName);
		return true;
	}

	protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}
}
