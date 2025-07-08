using System;
using CimBios.Tools.ModelDebug.Services;

namespace CimBios.Tools.ModelDebug.ViewModels;

public static class GlobalServices
{
    public static ProtocolService ProtocolService
    {
        get
        {
            if (ServiceLocator.GetInstance().TryGetService<ProtocolService>(
                    out var protocolService) == false || protocolService == null)
                throw new NotSupportedException(
                    "Protocol service has not been initialized!");

            return protocolService;
        }
    }

    public static CimModelLoaderService LoaderService
    {
        get
        {
            if (ServiceLocator.GetInstance().TryGetService<CimModelLoaderService>(
                    out var loaderService) == false || loaderService == null)
                throw new NotSupportedException(
                    "Loader service has not been initialized!");

            return loaderService;
        }
    }

    public static DialogsService DialogService
    {
        get
        {
            if (ServiceLocator.GetInstance().TryGetService<DialogsService>(
                    out var dialogService) == false || dialogService == null)
                throw new NotSupportedException(
                    "Dialog service has not been initialized!");

            return dialogService;
        }
    }

    public static NavigationService NavigationService
    {
        get
        {
            if (ServiceLocator.GetInstance().TryGetService<NavigationService>(
                    out var navigationService) == false || navigationService == null)
                throw new NotSupportedException(
                    "Navigation service has not been initialized!");

            return navigationService;
        }
    }
}