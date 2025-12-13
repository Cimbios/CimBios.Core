using CimBios.Core.RdfIOLib;
using Serilog;

namespace CimBios.Core.CimModel.Schema.RdfSchema;

public class CimRdfSchemaXmlFactory : ICimSchemaFactory
{
    public ICimSchema CreateSchema(ILogger? logger=null)
    {
        var rdfReader = new RdfXmlReader();
        var serializerFactory = new CimRdfSchemaSerializerFactory(rdfReader, logger);
        return new CimSchema(serializerFactory);
    }
}