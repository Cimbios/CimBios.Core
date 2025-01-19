using CimBios.Core.CimModel.Schema;

namespace CimBios.Core.CimModel.CimDatatypeLib;

[CimClass("http://iec.ch/TC57/61970-552/ModelDescription/1#FullModel")]
public class FullModel(string uuid, ICimMetaClass metaClass, 
    bool isAuto = false) 
    : ModelObject(uuid, metaClass, isAuto)
{
    public string? created 
    { 
        get => GetAttribute<string>(nameof(created)); 
        set => SetAttribute(nameof(created), value); 
    }
    public string? version
    { 
        get => GetAttribute<string>(nameof(version)); 
        set => SetAttribute(nameof(version), value);
    }
}
