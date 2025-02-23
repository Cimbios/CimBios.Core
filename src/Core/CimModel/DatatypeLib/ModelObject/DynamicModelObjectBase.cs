using System.ComponentModel;
using System.Dynamic;
using CimBios.Core.CimModel.Schema;
using CimBios.Core.CimModel.CimDatatypeLib.EventUtils;

namespace CimBios.Core.CimModel.CimDatatypeLib;

/// <summary>
/// Provides dynamo object functionality.
/// </summary>
public abstract class DynamicModelObjectBase : DynamicObject, IModelObject
{
    protected DynamicModelObjectBase() : base() {}

    public dynamic? AsDynamic()
    {
        return this;
    }

    public override bool TryGetMember(GetMemberBinder binder, out object? result)
    {
        var metaProperty = TryGetMetaPropertyByName(binder.Name);
        if (metaProperty != null)
        {
            var propValue = this.TryGetPropertyValue(metaProperty);
            result = propValue;

            if (metaProperty.PropertyKind == CimMetaPropertyKind.Assoc1ToM
                && propValue is IList<IModelObject> assocCollection)
            {                
                result = BindDynamicAssocsCollection(metaProperty, assocCollection);
            }

            return true;
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

    public abstract string OID { get; }

    public abstract ICimMetaClass MetaClass { get; }

    public abstract bool IsAuto { get; }

    public event PropertyChangedEventHandler? PropertyChanged;
    public event CanCancelPropertyChangingEventHandler? PropertyChanging;

    public virtual IReadOnlyModelObject AsReadOnly()
    {
        return new ReadOnlyModelObject(this);
    }

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

    public virtual void OnPropertyChanged(CimMetaPropertyChangedEventArgs args)
    {
        PropertyChanged?.Invoke(this, args);
    }

    public virtual void OnPropertyChanging(CanCancelPropertyChangingEventArgs args)
    {
        PropertyChanging?.Invoke(this, args);
    }    

    protected bool CanChangeProperty(ICimMetaProperty metaProperty, 
        object? newValue, bool? isRemove = null)
    {
        if (PropertyChanging != null)
        {
            CanCancelPropertyChangingEventArgs arg;
            if (metaProperty.PropertyKind == CimMetaPropertyKind.Attribute)
            {
                arg = new CanCancelAttributeChangingEventArgs(metaProperty, 
                    newValue);
            }
            else if ((metaProperty.PropertyKind == CimMetaPropertyKind.Assoc1To1
                || metaProperty.PropertyKind == CimMetaPropertyKind.Assoc1ToM)
                && newValue is IModelObject modelObject)
            {
                arg = new CanCancelAssocChangingEventArgs(metaProperty, 
                    modelObject, isRemove ?? false);
            }
            else
            {
                return false;
            }

            PropertyChanging.Invoke(this, arg);
            
            if (arg.Cancel == true)
            {
                return false;
            }
        }      

        return true;
    }

    private BindingList<IModelObject> BindDynamicAssocsCollection(
        ICimMetaProperty metaProperty, IList<IModelObject> assocCollection)
    {
        if (metaProperty.PropertyKind != CimMetaPropertyKind.Assoc1ToM)
        {
            throw new NotSupportedException();
        }

        var blist = new BindingList<IModelObject>(assocCollection);

        blist.ListChanged += (_, e) =>
        {
            if (e.ListChangedType == ListChangedType.ItemAdded)
            {
                var newObj = blist.ElementAt(e.NewIndex);
                AddAssoc1ToM(metaProperty, newObj);
            }
            else if (e.ListChangedType == ListChangedType.ItemDeleted)
            {
                var oldObj = GetAssoc1ToM(metaProperty)
                    .ElementAt(e.NewIndex);
                
                RemoveAssoc1ToM(metaProperty, oldObj);
            }
        };

        return blist;
    }
}
