using System;
using CimBios.Tools.ModelDebug.Services;

namespace CimBios.Tools.ModelDebug.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    public Avalonia.Visual OwnerView { get; }

    private ProtocolService _ProtocolService  
    {
        get
        {
            if (ServiceLocator.GetInstance().TryGetService<ProtocolService>(
                out var protocolService) == false || protocolService == null)
            {
                throw new NotSupportedException(
                    "Protocol service has not been initialized!");
            }

            return protocolService;
        }
    }

    public MainWindowViewModel(Avalonia.Visual ownerView)
    {
        OwnerView = ownerView;
    }
}
