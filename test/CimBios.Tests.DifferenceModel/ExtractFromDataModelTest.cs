using CimBios.Core.CimModel.CimDataModel;
using CimBios.Core.CimModel.CimDatatypeLib;
using CimBios.Core.CimModel.RdfSerializer;
using CimBios.Core.CimModel.Schema;
using CimBios.Core.CimModel.Schema.RdfSchema;
using CimBios.Core.CimModel.CimDatatypeLib.CIM17Types;
using CimBios.Core.CimModel.CimDifferenceModel;

namespace CimBios.Tests.DifferenceModel;

public class ExtractFromDataModelTest
{
    private const string DiffSchemaPath 
        = "../../../assets/Iec61970-552-Headers-rdfs.xml";
    private const string ModelSchemaPath 
        = "../../../assets/Iec61970BaseCore-rdfs.xml";

    [Fact]
    public void AddModelObject()
    {
        var diffSchema = LoadCimSchema(DiffSchemaPath);
        var rdfSerializer = new RdfXmlSerializer(diffSchema, 
            new CimDatatypeLib(diffSchema));

        var cimDifferenceModel = new CimDifferenceModel(rdfSerializer);

        var cimDocument = CreateCimModelInstance(ModelSchemaPath);

        cimDocument.CreateObject<Substation>("test1");

        cimDifferenceModel.ExtractFromDataModel(cimDocument);

        Assert.Contains(
            cimDifferenceModel.Differences
                .OfType<AdditionDifferenceObject>(),
            d => d.OID == "test1"
        );
    }

    [Fact]
    public void UpdateModelObjectAttribute()
    {
        var diffSchema = LoadCimSchema(DiffSchemaPath);
        var rdfSerializer = new RdfXmlSerializer(diffSchema, 
            new CimDatatypeLib(diffSchema));

        var cimDifferenceModel = new CimDifferenceModel(rdfSerializer);

        var cimDocument = CreateCimModelInstance(ModelSchemaPath);

        var substation = cimDocument.CreateObject<Substation>("test1");
        cimDocument.CommitAllChanges(); // 4 avoid add change statement

        substation.name = "Test name";
       
        cimDifferenceModel.ExtractFromDataModel(cimDocument);

        Assert.Contains(
            cimDifferenceModel.Differences
                .OfType<UpdatingDifferenceObject>(),
            d => d.OID == "test1" && d.ModifiedObject
                .GetAttribute<string>("name") == "Test name"
        );
    }

    [Fact]
    public void UpdateModelObjectAssocs()
    {
        var diffSchema = LoadCimSchema(DiffSchemaPath);
        var rdfSerializer = new RdfXmlSerializer(diffSchema, 
            new CimDatatypeLib(diffSchema));

        var cimDifferenceModel = new CimDifferenceModel(rdfSerializer);

        var cimDocument = CreateCimModelInstance(ModelSchemaPath);

        var substation = cimDocument.CreateObject<Substation>("test1");
        var voltageLevel = cimDocument.CreateObject<VoltageLevel>("test2");
        cimDocument.CommitAllChanges(); // 4 avoid add change statement

        voltageLevel.Substation = substation;
       
        cimDifferenceModel.ExtractFromDataModel(cimDocument);

        Assert.Contains(
            cimDifferenceModel.Differences
                .OfType<UpdatingDifferenceObject>(),
            d => d.OID == "test2" && d.ModifiedObject
                .GetAssoc1To1<IModelObject>("Substation") == substation
        );
    }

    [Fact]
    public void AddAndUpdateModelObject()
    {
        var diffSchema = LoadCimSchema(DiffSchemaPath);
        var rdfSerializer = new RdfXmlSerializer(diffSchema, 
            new CimDatatypeLib(diffSchema));

        var cimDifferenceModel = new CimDifferenceModel(rdfSerializer);

        var cimDocument = CreateCimModelInstance(ModelSchemaPath);

        var substation = cimDocument.CreateObject<Substation>("test1");

        substation.name = "Test name";
        substation.name = "New name";
       
        cimDifferenceModel.ExtractFromDataModel(cimDocument);

        Assert.Contains(
            cimDifferenceModel.Differences
                .OfType<AdditionDifferenceObject>(),
            d => d.OID == "test1" && d.ModifiedObject
                .GetAttribute<string>("name") == "New name"
        );
    }

