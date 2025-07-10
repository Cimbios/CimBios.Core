using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using CimBios.Tools.ModelDebug.Views;

namespace CimBios.Tools.ModelDebug.Services;

public class DialogsService(Visual ownerView)
{
    private Visual OwnerView { get; } = ownerView;

    public async Task<IDialogResult> ShowDialog<D>(params object[]? args)
        where D : IDialog
    {
        if (OwnerView is Window ownerWindow == false)
            return new FailedDialogResult();

        var dialog = Activator.CreateInstance<D>();

        await dialog.Show(ownerWindow, args);

        return dialog.Result;
    }

    public async Task<IDialogResult> ShowDialog<D>()
        where D : IDialog
    {
        return await ShowDialog<D>(null);
    }
}