using System.ComponentModel;
using CimBios.Core.CimModel.CimDatatypeLib;
using CimBios.Core.CimModel.Schema;

namespace CimBios.Core.CimModel.DatatypeLib;

public class WeakModelObject : DynamicModelObjectBase, IModelObject
{
    public override string Uuid => throw new NotImplementedException();

    public override ICimMetaClass MetaClass => throw new NotImplementedException();

    public override bool IsAuto => throw new NotImplementedException();

    public override event PropertyChangedEventHandler? PropertyChanged;

    public override void AddAssoc1ToM(ICimMetaProperty metaProperty, 
        IModelObject obj)
    {
        throw new NotImplementedException();
    }

    public override void AddAssoc1ToM(string assocName, IModelObject obj)
    {
        throw new NotImplementedException();
    }

    public override T? GetAssoc1To1<T>(ICimMetaProperty metaProperty) 
        where T : default
    {
        throw new NotImplementedException();
    }

    public override T? GetAssoc1To1<T>(string assocName) where T : default
    {
        throw new NotImplementedException();
    }

    public override IModelObject[] GetAssoc1ToM(ICimMetaProperty metaProperty)
    {
        throw new NotImplementedException();
    }

    public override IModelObject[] GetAssoc1ToM(string assocName)
    {
        throw new NotImplementedException();
    }

    public override T[] GetAssoc1ToM<T>(ICimMetaProperty metaProperty)
    {
        throw new NotImplementedException();
    }

    public override T[] GetAssoc1ToM<T>(string assocName)
    {
        throw new NotImplementedException();
    }

    public override object? GetAttribute(ICimMetaProperty metaProperty)
    {
        throw new NotImplementedException();
    }

    public override object? GetAttribute(string attributeName)
    {
        throw new NotImplementedException();
    }

    public override T? GetAttribute<T>(ICimMetaProperty metaProperty) where T : default
    {
        throw new NotImplementedException();
    }

    public override T? GetAttribute<T>(string attributeName) where T : default
    {
        throw new NotImplementedException();
    }

    public override bool HasProperty(string propertyName)
    {
        throw new NotImplementedException();
    }

    public override void RemoveAllAssocs1ToM(ICimMetaProperty metaProperty)
    {
        throw new NotImplementedException();
    }

    public override void RemoveAllAssocs1ToM(string assocName)
    {
        throw new NotImplementedException();
    }

    public override void RemoveAssoc1ToM(ICimMetaProperty metaProperty, IModelObject obj)
    {
        throw new NotImplementedException();
    }

    public override void RemoveAssoc1ToM(string assocName, IModelObject obj)
    {
        throw new NotImplementedException();
    }

    public override void SetAssoc1To1(ICimMetaProperty metaProperty, IModelObject? obj)
    {
        throw new NotImplementedException();
    }

    public override void SetAssoc1To1(string assocName, IModelObject? obj)
    {
        throw new NotImplementedException();
    }

    public override void SetAttribute<T>(ICimMetaProperty metaProperty, T? value) where T : default
    {
        throw new NotImplementedException();
    }

    public override void SetAttribute<T>(string attributeName, T? value) where T : default
    {
        throw new NotImplementedException();
    }
}
