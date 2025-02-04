using CimBios.Core.CimModel.CimDatatypeLib;
using CimBios.Core.CimModel.RdfSerializer;
using CimBios.Core.CimModel.Schema;
using CimBios.Core.CimModel.Schema.RdfSchema;
using CimBios.Core.DifferenceModel;

namespace CimBios.Tests.DifferenceManager;

public class LoadTest
{
    [Fact]
    public void LoadAndGetDifferenceModelObject()
    {
        var schema = LoadCimSchema("../../../assets/Iec61970-552-Headers-rdfs.xml");
        var rdfSerializer = new RdfXmlSerializer(schema, new CimDatatypeLib());
        var cimDifferenceModel = new CimDifferenceModel(rdfSerializer);

        cimDifferenceModel.Load(new StreamReader("../../../assets/test_diff.xml"));
    }

    private static ICimSchema LoadCimSchema(string path, 
        ICimSchemaFactory? factory = null)
    {
        factory ??= new CimRdfSchemaXmlFactory();
        var cimSchema = factory.CreateSchema();

        cimSchema.Load(new StreamReader(path));

        return cimSchema;
    }
}
