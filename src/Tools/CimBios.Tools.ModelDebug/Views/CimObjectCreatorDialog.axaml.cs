using Avalonia.Controls;
using CimBios.Core.CimModel.CimDatatypeLib.OID;
using CimBios.Core.CimModel.Schema;
using CimBios.Tools.ModelDebug.ViewModels;

namespace CimBios.Tools.ModelDebug.Views;

public partial class CimObjectCreatorDialog : Window
{
    protected CimObjectCreatorViewModel _Model;

    public CimObjectCreatorDialog()
    {
        _Model = new CimObjectCreatorViewModel(this);

        DataContext = _Model;

        InitializeComponent();
    }
    //public CimObjectCreatorResult => new()

    public bool? DialogState => _Model.DialogState;
}

public class CimObjectCreatorResult(
    ICimMetaClass metaClass,
    IOIDDescriptor descriptor)
{
    public ICimMetaClass MetaClass { get; } = metaClass;

    public IOIDDescriptor Descriptor { get; } = descriptor;
}