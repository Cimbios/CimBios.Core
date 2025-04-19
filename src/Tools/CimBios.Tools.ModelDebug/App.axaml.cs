using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using CimBios.Tools.ModelDebug.Services;
using CimBios.Tools.ModelDebug.Views;

namespace CimBios.Tools.ModelDebug;

public partial class App : Application
{
    public override void Initialize()
    {
         Services.ServiceLocator.GetInstance()
            .RegisterService(new CimModelLoaderService()); 

         Services.ServiceLocator.GetInstance()
            .RegisterService(new ProtocolService()); 

        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Line below is needed to remove Avalonia data validation.
            // Without this line you will get duplicate validations from both Avalonia and CT
            BindingPlugins.DataValidators.RemoveAt(0);

            var mainWindow = new MainWindow();
            desktop.MainWindow = mainWindow;
        }

        base.OnFrameworkInitializationCompleted();
    }
}