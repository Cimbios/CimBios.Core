
using CimBios.Core.CimModel.CimDataModel;
using CimBios.Core.CimModel.CimDatatypeLib;
using CimBios.Core.CimModel.CimDatatypeLib.CIM17Types;
using CimBios.Core.CimModel.CimDatatypeLib.OID;
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

        var checkObject = cimModel.GetObject<WeakModelObject>(
            cimModel.OIDDescriptorFactory.Create("_currenttransformer1"));
            
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
            .GetObject<Terminal>(new TextDescriptor("_terminal1"));
            
        Assert.NotNull(checkObject);
    }

    [Fact]
    public void UnkClassKnownProperty()
    {
        var cimModel = CreateCimModelInstance(
            "../../../assets/test_model.xml",
            "../../../assets/Iec61970BaseCore-rdfs.xml"
        );

        var checkObject = cimModel.GetObject<WeakModelObject>(
            new TextDescriptor("_currenttransformer1"));

        var checkAttrName = checkObject?.GetAttribute("name");
            
        Assert.NotNull(checkAttrName);
    }    

    [Fact]
    public void UnkClassUnknownProperty()
    {
        var cimModel = CreateCimModelInstance(
            "../../../assets/test_model.xml",
            "../../../assets/Iec61970BaseCore-rdfs.xml"
        );

        var checkObject = cimModel.GetObject<WeakModelObject>(
            new TextDescriptor("_currenttransformer1"));
            
        var checkAttrName = checkObject?
            .GetAttribute("ratedCurrent");
            
        Assert.NotNull(checkAttrName);
    }  

    [Fact]
    public void ClassUnkRefAsAssoc()
    {
        var cimModel = CreateCimModelInstance(
            "../../../assets/test_model.xml",
            "../../../assets/Iec61970BaseCore-rdfs.xml"
        );

        var checkObject = cimModel.GetObject<Terminal>(
            new TextDescriptor("_terminal1"));
            
        var checkEnumAssocType = checkObject?.GetAssoc1ToM("Type");
            
        Assert.IsType<ModelObjectUnresolvedReference>(checkEnumAssocType?.Single());
    }  

    [Fact]
    public void UnkCompound()
    {
        var cimModel = CreateCimModelInstance(
            "../../../assets/test_model.xml",
            "../../../assets/Iec61970BaseCore-rdfs.xml"
        );

        var checkObject = cimModel.GetObject<WeakModelObject>(
            new TextDescriptor("_currenttransformer1"));
            
        var checkCompound = checkObject?
            .GetAttribute<IModelObject>("CompoundProperty");

        Assert.IsType<WeakModelObject>(checkCompound);
        Assert.True(checkCompound.OID is AutoDescriptor);
    }

    private static ICimDataModel CreateCimModelInstance(string modelPath, 
        string schemaPath)
    {
        var schema = LoadCimSchema(schemaPath);
        var typeLib = new CimDatatypeLib(schema);

        var cimDocument = new CimDocument(schema, typeLib, 
            new TextDescriptorFactory());
            
        cimDocument.Load(modelPath, new RdfXmlSerializerFactory() 
        { 
            Settings = new RdfSerializerSettings() 
            {  
                UnknownClassesAllowed = true,
                UnknownPropertiesAllowed = true
            } 
        });

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
