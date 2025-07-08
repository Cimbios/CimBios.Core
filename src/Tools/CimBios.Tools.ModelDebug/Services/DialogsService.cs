using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using CimBios.Tools.ModelDebug.Views;

namespace CimBios.Tools.ModelDebug.Services;

public class DialogsService(Visual ownerView)
{
    private Visual OwnerView { get; } = ownerView;

    public async Task<CimModelOpenSaveResult?> ShowModelSaveLoadDialog(
        CimModelOpenSaveWindow.DialogMode dialogMode)
    {
        if (OwnerView is Window ownerWindow == false) return null;

        var dialog = new CimModelOpenSaveWindow(dialogMode);

        await dialog.ShowDialog(ownerWindow);

        return dialog.DialogState == false ? null : dialog.Result;
    }

    public async void ShowCreateObjectDialog()
    {
        if (OwnerView is Window ownerWindow == false) return;

        var dialog = new CimObjectCreatorDialog();

        await dialog.ShowDialog(ownerWindow);

        if ((dialog.DialogState ?? false) == false)
        {
        }
    }
}