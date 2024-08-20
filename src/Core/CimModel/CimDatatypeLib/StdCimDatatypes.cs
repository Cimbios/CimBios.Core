namespace CimBios.Core.CimModel.CimDatatypeLib;

public interface IFullModel : IModelObject
{
    public string Created { get; set; }
    public string Version { get; set; }
}

[CimClass("http://iec.ch/TC57/61970-552/ModelDescription/1#FullModel")]
public class FullModel : IFullModel
{
    public string Uuid { get => ObjectData.Uuid; }
    public string Created 
    { 
        get => ObjectData.GetAttribute<string>("Model.created"); 
        set => ObjectData.SetAttribute("Model.created", value); 
    }
    public string Version
    { 
        get => ObjectData.GetAttribute<string>("Model.version"); 
        set => ObjectData.SetAttribute("Model.version", value);
    }

    public IDataFacade ObjectData { get; }

    public FullModel(DataFacade objectData)
    {
        ObjectData = objectData;
    }
}
