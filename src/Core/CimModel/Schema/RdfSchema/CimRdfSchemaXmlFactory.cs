using CimBios.Core.RdfIOLib;

namespace CimBios.Core.CimModel.Schema.RdfSchema;

public class CimRdfSchemaXmlFactory : ICimSchemaFactory
{
    public ICimSchema CreateSchema()
    {
        var rdfReader = new RdfXmlReader();
        var serializerFactory = new CimRdfSchemaSerializerFactory(rdfReader);
        return new CimSchema(serializerFactory);
    }
}