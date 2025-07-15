using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using CimBios.Core.CimModel.CimDatatypeLib.EventUtils;

namespace CimBios.Tools.ModelDebug.Views;

public partial class CimPropertyValueEditorDialog : Window, IDialog
{
    public CimPropertyValueEditorDialog()
    {
        InitializeComponent();
    }

    public bool? DialogState { get; private set; }

    public IDialogResult Result => throw new System.NotImplementedException();

    public int SelectedOperationId { get; set; } = 0;

    public Task Show(Window owner, params object[]? args)
    {
        return this.ShowDialog(owner);
    }

    public void Ok()
    {
        DialogState = true;

        this.Close();
    }

    public void Cancel()
    {
        DialogState = false;

        this.Close();
    }
}

public class CimPropertyValueEditorDialogResult(bool succeed,
    CimMetaPropertyChangedEventArgs changedEventArgs)
    : IDialogResult
{
    public bool Succeed => succeed;
    public CimMetaPropertyChangedEventArgs ChangedArgs => changedEventArgs;
}
