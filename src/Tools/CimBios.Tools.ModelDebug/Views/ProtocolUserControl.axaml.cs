using Avalonia.Controls;
using CimBios.Tools.ModelDebug.ViewModels;

namespace CimBios.Tools.ModelDebug.Views;

public partial class ProtocolUserControl : UserControl
{
    public ProtocolUserControl()
    {
        DataContext = new ProtocolViewModel();
        InitializeComponent();
    }
}