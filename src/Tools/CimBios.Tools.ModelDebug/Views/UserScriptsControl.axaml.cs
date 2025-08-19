using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using CimBios.Tools.ModelDebug.ViewModels;

namespace CimBios.Tools.ModelDebug.Views;

public partial class UserScriptsControl : UserControl
{
    public UserScriptsControl()
    {
        InitializeComponent();

        DataContext = new UserScriptsViewModel();
    }
}