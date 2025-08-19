using Avalonia.Controls;
using CimBios.Tools.ModelDebug.ViewModels;

namespace CimBios.Tools.ModelDebug.Views;

public partial class CimObjectPropertiesObserver : UserControl
{
    public CimObjectPropertiesObserver()
    {
        InitializeComponent();

        DataContext = new CimObjectPropertiesObserverViewModel();
    }
}