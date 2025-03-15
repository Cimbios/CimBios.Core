using CimBios.Core.CimModel.CimDataModel;
using CimBios.Core.CimModel.CimDatatypeLib;
using CimBios.Core.CimModel.CimDatatypeLib.CIM17Types;
using CimBios.Core.CimModel.CimDatatypeLib.OID;
using CimBios.Core.CimModel.RdfSerializer;
using CimBios.Core.CimModel.Schema;
using CimBios.Core.CimModel.Schema.RdfSchema;

namespace CimBios.Tests.DifferenceModel;

public class ModelObjectsComparerTest
{
    private const string ModelSchemaPath 
        = "../../../../common_assets/Iec61970BaseCore-rdfs.xml";

    [Fact]
    public void CompareDifferentPrimitiveAttribute()
    {
        var cimDocument1 = CreateCimModelInstance(ModelSchemaPath);
        var cimDocument2 = CreateCimModelInstance(ModelSchemaPath);
        var t1 = cimDocument1.CreateObject<Terminal>(new TextDescriptor("t1"));
        var t2 = cimDocument2.CreateObject<Terminal>(new TextDescriptor("t2"));

        t1.name = "t1";
        t2.name = "t2";

        var diff = ModelObjectsComparer.Compare(t1, t2);

        Assert.Single(diff.ModifiedProperties, u => u.ShortName == "name");

        var modifiedName = diff.ModifiedProperties.Single();
        Assert.Equal(diff.OriginalObject?.GetAttribute<string>(modifiedName),
            t1.name);

        Assert.Equal(diff.ModifiedObject.GetAttribute<string>(modifiedName),
            t2.name);          
    }

    [Fact]
    public void CompareEqualsPrimitiveAttribute()
    {
        var cimDocument1 = CreateCimModelInstance(ModelSchemaPath);
        var cimDocument2 = CreateCimModelInstance(ModelSchemaPath);
        var t1 = cimDocument1.CreateObject<Terminal>(new TextDescriptor("t1"));
        var t2 = cimDocument2.CreateObject<Terminal>(new TextDescriptor("t2"));

        t1.name = "t";
        t2.name = "t";

        var diff = ModelObjectsComparer.Compare(t1, t2);

        Assert.Empty(diff.ModifiedProperties);     
    }

    [Fact]
    public void CompareDifferentFloatAttribute()
    {
        var cimDocument1 = CreateCimModelInstance(ModelSchemaPath);
        var cimDocument2 = CreateCimModelInstance(ModelSchemaPath);

        var v220 = cimDocument1.CreateObject<BaseVoltage>(
            new TextDescriptor("Voltage220"));
        var v500 = cimDocument2.CreateObject<BaseVoltage>(
            new TextDescriptor("Voltage500"));
        var v500_1 = cimDocument2.CreateObject<BaseVoltage>(
            new TextDescriptor("Voltage500_1"));
        var v500_2 = cimDocument2.CreateObject<BaseVoltage>(
            new TextDescriptor("Voltage500_2"));

        v220.nominalVoltage = 220.0f;
        v500.nominalVoltage = 500.0f;
        v500_1.nominalVoltage = 500.0f;
        v500_2.nominalVoltage = 500.0001f;

        var diff1 = ModelObjectsComparer.Compare(v220, v500);
        Assert.Single(diff1.ModifiedProperties, 
            u => u.ShortName == "nominalVoltage");

        var diff2 = ModelObjectsComparer.Compare(v500, v500_1);
        Assert.Empty(diff2.ModifiedProperties);

        var diff3 = ModelObjectsComparer.Compare(v500_1, v500_2);
        Assert.Single(diff3.ModifiedProperties, 
            u => u.ShortName == "nominalVoltage");
    }

    [Fact]
    public void CompareDifferenEnumAttribute()
    {
        var cimDocument1 = CreateCimModelInstance(ModelSchemaPath);
        var cimDocument2 = CreateCimModelInstance(ModelSchemaPath);
        
        var t1 = cimDocument1.CreateObject<Terminal>(
            new TextDescriptor("t1"));
        var t2 = cimDocument2.CreateObject<Terminal>(
                new TextDescriptor("t2"));

        t1.phases = PhaseCode.A;
        t2.phases = PhaseCode.B;

        var diff = ModelObjectsComparer.Compare(t1, t2);

        Assert.Single(diff.ModifiedProperties, u => u.ShortName == "phases");

        var modifiedName = diff.ModifiedProperties.Single();
        Assert.Equal(diff.OriginalObject?.GetAttribute<PhaseCode>(modifiedName),
            t1.phases);

        Assert.Equal(diff.ModifiedObject.GetAttribute<PhaseCode>(modifiedName),
            t2.phases);          
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
