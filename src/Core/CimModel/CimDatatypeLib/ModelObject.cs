using System;
using System.ComponentModel;

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
}

/// <summary>
/// Model super type.
/// </summary>
public class ModelObject : IModelObject
{
    public string Uuid { get => ObjectData.Uuid; }

    public IDataFacade ObjectData { get; }

    public ModelObject(DataFacade objectData)
    {
        ObjectData = objectData;
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
}

