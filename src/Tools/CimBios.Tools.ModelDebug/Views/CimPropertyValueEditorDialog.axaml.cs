using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Data;
using CimBios.Core.CimModel.CimDatatypeLib.EventUtils;
using CimBios.Core.CimModel.Schema;
using CimBios.Tools.ModelDebug.Models;

namespace CimBios.Tools.ModelDebug.Views;

public partial class CimPropertyValueEditorDialog 
    : Window, IDialog 
{
    public CimPropertyValueEditorDialog()
    {
        InitializeComponent();
    }

    public bool? DialogState { get; private set; }

    public IDialogResult Result 
        => new CimPropertyValueEditorDialogResult(
            DialogState ?? false, PropertyValue);
    
    public CimObjectPropertyModel? EditObject { get; private set; }

    public string PropertyValue { get; set; } = string.Empty;
    
    public Task Show(Window owner, params object[]? args)
    {
        if (args is { Length: > 0 } && args[0] is CimObjectPropertyModel model)
        {
            EditObject = model;

            FillPropertyValue();
        }
        
        return this.ShowDialog(owner);
    }

    private void FillPropertyValue()
    {
        if (EditObject is null) return;

        switch (EditObject.MetaProperty.PropertyKind)
        {
            case CimMetaPropertyKind.Attribute:
                PropertyValue = EditObject.ModelObject
                    .GetAttribute(EditObject.MetaProperty)?
                    .ToString() ?? string.Empty;
                break;
            case CimMetaPropertyKind.Assoc1To1:
                PropertyValue = EditObject.ModelObject
                    .GetAssoc1To1(EditObject.MetaProperty)?
                    .OID.ToString() ?? string.Empty;
                break;
            case CimMetaPropertyKind.Assoc1ToM:
                PropertyValue = string.Join(";", EditObject.ModelObject
                    .GetAssoc1ToM(EditObject.MetaProperty)
                    .Select(o => o.OID.ToString()));
                break;
        }
        
        UpdateData();
    }
    
    private void UpdateData()
    {
        BindingOperations.GetBindingExpressionBase(
            OidTextBlock, TextBlock.TextProperty)?.UpdateTarget();
            
        BindingOperations.GetBindingExpressionBase(
            PropertyUriTextBlock, TextBlock.TextProperty)?.UpdateTarget();
        
        BindingOperations.GetBindingExpressionBase(
            PropertyValueTextBlock, TextBlock.TextProperty)?.UpdateTarget();
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

public class CimPropertyValueEditorDialogResult(bool succeed, string value)
    : IDialogResult
{
    public bool Succeed => succeed;
    
    public string Value => value;
}
