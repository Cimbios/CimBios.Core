using System;
using Avalonia;
using CommunityToolkit.Mvvm.Input;
using CimBios.Core.CimModel.CimDatatypeLib.CIM17Types;
using Avalonia.Media.TextFormatting.Unicode;
using System.Reflection.Metadata;
using Tmds.DBus.Protocol;

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