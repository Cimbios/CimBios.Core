using System.ComponentModel;
using CimBios.Core.CimModel.CimDatatypeLib.EventUtils;
using CimBios.Core.CimModel.CimDatatypeLib.OID;
using CimBios.Core.CimModel.Schema;

namespace CimBios.Core.CimModel.CimDatatypeLib;

/// <summary>
/// Class for unfindable by reference object in model
/// ObjectData - ClassType means predicate URI
/// </summary>
public sealed class ModelObjectUnresolvedReference : IModelObject
{
    public Uri Predicate => MetaClass.BaseUri;

    public IOIDDescriptor OID { get; }

    public ICimMetaClass MetaClass { get; }

    public ICimDatatypeLib TypeLib => throw new NotImplementedException();

    public ModelObjectUnresolvedReference(IOIDDescriptor oid, 
        ICimMetaClass metaClass)
    {
        OID = oid;
        MetaClass = metaClass;
    }

    public event CanCancelPropertyChangingEventHandler? PropertyChanging
    {
        add => throw new NotSupportedException();

        remove { }
    }

    public event PropertyChangedEventHandler? PropertyChanged
    {
        add => throw new NotSupportedException();

        remove { }
    }

    public void SetAttribute<T>(ICimMetaProperty metaProperty, T? value)
    {
        throw new NotImplementedException();
    }

    public void SetAttribute<T>(string attributeName, T? value)
    {
        throw new NotImplementedException();
    }

    public void SetAssoc1To1(ICimMetaProperty metaProperty, IModelObject? obj)
    {
        throw new NotImplementedException();
    }

    public void SetAssoc1To1(string assocName, IModelObject? obj)
    {
        throw new NotImplementedException();
    }

    public void AddAssoc1ToM(ICimMetaProperty metaProperty, IModelObject obj)
    {
        throw new NotImplementedException();
    }

    public void AddAssoc1ToM(string assocName, IModelObject obj)
    {
        throw new NotImplementedException();
    }

    public void RemoveAssoc1ToM(ICimMetaProperty metaProperty, IModelObject obj)
    {
        throw new NotImplementedException();
    }

    public void RemoveAssoc1ToM(string assocName, IModelObject obj)
    {
        throw new NotImplementedException();
    }

    public void RemoveAllAssocs1ToM(ICimMetaProperty metaProperty)
    {
        throw new NotImplementedException();
    }

    public void RemoveAllAssocs1ToM(string assocName)
    {
        throw new NotImplementedException();
    }

    public IReadOnlyModelObject AsReadOnly()
    {
        throw new NotImplementedException();
    }

    public object? GetAttribute(ICimMetaProperty metaProperty)
    {
        throw new NotImplementedException();
    }

    public object? GetAttribute(string attributeName)
    {
        throw new NotImplementedException();
    }

    public T? GetAttribute<T>(ICimMetaProperty metaProperty)
    {
        throw new NotImplementedException();
    }

    public T? GetAttribute<T>(string attributeName)
    {
        throw new NotImplementedException();
    }

    public T? GetAssoc1To1<T>(ICimMetaProperty metaProperty) where T : IModelObject
    {
        throw new NotImplementedException();
    }

    public T? GetAssoc1To1<T>(string assocName) where T : IModelObject
    {
        throw new NotImplementedException();
    }

    public IModelObject[] GetAssoc1ToM(ICimMetaProperty metaProperty)
    {
        throw new NotImplementedException();
    }

    public IModelObject[] GetAssoc1ToM(string assocName)
    {
        throw new NotImplementedException();
    }

    public T[] GetAssoc1ToM<T>(ICimMetaProperty metaProperty) where T : IModelObject
    {
        throw new NotImplementedException();
    }

    public T[] GetAssoc1ToM<T>(string assocName) where T : IModelObject
    {
        throw new NotImplementedException();
    }

    public bool HasProperty(string propertyName)
    {
        throw new NotImplementedException();
    }

    public IModelObject? GetAssoc1To1(ICimMetaProperty metaProperty)
    {
        throw new NotImplementedException();
    }

    public IModelObject? GetAssoc1To1(string assocName)
    {
        throw new NotImplementedException();
    }

    public void InitializeCompoundAttribute(ICimMetaProperty metaProperty, 
        bool reset = true)
    {
        throw new NotImplementedException();
    }
}
