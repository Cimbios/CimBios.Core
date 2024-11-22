namespace CimBios.Core.CimModel.Schema.RdfSchema;

public class CimRdfSchemaFactory : ICimSchemaFactory
{
    public ICimSchema CreateSchema()
    {
        return new CimSchema(new CimRdfSchemaSerializer());
    }
}
