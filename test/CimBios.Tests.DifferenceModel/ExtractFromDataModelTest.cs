using CimBios.Core.CimModel.CimDataModel;
using CimBios.Core.CimModel.CimDatatypeLib;
using CimBios.Core.CimModel.RdfSerializer;
using CimBios.Core.CimModel.Schema;
using CimBios.Core.CimModel.Schema.RdfSchema;
using CimBios.Core.CimModel.CimDatatypeLib.CIM17Types;
using CimBios.Core.CimModel.CimDifferenceModel;
using CimBios.Core.CimModel.CimDatatypeLib.OID;

namespace CimBios.Tests.DifferenceModel;

public class ExtractFromDataModelTest
{
    private const string DiffSchemaPath 
        = "../../../../common_assets/Iec61970-552-Headers-rdfs.xml";
    private const string ModelSchemaPath 
        = "../../../../common_assets/Iec61970BaseCore-rdfs.xml";

    [Fact]
    public void AddModelObject()
    {
        var diffSchema = LoadCimSchema(DiffSchemaPath);

        var cimDifferenceModel = new CimDifferenceModel(diffSchema, 
            new CimDatatypeLib(diffSchema), new TextDescriptorFactory());

        var cimDocument = CreateCimModelInstance(ModelSchemaPath);

        cimDocument.CreateObject<Substation>(new TextDescriptor("test1"));

        cimDifferenceModel.ExtractFromDataModel(cimDocument);

        Assert.Contains(
            cimDifferenceModel.Differences
                .OfType<AdditionDifferenceObject>(),
            d => (TextDescriptor)d.OID == "test1"
        );
    }

    [Fact]
    public void UpdateModelObjectAttribute()
    {
        var diffSchema = LoadCimSchema(DiffSchemaPath);

        var cimDifferenceModel = new CimDifferenceModel(diffSchema, 
            new CimDatatypeLib(diffSchema), new TextDescriptorFactory());

        var cimDocument = CreateCimModelInstance(ModelSchemaPath);

        var substation = cimDocument.CreateObject<Substation>(
            new TextDescriptor("test1"));
        cimDocument.CommitAllChanges(); // 4 avoid add change statement

        substation.name = "Test name";
       
        cimDifferenceModel.ExtractFromDataModel(cimDocument);

        Assert.Contains(
            cimDifferenceModel.Differences
                .OfType<UpdatingDifferenceObject>(),
            d => (TextDescriptor)d.OID == "test1" && d.ModifiedObject
                .GetAttribute<string>("name") == "Test name"
        );
    }

    [Fact]
    public void UpdateModelObjectAssocs()
    {
        var diffSchema = LoadCimSchema(DiffSchemaPath);

        var cimDifferenceModel = new CimDifferenceModel(diffSchema, 
            new CimDatatypeLib(diffSchema), new TextDescriptorFactory());

        var cimDocument = CreateCimModelInstance(ModelSchemaPath);

        var substation = cimDocument.CreateObject<Substation>(
            new TextDescriptor("test1"));
        var voltageLevel = cimDocument.CreateObject<VoltageLevel>(
            new TextDescriptor("test2"));
        cimDocument.CommitAllChanges(); // 4 avoid add change statement

        voltageLevel.Substation = substation;
       
        cimDifferenceModel.ExtractFromDataModel(cimDocument);

        Assert.Contains(
            cimDifferenceModel.Differences
                .OfType<UpdatingDifferenceObject>(),
            d => (TextDescriptor)d.OID == "test2" && d.ModifiedObject
                .GetAssoc1To1<IModelObject>("Substation") == substation
        );
    }

    [Fact]
    public void AddAndUpdateModelObject()
    {
        var diffSchema = LoadCimSchema(DiffSchemaPath);

        var cimDifferenceModel = new CimDifferenceModel(diffSchema, 
            new CimDatatypeLib(diffSchema), new TextDescriptorFactory());

        var cimDocument = CreateCimModelInstance(ModelSchemaPath);

        var substation = cimDocument.CreateObject<Substation>(
            new TextDescriptor("test1"));

        substation.name = "Test name";
        substation.name = "New name";
       
        cimDifferenceModel.ExtractFromDataModel(cimDocument);

        Assert.Contains(
            cimDifferenceModel.Differences
                .OfType<AdditionDifferenceObject>(),
            d => (TextDescriptor)d.OID == "test1" && d.ModifiedObject
                .GetAttribute<string>("name") == "New name"
        );
    }

