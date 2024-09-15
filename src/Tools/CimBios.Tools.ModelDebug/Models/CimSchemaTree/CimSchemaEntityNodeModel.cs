using CimBios.Core.CimModel.Schema;

namespace CimBios.Tools.ModelDebug.Models;

public class CimSchemaEntityNodeModel : TreeViewNodeModel
{
    public ICimSchemaSerializable CimSchemaEntity { get; }

    public string Title { get; set; }

    public string Description { get; set; } = string.Empty;

    public CimSchemaEntityNodeModel(ICimSchemaSerializable cimSchemaEntity)
    {
        CimSchemaEntity = cimSchemaEntity;
        Description = $"{cimSchemaEntity.BaseUri.AbsoluteUri}\n{cimSchemaEntity.Description}";
    }
}
