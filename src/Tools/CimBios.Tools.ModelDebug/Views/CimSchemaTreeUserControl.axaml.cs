using Avalonia.Controls;
using CimBios.Tools.ModelDebug.ViewModels;

namespace CimBios.Tools.ModelDebug.Views;

public partial class CimSchemaTreeUserControl : UserControl
{
    public CimSchemaTreeUserControl()
    {
        InitializeComponent();

        DataContext = new CimSchemaTreeViewModel();
    }
}