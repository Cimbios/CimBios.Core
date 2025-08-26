using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using CimBios.Core.CimModel.CimDatatypeLib;
using CimBios.Tools.ModelDebug.ViewModels;

namespace CimBios.Tools.ModelDebug.Views;

public partial class PropertyDiffsObserverControl : UserControl
{
    public IDifferenceObject? SelectedDiff
    {
        get => _difference;
        set
        {
            if (_difference == value)
            {
                return;
            }
            
            _difference = value;

            if (_difference != null) 
                ViewModel.InitializeData(_difference);
            else 
                ViewModel.Clear();
        }
    }
    
    private PropertyDiffsObserverViewModel ViewModel { get; }
    
    private IDifferenceObject? _difference = null;

    public PropertyDiffsObserverControl()
    {
        InitializeComponent();
        
        ViewModel = new PropertyDiffsObserverViewModel();
        DataContext = ViewModel;
    }
}