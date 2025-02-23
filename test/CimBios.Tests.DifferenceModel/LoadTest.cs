using CimBios.Core.CimModel.CimDatatypeLib;
using CimBios.Core.CimModel.RdfSerializer;
using CimBios.Core.CimModel.Schema;
using CimBios.Core.CimModel.Schema.RdfSchema;
using CimBios.Core.CimDifferenceModel;

namespace CimBios.Tests.DifferenceModel;

public class LoadTest
{
    [Fact]
    public void LoadAndGetDifferenceModelObject()
    {
        var schema = LoadCimSchema("../../../assets/Iec61970-552-Headers-rdfs.xml");
        var rdfSerializer = new RdfXmlSerializer(schema, 
            new CimDatatypeLib(schema));
        var cimDifferenceModel = new CimDifferenceModel(rdfSerializer);

        cimDifferenceModel.Load(new StreamReader("../../../assets/test_diff.xml"));
    }

    [Fact]
    public void SaveDifferenceModelObject()
    {
        var schema = LoadCimSchema("../../../assets/Iec61970-552-Headers-rdfs.xml");
        var rdfSerializer = new RdfXmlSerializer(schema, 
            new CimDatatypeLib(schema));
        var cimDifferenceModel = new CimDifferenceModel(rdfSerializer);

        cimDifferenceModel.Load(new StreamReader("../../../assets/test_diff.xml"));
        cimDifferenceModel.Save(new StreamWriter("../../../assets/test_diff_wr.xml"));
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
