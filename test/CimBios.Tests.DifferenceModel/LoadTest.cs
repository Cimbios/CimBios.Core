using CimBios.Core.CimModel.CimDatatypeLib;
using CimBios.Core.CimModel.CimDatatypeLib.OID;
using CimBios.Core.CimModel.CimDifferenceModel;
using CimBios.Core.CimModel.RdfSerializer;
using CimBios.Core.CimModel.Schema;
using CimBios.Core.CimModel.Schema.RdfSchema;

namespace CimBios.Tests.DifferenceModel;

public class LoadTest
{
    [Fact]
    public void LoadAndGetDifferenceModelObject()
    {
        var schema = LoadCimSchema("../../../assets/Iec61970-552-Headers-rdfs.xml");
        var typeLib = new CimDatatypeLib(schema);

        var cimDifferenceModel = new CimDifferenceModel(schema, typeLib, 
            new TextDescriptorFactory());

        cimDifferenceModel.Load(new StreamReader("../../../assets/test_diff.xml"), 
            new RdfXmlSerializerFactory());

        Assert.True(true);
    }

    [Fact]
    public void SaveDifferenceModelObject()
    {
        var schema = LoadCimSchema("../../../assets/Iec61970-552-Headers-rdfs.xml");
        var typeLib = new CimDatatypeLib(schema);

        var cimDifferenceModel = new CimDifferenceModel(schema, typeLib, 
            new TextDescriptorFactory());

        cimDifferenceModel.Load(new StreamReader("../../../assets/test_diff.xml"), 
            new RdfXmlSerializerFactory());
        cimDifferenceModel.Load(new StreamReader("../../../assets/test_diff_wr.xml"), 
            new RdfXmlSerializerFactory());
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
