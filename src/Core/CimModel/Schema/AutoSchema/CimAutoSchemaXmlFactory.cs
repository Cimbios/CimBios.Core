using CimBios.Core.RdfIOLib;
using Serilog;

namespace CimBios.Core.CimModel.Schema.AutoSchema;

public class CimAutoSchemaXmlFactory : ICimSchemaFactory
{
    public ICimSchema CreateSchema(ILogger? logger=null)
    {
        var rdfReader = new RdfXmlReader();
        var serializerFactory = new CimAutoSchemaSerializerFactory(rdfReader);
        return new CimSchema(serializerFactory);
    }
}