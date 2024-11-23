namespace CimBios.Core.CimModel.Schema.AutoSchema;

public class CimAutoSchemaFactory : ICimSchemaFactory
{
    public ICimSchema CreateSchema()
    {
        return new CimSchema(new CimAutoSchemaSerializer());
    }
}
