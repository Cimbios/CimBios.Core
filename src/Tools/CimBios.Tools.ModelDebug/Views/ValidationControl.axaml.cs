using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using CimBios.Tools.ModelDebug.Services;

namespace CimBios.Tools.ModelDebug.Views;

public partial class ValidationControl : UserControl
{
    public ValidationControl()
    {
        InitializeComponent();

        Services.ServiceLocator.GetInstance()
            .RegisterService(new ValidationService());
    }
}
