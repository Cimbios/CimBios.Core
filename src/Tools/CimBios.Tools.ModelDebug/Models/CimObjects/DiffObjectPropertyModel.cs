using System.Linq;
using CimBios.Core.CimModel.CimDatatypeLib;
using CimBios.Core.CimModel.Schema;

namespace CimBios.Tools.ModelDebug.Models.CimObjects;

public class DiffObjectPropertyModel
    : TreeViewNodeModel
{
    public ICimMetaProperty MetaProperty { get; }
    public string OldValue => GetStringValue(_originalObject);
    public string NewValue => GetStringValue(_modifiedObject);
    
    private readonly IReadOnlyModelObject? _originalObject;
    private readonly IReadOnlyModelObject _modifiedObject;

    public DiffObjectPropertyModel(IReadOnlyModelObject? originalObject,
        IReadOnlyModelObject modifiedObject,
        ICimMetaProperty metaProperty)
    {
        _originalObject = originalObject;
        _modifiedObject = modifiedObject;
        
        MetaProperty = metaProperty;
        
        InitChildPropertyNodes();
    }

    public DiffObjectPropertyModel(IDifferenceObject differenceObject,
        ICimMetaProperty metaProperty) 
            : this(differenceObject.OriginalObject, 
                differenceObject.ModifiedObject, metaProperty)
    {
    }

    private void InitChildPropertyNodes()
    {
        if (MetaProperty.PropertyKind == CimMetaPropertyKind.Attribute
            && (MetaProperty.PropertyDatatype?.IsCompound ?? false))
        {
            var oldCompound = _originalObject?.GetAttribute<IModelObject>(MetaProperty);
            var newCompound = _modifiedObject.GetAttribute<IModelObject>(MetaProperty);
            if (newCompound is null) return;

            foreach (var property in newCompound.MetaClass.AllProperties)
            {
                AddChild(new DiffObjectPropertyModel(
                    oldCompound, newCompound, property));
            }
        }
    }

    private string GetStringValue(IReadOnlyModelObject? modelObject)
    {
        if (modelObject is null) return "<null>";
        
        switch (MetaProperty.PropertyKind)
        {
            case CimMetaPropertyKind.Attribute:
            {
                var value = modelObject.GetAttribute(MetaProperty);
                if (value is IModelObject compound)
                    return $"{compound.MetaClass.ShortName} : {compound.OID}";
            
                return value?.ToString() ?? "<null>";
            }
            case CimMetaPropertyKind.Assoc1To1:
            {
                var ref11 = modelObject.GetAssoc1To1(MetaProperty);
                return ref11 is null ? "<null>" : ref11.OID.ToString();
            }
            case CimMetaPropertyKind.Assoc1ToM:
            {
                var ref1M = modelObject.GetAssoc1ToM(MetaProperty);
                return ref1M.Length == 0 
                    ? "<null>" : string.Join("\n", 
                        ref1M.Select(r => r.OID.ToString()));
            }
            default:
                return "<null>";
        }
    }
}