using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using CimBios.Tools.ModelDebug.ViewModels;

namespace CimBios.Tools.ModelDebug.Views;

public partial class CimSchemaTreeUserControl : UserControl
{
    public CimSchemaTreeUserControl()
    {
        InitializeComponent();

        this.DataContext = new CimSchemaTreeViewModel();
    }
}