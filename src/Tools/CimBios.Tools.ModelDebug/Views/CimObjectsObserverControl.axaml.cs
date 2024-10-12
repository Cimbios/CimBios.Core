using Avalonia.Controls;
using CimBios.Tools.ModelDebug.ViewModels;

namespace CimBios.Tools.ModelDebug.Views;

public partial class CimObjectsObserverControl : UserControl
{
    public CimObjectsObserverControl()
    {
        InitializeComponent();

        this.DataContext = new CimObjectsObserverViewModel();
    }
}