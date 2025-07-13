using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using CimBios.Core.CimModel.CimDatatypeLib.OID;
using CimBios.Core.CimModel.Schema;
using CimBios.Tools.ModelDebug.ViewModels;

namespace CimBios.Tools.ModelDebug.Views;

public partial class CimObjectCreatorDialog : Window, IDialog
{
    protected CimObjectCreatorViewModel _Model;

    public CimObjectCreatorDialog()
    {
        _Model = new CimObjectCreatorViewModel(this);

        DataContext = _Model;

        InitializeComponent();
    }

    public bool? DialogState => _Model.DialogState;

    public IDialogResult Result => new CimObjectCreatorResult(_Model);

    public Task Show(Window owner, params object[]? args)
    {
        return this.ShowDialog(owner);
    }
}

public class CimObjectCreatorResult(CimObjectCreatorViewModel model) 
    : IDialogResult
{
    public ICimMetaClass MetaClass { get; } = model.MetaClass
        ?? throw new InvalidOperationException("No selected meta class!");

    public IOIDDescriptor Descriptor { get; } = model.Descriptor
        ?? throw new InvalidOperationException("No selected OID descriptor!");

    public bool Succeed => model.DialogState ?? false;
}