using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using CimBios.Tools.ModelDebug.Views;
using CommunityToolkit.Mvvm.Input;

namespace CimBios.Tools.ModelDebug.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    public AsyncRelayCommand ShowDataProviderSelectorCommand { get; }

    private Window? MainWindow 
    { 
        get
        {
            if (Application.Current?.ApplicationLifetime 
                is IClassicDesktopStyleApplicationLifetime desktop)
            {
                return desktop.MainWindow;
            }

            return null;
        }
    }

    public MainWindowViewModel()
    {
        ShowDataProviderSelectorCommand = 
            new AsyncRelayCommand(ShowDataProviderSelectorWindow);
    }

    private Task ShowDataProviderSelectorWindow()
    {
        if (MainWindow == null)
        {
            return Task.CompletedTask;
        }

        var dpsWindow = new DataSelectorWindow()
        {
            DataContext = new DataSelectorViewModel()
        };
        dpsWindow.ShowDialog(MainWindow);

        return Task.CompletedTask;
    }
}
