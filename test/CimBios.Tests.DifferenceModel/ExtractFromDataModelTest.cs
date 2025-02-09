using CimBios.Core.CimModel.CimDataModel;
using CimBios.Core.CimModel.CimDatatypeLib;
using CimBios.Core.CimModel.RdfSerializer;
using CimBios.Core.CimModel.Schema;
using CimBios.Core.CimModel.Schema.RdfSchema;
using CimBios.Core.CimDifferenceModel;
using CimBios.Core.CimModel.CimDatatypeLib.CIM17Types;

namespace CimBios.Tests.DifferenceModel;

public class ExtractFromDataModelTest
{
    [Fact]
    public void AddModelObject()
    {
        var diffSchema = LoadCimSchema("../../../assets/Iec61970-552-Headers-rdfs.xml");
        var rdfSerializer = new RdfXmlSerializer(diffSchema, new CimDatatypeLib(diffSchema));
        var cimDifferenceModel = new CimDifferenceModel(rdfSerializer);

        var cimDocument = CreateCimModelInstance("../../../assets/Iec61970BaseCore-rdfs.xml");

        cimDocument.CreateObject<Terminal>("test1");
        cimDocument.CreateObject<Terminal>("test2");
       
        cimDifferenceModel.ExtractFromDataModel(cimDocument);

        Assert.True(true);
    }

    private static ICimDataModel CreateCimModelInstance(string schemaPath)
    {
        var schema = LoadCimSchema(schemaPath);
        var rdfSerializer = new RdfXmlSerializer(schema, 
            new CimDatatypeLib(schema))
        {
            Settings = new RdfSerializerSettings()
            {
                UnknownClassesAllowed = true,
                UnknownPropertiesAllowed = true
            }
        };

        var cimDocument = new CimDocument(rdfSerializer);

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

