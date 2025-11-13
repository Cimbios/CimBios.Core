using System.ComponentModel;
using CimBios.Core.CimModel.CimDatatypeLib.EventUtils;
using CimBios.Core.CimModel.CimDatatypeLib.OID;
using CimBios.Core.CimModel.Schema;

namespace CimBios.Core.CimModel.CimDatatypeLib;

public class ReadOnlyModelObject : IReadOnlyModelObject
{
    private IReadOnlyModelObject ModelObject { get; }
    public IOIDDescriptor OID => ModelObject.OID;
    public ICimMetaClass MetaClass => ModelObject.MetaClass;

    public ReadOnlyModelObject (IReadOnlyModelObject modelObject)
    {
        ModelObject = modelObject;

        RouteEventsSetup();
    }

    private void RouteEventsSetup()
    {
        ModelObject.PropertyChanging += (_, args) 
            => PropertyChanging?.Invoke(this, args);
        
        ModelObject.PropertyChanged += (_, args) 
            => PropertyChanged?.Invoke(this, args);
    }

    public bool HasProperty(string propertyName)
    {
        return ModelObject.HasProperty(propertyName);
    }

    public void Shrink()
    {
        ModelObject.Shrink();
    }

    public object? GetAttribute(ICimMetaProperty metaProperty)
    {
        return ModelObject.GetAttribute(metaProperty);
    }

    public object? GetAttribute(string attributeName)
    {
        return ModelObject.GetAttribute(attributeName);
    }

    public T? GetAttribute<T>(ICimMetaProperty metaProperty)
    {
        return ModelObject.GetAttribute<T>(metaProperty);
    }

    public T? GetAttribute<T>(string attributeName)
    {
        return ModelObject.GetAttribute<T>(attributeName);
    }

    public T? GetAssoc1To1<T>(ICimMetaProperty metaProperty)
        where T : IModelObject
    {
        return ModelObject.GetAssoc1To1<T>(metaProperty);
    }

    public T? GetAssoc1To1<T>(string assocName) where T : IModelObject
    {
        return ModelObject.GetAssoc1To1<T>(assocName);
    }

    public IModelObject[] GetAssoc1ToM(ICimMetaProperty metaProperty)
    {
        return ModelObject.GetAssoc1ToM(metaProperty);
    }

    public IModelObject[] GetAssoc1ToM(string assocName)
    {
        return ModelObject.GetAssoc1ToM(assocName);
    }

    public T[] GetAssoc1ToM<T>(ICimMetaProperty metaProperty)
        where T : IModelObject
    {
        return ModelObject.GetAssoc1ToM<T>(metaProperty);
    }

    public T[] GetAssoc1ToM<T>(string assocName)
        where T : IModelObject
    {
        return ModelObject.GetAssoc1ToM<T>(assocName);
    }

    public IModelObject? GetAssoc1To1(ICimMetaProperty metaProperty)
    {
        return ModelObject.GetAssoc1To1(metaProperty);
    }

    public IModelObject? GetAssoc1To1(string assocName)
    {
        return ModelObject.GetAssoc1To1(assocName);
    }
    
    public event PropertyChangedEventHandler? PropertyChanged;
    public event CanCancelPropertyChangingEventHandler? PropertyChanging;
}

/// <summary>
///     Extension methods for IReadOnlyModelObject interface.
/// </summary>
public static class IReadOnlyModelObjectExtensions
{
    public static object? TryGetPropertyValue(
        this IReadOnlyModelObject modelObject,
        ICimMetaProperty metaProperty)
    {
        if (metaProperty.PropertyKind == CimMetaPropertyKind.Attribute) return modelObject.GetAttribute(metaProperty);

        if (metaProperty.PropertyKind == CimMetaPropertyKind.Assoc1To1)
            return modelObject.GetAssoc1To1<IModelObject>(metaProperty);

        if (metaProperty.PropertyKind == CimMetaPropertyKind.Assoc1ToM) return modelObject.GetAssoc1ToM(metaProperty);

        throw new NotSupportedException();
    }
}
