using System;
using Avalonia;
using CommunityToolkit.Mvvm.Input;

namespace CimBios.Tools.ModelDebug.ViewModels;

public class MainWindowViewModel(Visual ownerView) : ViewModelBase
{
    public Visual OwnerView { get; } = ownerView;

    public RelayCommand SaveLocalDiffs { get; } = new(() =>
    {
        GlobalServices.LoaderService
            .SaveLocalDifferencesToFile($"ld-{DateTime.Now.ToFileTime()}.xml");
    });
}