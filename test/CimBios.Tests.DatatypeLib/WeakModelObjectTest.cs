
using CimBios.Core.CimModel.CimDataModel;
using CimBios.Core.CimModel.CimDatatypeLib;
using CimBios.Core.CimModel.CimDatatypeLib.CIM17;
using CimBios.Core.CimModel.DatatypeLib;
using CimBios.Core.CimModel.RdfSerializer;
using CimBios.Core.CimModel.Schema;
using CimBios.Core.CimModel.Schema.RdfSchema;

namespace CimBios.Tests.DatatypeLib;

public class WeakModelObjectTests
{
    [Fact]
    public void UnkClassCreating()
    {
        var cimModel = CreateCimModelInstance(
            "../../../assets/test_model.xml",
            "../../../assets/Iec61970BaseCore-rdfs.xml"
        );

        var checkObject = cimModel
            .GetObject<WeakModelObject>("currenttransformer1");
            
        Assert.NotNull(checkObject);
    }

    [Fact]
    public void JustCheckKnownClassCreating()
    {
        var cimModel = CreateCimModelInstance(
            "../../../assets/test_model.xml",
            "../../../assets/Iec61970BaseCore-rdfs.xml"
        );

        var checkObject = cimModel
            .GetObject<Terminal>("terminal1");
            
        Assert.NotNull(checkObject);
    }

    

    private static ICimDataModel CreateCimModelInstance(string modelPath, 
        string schemaPath)
    {
        var schema = LoadCimSchema(schemaPath);
        var rdfSerializer = new RdfXmlSerializer(schema, new CimDatatypeLib())
        {
            Settings = new RdfSerializerSettings()
            {
                UnknownClassesAllowed = true,
                UnknownPropertiesAllowed = true
            }
        };

        var cimDocument = new CimDocument(rdfSerializer);
        cimDocument.Load(modelPath);

        return cimDocument;
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
