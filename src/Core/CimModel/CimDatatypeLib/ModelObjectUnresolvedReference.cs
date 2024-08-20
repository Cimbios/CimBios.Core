namespace CimBios.Core.CimModel.CimDatatypeLib;

/// <summary>
/// Class for unfind by reference object in model
/// ObjectData - ClassType means predicate URI
/// </summary>
public sealed class ModelObjectUnresolvedReference : IModelObject
{
    public IDataFacade ObjectData { get; }

    public string Uuid => ObjectData.Uuid;

    public ModelObjectUnresolvedReference(IDataFacade objectData)
    {
        ObjectData = objectData;
    }
}
