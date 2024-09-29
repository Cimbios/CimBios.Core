using CimBios.Core.CimModel.Schema;

namespace CimBios.Tools.ModelDebug.Models;

public class CimSchemaEntityNodeModel : TreeViewNodeModel
{
    public ICimSchemaSerializable CimSchemaEntity { get; }
    
    public CimSchemaEntityNodeModel(ICimSchemaSerializable cimSchemaEntity)
    {
        CimSchemaEntity = cimSchemaEntity;
        Description = $"{cimSchemaEntity.BaseUri.AbsoluteUri}\n{cimSchemaEntity.Description}";
    }
}
