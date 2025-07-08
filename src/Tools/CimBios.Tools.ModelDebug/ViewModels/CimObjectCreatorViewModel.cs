using Avalonia;
using Avalonia.Controls;

namespace CimBios.Tools.ModelDebug.ViewModels;

public class CimObjectCreatorViewModel : ViewModelBase
{
    public CimObjectCreatorViewModel(Window parentWindow)
    {
        OwnerView = parentWindow;
    }

    private Visual OwnerView { get; }

    public bool? DialogState { get; private set; }

    public void Ok()
    {
        DialogState = true;

        if (OwnerView is Window ownerWindow) ownerWindow.Close();
    }

    public void Cancel()
    {
        DialogState = false;

        if (OwnerView is Window ownerWindow) ownerWindow.Close();
    }
}