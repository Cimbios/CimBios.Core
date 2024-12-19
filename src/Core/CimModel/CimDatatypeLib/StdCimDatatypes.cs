namespace CimBios.Core.CimModel.CimDatatypeLib;

public interface IFullModel
{
    public string? Created { get; set; }
    public string? Version { get; set; }
}

[CimClass("http://iec.ch/TC57/61970-552/ModelDescription/1#FullModel")]
public class FullModel(DataFacade objectData) 
    : ModelObject(objectData), IFullModel
{
    public string? Created 
    { 
        get => ObjectData.GetAttribute<string>("Model.created"); 
        set => ObjectData.SetAttribute("Model.created", value); 
    }
    public string? Version
    { 
        get => ObjectData.GetAttribute<string>("Model.version"); 
        set => ObjectData.SetAttribute("Model.version", value);
    }
}