    [Fact]
    public void RemoveModelObject()
    {
        var diffSchema = LoadCimSchema(DiffSchemaPath);
        var rdfSerializer = new RdfXmlSerializer(diffSchema, 
            new CimDatatypeLib(diffSchema));

        var cimDifferenceModel = new CimDifferenceModel(rdfSerializer);

        var cimDocument = CreateCimModelInstance(ModelSchemaPath);

        var terminal = cimDocument.CreateObject<Terminal>("test1");
        terminal.name = "Test name";
        terminal.sequenceNumber = 2;
        cimDocument.CommitAllChanges(); // 4 avoid add change statement

        cimDocument.RemoveObject(terminal);
       
        cimDifferenceModel.ExtractFromDataModel(cimDocument);

        Assert.Contains(
            cimDifferenceModel.Differences
                .OfType<DeletionDifferenceObject>(),
            d => d.OID == "test1" 
                && d.ModifiedObject.GetAttribute<string>("name") == "Test name"
                && d.ModifiedObject.GetAttribute<int>("sequenceNumber") == 2
        );
    }

    [Fact]
    public void UpdateAndRemoveModelObject()
    {
        var diffSchema = LoadCimSchema(DiffSchemaPath);
        var rdfSerializer = new RdfXmlSerializer(diffSchema, 
            new CimDatatypeLib(diffSchema));

        var cimDifferenceModel = new CimDifferenceModel(rdfSerializer);

        var cimDocument = CreateCimModelInstance(ModelSchemaPath);

        var terminal = cimDocument.CreateObject<Terminal>("test1");
        terminal.name = "Test name";
        cimDocument.CommitAllChanges(); // 4 avoid add change statement

        terminal.name = "New name";
        cimDocument.RemoveObject(terminal);
       
        cimDifferenceModel.ExtractFromDataModel(cimDocument);

        Assert.Contains(
            cimDifferenceModel.Differences
                .OfType<DeletionDifferenceObject>(),
            d => d.OID == "test1" 
                && d.ModifiedObject.GetAttribute<string>("name") == "Test name"
        );
    }

    // Just behaivour
    [Fact]
    public void SaveDiff ()
    {
        var diffSchema = LoadCimSchema(DiffSchemaPath);
        var rdfSerializer = new RdfXmlSerializer(diffSchema, 
            new CimDatatypeLib(diffSchema));

        var cimDifferenceModel = new CimDifferenceModel(rdfSerializer);

        var cimDocument = CreateCimModelInstance(ModelSchemaPath);

        var terminal = cimDocument.CreateObject<Terminal>("ex_t_1");
        terminal.name = "Test Terminal";

        var cn = cimDocument.CreateObject<ConnectivityNode>("ex_cn_1");
        cn.name = "Test ConnectivityNode";

        var substation = cimDocument.CreateObject<Substation>("ex_ss_1");
        substation.name = "Test Substation";

        var voltageLevel = cimDocument.CreateObject<VoltageLevel>("ex_vl_1");
        voltageLevel.name = "Test VoltageLevel";
        voltageLevel.Substation = substation;

        var bay = cimDocument.CreateObject<Bay>("ex_bay_1");
        bay.name = "Test Bay";
        bay.VoltageLevel = voltageLevel;   

        cimDocument.CommitAllChanges(); // 4 avoid add change statement

        terminal.name = "Terminal name";
        terminal.ConnectivityNode = cn;
        terminal.phases = PhaseCode.ABC;

        var newTerminal = cimDocument.CreateObject<Terminal>("new_t_1");
        newTerminal.name = "New terminal"; 
        newTerminal.sequenceNumber = 1;    

        cn.AddAssoc1ToM("Terminals", newTerminal);

        cimDocument.RemoveObject(voltageLevel);

        cimDifferenceModel.ExtractFromDataModel(cimDocument);
        cimDifferenceModel.Save(
            new StreamWriter("../../../assets/test_diff_wr_extracted.xml"));
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

