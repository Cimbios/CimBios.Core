using Avalonia.Controls;
using CimBios.Tools.ModelDebug.ViewModels;

namespace CimBios.Tools.ModelDebug.Views;

public partial class CimObjectsObserverControl : UserControl
{
    public CimObjectsObserverControl()
    {
        InitializeComponent();

        var dataGrid = this.FindControl<TreeDataGrid>("dataGrid");
        DataContext = new CimObjectsObserverViewModel(dataGrid);
    }
}