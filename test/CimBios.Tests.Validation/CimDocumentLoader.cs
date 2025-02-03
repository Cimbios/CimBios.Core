using CimBios.Core.CimModel.CimDataModel;
using CimBios.Core.CimModel.CimDatatypeLib;
using CimBios.Core.CimModel.RdfSerializer;
using CimBios.Core.CimModel.Schema.RdfSchema;

namespace CimBios.Tests.Validation;

public static class CimDocumentLoader
{
    public static CimDocument LoadCimDocument(string path, string schemaPath)
    {
        var cimSchema = new CimRdfSchemaXmlFactory().CreateSchema();
        cimSchema.Load(new StreamReader(schemaPath));

        var serializer = new RdfXmlSerializer(cimSchema, new CimDatatypeLib());
        var cimDocument = new CimDocument(serializer);
        cimDocument.Load(path);
        
        return cimDocument;
    }
}
