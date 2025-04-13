using CimBios.Core.CimModel.Schema;

namespace CimBios.Tools.ModelDebug.Models;

public class CimSchemaSelectorModel(string title,
    ICimSchemaFactory schemaFactory)
{
    public string Title { get; } = title;
    public ICimSchemaFactory SchemaFactory { get; } = schemaFactory;
}
