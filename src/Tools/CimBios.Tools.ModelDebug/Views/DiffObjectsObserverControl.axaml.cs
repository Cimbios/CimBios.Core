using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using CimBios.Tools.ModelDebug.ViewModels;

namespace CimBios.Tools.ModelDebug.Views;

public partial class DiffObjectsObserverControl : UserControl
{
    public DiffObjectsObserverControl()
    {
        InitializeComponent();
        
        DataContext = new DiffObjectsViewModel(DiffsDataGrid, PropertyControl);
    }
}