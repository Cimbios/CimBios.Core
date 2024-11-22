namespace CimBios.Core.CimModel.Schema.AutoSchema;

public class CimAutoSchemaFactoryFactory : ICimSchemaFactory
{
    public ICimSchema CreateSchema()
    {
        return new CimSchema(new CimAutoSchemaSerializer());
    }
}
