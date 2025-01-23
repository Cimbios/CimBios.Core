using System.ComponentModel;
using System.Dynamic;
using CimBios.Core.CimModel.CimDatatypeLib;
using CimBios.Core.CimModel.Schema;

namespace CimBios.Core.CimModel.DatatypeLib;

public abstract class DynamicModelObjectBase : DynamicObject, IModelObject
{
    public dynamic? AsDynamic()
    {
        return this;
    }

    public override bool TryGetMember(GetMemberBinder binder, out object? result)
    {
        var metaProperty = TryGetMetaPropertyByName(binder.Name);
        if (metaProperty != null)
        {
            if (metaProperty.PropertyKind == CimMetaPropertyKind.Attribute)
            {
                result = GetAttribute(metaProperty);
                return true;
            }
            else if (metaProperty.PropertyKind == CimMetaPropertyKind.Assoc1To1)
            {
                result = GetAssoc1To1<IModelObject>(metaProperty);
                return true;        
            }
            else if (metaProperty.PropertyKind == CimMetaPropertyKind.Assoc1ToM)
            {
                result = GetAssoc1ToM(metaProperty);
                return true;        
            }         
        }

        return base.TryGetMember(binder, out result);
    }

    public override bool TrySetMember(SetMemberBinder binder, object? value)
    {
        var metaProperty = TryGetMetaPropertyByName(binder.Name);
        if (metaProperty != null)
        {
            if (metaProperty.PropertyKind == CimMetaPropertyKind.Attribute)
            {
                SetAttribute(metaProperty, value);
                return true;
            }
            else if (metaProperty.PropertyKind == CimMetaPropertyKind.Assoc1To1)
            {
                SetAssoc1To1(metaProperty, value as IModelObject);
                return true;        
            }      
        }

        return base.TrySetMember(binder, value);
    }

    protected ICimMetaProperty? TryGetMetaPropertyByName(string name)
    {
        var splitted = name.Split('.');
        var isClassPropForm = splitted.Length.Equals(2);

        foreach (var property in MetaClass.AllProperties)
        {
            var propCPForm = $"{property.OwnerClass?.ShortName}.{property.ShortName}";

            if ((isClassPropForm && propCPForm == name)
                || (property.ShortName == name))
            {
                return property;
            }
        }

        return null;
    }

    public abstract string Uuid { get; }

    public abstract ICimMetaClass MetaClass { get; }

    public abstract bool IsAuto { get; }

    public abstract event PropertyChangedEventHandler? PropertyChanged;

    public abstract void AddAssoc1ToM(ICimMetaProperty metaProperty, 
        IModelObject obj);

    public abstract void AddAssoc1ToM(string assocName, IModelObject obj);

    public abstract T? GetAssoc1To1<T>(ICimMetaProperty metaProperty) 
        where T : IModelObject;

    public abstract T? GetAssoc1To1<T>(string assocName) where T : IModelObject;

    public abstract IModelObject[] GetAssoc1ToM(ICimMetaProperty metaProperty);

    public abstract IModelObject[] GetAssoc1ToM(string assocName);

    public abstract T[] GetAssoc1ToM<T>(ICimMetaProperty metaProperty) 
        where T : IModelObject;

    public abstract T[] GetAssoc1ToM<T>(string assocName) 
        where T : IModelObject;

    public abstract object? GetAttribute(ICimMetaProperty metaProperty);

    public abstract object? GetAttribute(string attributeName);

    public abstract T? GetAttribute<T>(ICimMetaProperty metaProperty);

    public abstract T? GetAttribute<T>(string attributeName);

    public abstract bool HasProperty(string propertyName);

    public abstract void RemoveAllAssocs1ToM(ICimMetaProperty metaProperty);

    public abstract void RemoveAllAssocs1ToM(string assocName);

    public abstract void RemoveAssoc1ToM(ICimMetaProperty metaProperty, 
        IModelObject obj);

    public abstract void RemoveAssoc1ToM(string assocName,
        IModelObject obj);

    public abstract void SetAssoc1To1(ICimMetaProperty metaProperty, 
        IModelObject? obj);

    public abstract void SetAssoc1To1(string assocName, IModelObject? obj);

    public abstract void SetAttribute<T>(ICimMetaProperty metaProperty, T? value);

    public abstract void SetAttribute<T>(string attributeName, T? value);
}
