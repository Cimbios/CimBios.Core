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
