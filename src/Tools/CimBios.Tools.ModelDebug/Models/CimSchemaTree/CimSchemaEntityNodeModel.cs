using CimBios.Core.CimModel.Schema;

namespace CimBios.Tools.ModelDebug.Models.CimSchemaTree;

public class CimSchemaEntityNodeModel : TreeViewNodeModel
{
    public CimSchemaEntityNodeModel(ICimMetaResource cimSchemaEntity)
    {
        CimSchemaEntity = cimSchemaEntity;
        Description = $"{cimSchemaEntity.BaseUri.AbsoluteUri}\n{cimSchemaEntity.Description}";
    }

    public ICimMetaResource CimSchemaEntity { get; }
}