    [Fact]
    public void RemoveModelObject()
    {
        var diffSchema = LoadCimSchema(DiffSchemaPath);

        var cimDifferenceModel = new CimDifferenceModel(diffSchema, 
            new CimDatatypeLib(diffSchema), new TextDescriptorFactory());

        var cimDocument = CreateCimModelInstance(ModelSchemaPath);

        var terminal = cimDocument.CreateObject<Terminal>(
            new TextDescriptor("test1"));

        terminal.name = "Test name";
        terminal.sequenceNumber = 2;
        cimDocument.CommitAllChanges(); // 4 avoid add change statement

        cimDocument.RemoveObject(terminal);
       
        cimDifferenceModel.ExtractFromDataModel(cimDocument);

        Assert.Contains(
            cimDifferenceModel.Differences
                .OfType<DeletionDifferenceObject>(),
            d => (TextDescriptor)d.OID == "test1" 
                && d.ModifiedObject.GetAttribute<string>("name") == "Test name"
                && d.ModifiedObject.GetAttribute<int>("sequenceNumber") == 2
        );
    }

    [Fact]
    public void UpdateAndRemoveModelObject()
    {
        var diffSchema = LoadCimSchema(DiffSchemaPath);

        var cimDifferenceModel = new CimDifferenceModel(diffSchema, 
            new CimDatatypeLib(diffSchema), new TextDescriptorFactory());

        var cimDocument = CreateCimModelInstance(ModelSchemaPath);

        var terminal = cimDocument.CreateObject<Terminal>(
            new TextDescriptor("test1"));
        terminal.name = "Test name";
        cimDocument.CommitAllChanges(); // 4 avoid add change statement

        terminal.name = "New name";
        cimDocument.RemoveObject(terminal);
       
        cimDifferenceModel.ExtractFromDataModel(cimDocument);

        Assert.Contains(
            cimDifferenceModel.Differences
                .OfType<DeletionDifferenceObject>(),
            d => (TextDescriptor)d.OID == "test1" 
                && d.ModifiedObject.GetAttribute<string>("name") == "Test name"
        );
    }

    // Just behaivour
    [Fact]
    public void SaveDiff ()
    {
        var diffSchema = LoadCimSchema(DiffSchemaPath);

        var cimDifferenceModel = new CimDifferenceModel(diffSchema, 
            new CimDatatypeLib(diffSchema), new TextDescriptorFactory());

        var cimDocument = CreateCimModelInstance(ModelSchemaPath);

        var terminal = cimDocument.CreateObject<Terminal>(
            new TextDescriptor("ex_t_1"));
        terminal.name = "Test Terminal";

        var cn = cimDocument.CreateObject<ConnectivityNode>(
            new TextDescriptor("ex_cn_1"));
        cn.name = "Test ConnectivityNode";

        var substation = cimDocument.CreateObject<Substation>(
            new TextDescriptor("ex_ss_1"));
        substation.name = "Test Substation";

        var voltageLevel = cimDocument.CreateObject<VoltageLevel>(
            new TextDescriptor("ex_vl_1"));
        voltageLevel.name = "Test VoltageLevel";
        voltageLevel.Substation = substation;

        var bay = cimDocument.CreateObject<Bay>(
            new TextDescriptor("ex_bay_1"));
        bay.name = "Test Bay";
        bay.VoltageLevel = voltageLevel;   

        cimDocument.CommitAllChanges(); // 4 avoid add change statement

        terminal.name = "Terminal name";
        terminal.ConnectivityNode = cn;
        terminal.phases = PhaseCode.ABC;

        var newTerminal = cimDocument.CreateObject<Terminal>(
            new TextDescriptor("new_t_1"));
        newTerminal.name = "New terminal"; 
        newTerminal.sequenceNumber = 1;    

        cn.AddAssoc1ToM("Terminals", newTerminal);

        cimDocument.RemoveObject(voltageLevel);

        cimDifferenceModel.ExtractFromDataModel(cimDocument);
        cimDifferenceModel.Save(
            new StreamWriter("../../../assets/test_diff_wr_extracted.xml"),
            new RdfXmlSerializerFactory());
    }

    private static ICimDataModel CreateCimModelInstance(string schemaPath)
    {
        var schema = LoadCimSchema(schemaPath);
        var cimDocument = new CimDocument(schema, new CimDatatypeLib(schema), 
            new TextDescriptorFactory());

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

