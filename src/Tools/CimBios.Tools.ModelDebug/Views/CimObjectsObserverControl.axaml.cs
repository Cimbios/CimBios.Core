using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
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