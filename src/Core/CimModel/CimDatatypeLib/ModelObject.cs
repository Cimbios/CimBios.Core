using System.Dynamic;

namespace CimBios.Core.CimModel.CimDatatypeLib;

/// <summary>
/// Structure interface of abstact super model type.
/// </summary>
public interface IModelObject
{
    /// <summary>
    /// Facade for data operations incapsulation.
    /// </summary>
    public IDataFacade ObjectData { get; }

    /// <summary>
    /// Neccesary object identifier.
    /// </summary>
    public string Uuid { get; }

    public dynamic? AsDynamic();
}

/// <summary>
/// Model super type. Dynamic object logic supports.
/// </summary>
public class ModelObject : DynamicObject, IModelObject
{
    public string Uuid { get => ObjectData.Uuid; }

    public IDataFacade ObjectData { get; }

    public ModelObject(DataFacade objectData)
    {
        ObjectData = objectData;
    }

    public dynamic? AsDynamic()
    {
        return this;
    }

    public override bool TryGetMember(GetMemberBinder binder, out object? result)
    {
        if (ObjectData.Attributes.Contains(binder.Name))
        {
            result = ObjectData.GetAttribute(binder.Name);
            return true;
        }
        else if (ObjectData.Assocs1To1.Contains(binder.Name))
        {
            result = ObjectData.GetAssoc1To1(binder.Name);
            return true;        
        }
        else if (ObjectData.Assocs1ToM.Contains(binder.Name))
        {
            result = ObjectData.GetAssoc1ToM(binder.Name);
            return true;        
        }

        return base.TryGetMember(binder, out result);
    }

    public override bool TrySetMember(SetMemberBinder binder, object? value)
    {
        if (ObjectData.Attributes.Contains(binder.Name))
        {
            ObjectData.SetAttribute(binder.Name, value);
            return true;
        }
        else if (ObjectData.Assocs1To1.Contains(binder.Name))
        {
            ObjectData.SetAssoc1To1(binder.Name, value as IModelObject);
            return true;        
        }

        return base.TrySetMember(binder, value);
    }
}

/// <summary>
/// Class for unfindable by reference object in model
/// ObjectData - ClassType means predicate URI
/// </summary>
public sealed class ModelObjectUnresolvedReference 
    : IModelObject
{
    public IDataFacade ObjectData { get; }

    public string Uuid => ObjectData.Uuid;

    public Uri Predicate => ObjectData.ClassType;

    public ModelObjectUnresolvedReference(IDataFacade objectData)
    {
        ObjectData = objectData;
    }

    public dynamic? AsDynamic()
    {
        return null;
    }
}

/// <summary>
/// Class for class instance from schema.
/// </summary>
public sealed class CimSchemaIndividualModelObject : IModelObject
{
    public IDataFacade ObjectData { get; }

    public string Uuid => ObjectData.Uuid;

    public Uri ClassType => ObjectData.ClassType;

    public CimSchemaIndividualModelObject(IDataFacade objectData)
    {
        ObjectData = objectData;
    }

    public dynamic? AsDynamic()
    {
        return null;
    }
}

