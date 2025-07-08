using Avalonia.Controls;
using CimBios.Tools.ModelDebug.Services;
using CimBios.Tools.ModelDebug.ViewModels;

namespace CimBios.Tools.ModelDebug.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        ServiceLocator.GetInstance()
            .RegisterService(new DialogsService(this));

        DataContext = new MainWindowViewModel(this);
        InitializeComponent();
    }
}