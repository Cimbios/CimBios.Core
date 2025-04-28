using System;
using System.Linq;
using Avalonia.Controls;
using CimBios.Tools.ModelDebug.Views;
using CimBios.Utils.ClassTraits.CanLog;

namespace CimBios.Tools.ModelDebug.Services;

public class DialogsService
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

    public async void ShowModelLoadDialog()
    {
        if (OwnerView is Window ownerWindow == false)
        {
            return;
        }

        var dialog = new CimModelOpenSaveWindow(
            CimModelOpenSaveWindow.DialogMode.Load);

        await dialog.ShowDialog(ownerWindow);

        if (dialog.DialogState == false)
        {
            return;
        }

        ILog? log = null;
        try
        {
            if (Services.ServiceLocator.GetInstance()
                .TryGetService<CimModelLoaderService>(out var loaderService) == false
                || loaderService == null)
            {
                throw new NotSupportedException(
                    "Loader service has not been initiaized!");
            }

            loaderService.LoadFromFile(
                dialog.ModelPath, dialog.SchemaPath, 
                dialog.DescriptorFactory, dialog.SchemaFactory, 
                dialog.RdfSerializerFactory, dialog.SerializerSettings,
                out log
            );
        }
        catch (Exception ex)
        {
            _ProtocolService.Error($"Loading CIM failed: {ex.Message}", "");
        }
        finally
        {
            if (log != null)
            {
                var groupDescriptor = new GroupDescriptor(
                    $"Load CIM model {dialog.ModelPath}");

                foreach (var logMessage in log.Messages
                    .Select(m => CanLogMessagesConverter
                        .Convert(m, groupDescriptor)))
                {
                    _ProtocolService.AddMessage(logMessage);
                }
            }
        }

        return;
    } 
}
