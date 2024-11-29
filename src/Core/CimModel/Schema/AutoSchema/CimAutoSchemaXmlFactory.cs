using CimBios.Core.RdfIOLib;

namespace CimBios.Core.CimModel.Schema.AutoSchema;

public class CimAutoSchemaXmlFactory : ICimSchemaFactory
{
    public ICimSchema CreateSchema()
    {
        var rdfReader = new RdfXmlReader();
        var serializerFactory = new CimAutoSchemaSerializerFactory(rdfReader);
        return new CimSchema(serializerFactory);
    }
}
