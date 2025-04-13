using Avalonia.Controls;
using CimBios.Tools.ModelDebug.Views;
using CommunityToolkit.Mvvm.Input;

namespace CimBios.Tools.ModelDebug.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    public RelayCommand ShowModelLoadDialogCommand { get; }
    public RelayCommand ShowModelSaveDialogCommand { get; }

    public Avalonia.Visual OwnerView { get; }

    public MainWindowViewModel(Avalonia.Visual ownerView)
    {
        OwnerView = ownerView;

        ShowModelLoadDialogCommand = new RelayCommand(ShowModelLoadDialog);
        ShowModelSaveDialogCommand = new RelayCommand(ShowModelSaveDialog);
    }

    private async void ShowModelLoadDialog()
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

        try
        {
            var model = CimDataModelProvider.LoadFromFile(
                dialog.ModelPath, dialog.SchemaPath, 
                dialog.DescriptorFactory, dialog.SchemaFactory, 
                dialog.RdfSerializerFactory, dialog.SerializerSettings,
                out var log
            );
        }
        catch
        {

        }
        finally
        {

        }

        return;
    }

    private async void ShowModelSaveDialog()
    {
        if (OwnerView is Window ownerWindow == false)
        {
            return;
        }

        await new CimModelOpenSaveWindow(CimModelOpenSaveWindow.DialogMode.Save)
            .ShowDialog(ownerWindow);

        // Make initialization.
        return;
    }
}
