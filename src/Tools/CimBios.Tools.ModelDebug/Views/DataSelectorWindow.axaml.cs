using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using CimBios.Tools.ModelDebug.ViewModels;

namespace CimBios.Tools.ModelDebug.Views;

public partial class DataSelectorWindow : Window
{
    public DataSelectorWindow()
    {
        DataContext = new DataSelectorViewModel(this);
        InitializeComponent();
    }
}