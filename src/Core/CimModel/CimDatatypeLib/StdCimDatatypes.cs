using CimBios.Core.CimModel.Schema;

namespace CimBios.Core.CimModel.CimDatatypeLib;

public interface IFullModel
{
    public string? Created { get; set; }
    public string? Version { get; set; }
}

[CimClass("http://iec.ch/TC57/61970-552/ModelDescription/1#FullModel")]
public class FullModel(string uuid, ICimMetaClass metaClass, 
    bool isAuto = false) 
    : ModelObject(uuid, metaClass, isAuto), IFullModel
{
    public string? Created 
    { 
        get => GetAttribute<string>("Model.created"); 
        set => SetAttribute("Model.created", value); 
    }
    public string? Version
    { 
        get => GetAttribute<string>("Model.version"); 
        set => SetAttribute("Model.version", value);
    }
}
