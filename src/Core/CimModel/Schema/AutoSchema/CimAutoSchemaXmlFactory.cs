using CimBios.Core.RdfIOLib;

namespace CimBios.Core.CimModel.Schema.AutoSchema;

public class CimAutoSchemaXmlFactory : ICimSchemaFactory
{
    public ICimSchema CreateSchema()
    {
        var rdfReader = new RdfXmlReader();
        var serializer = new CimAutoSchemaSerializer(rdfReader);
        return new CimSchema(serializer);
    }
}
