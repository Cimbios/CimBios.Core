using CimBios.Core.CimModel.CimDatatypeLib;
using CimBios.Core.CimModel.CimDatatypeLib.CIM17Types;
using CimBios.Core.CimModel.CimDatatypeLib.OID;
using CimBios.Tests.Infrastructure;

namespace CimBios.Tests.DifferenceModel;

public class ModelObjectsComparerTest
{
    [Fact]
    public void CompareDifferentPrimitiveAttribute()
    {
        var cimDocument1 = ModelLoader.CreateCimModelInstance();
        var cimDocument2 = ModelLoader.CreateCimModelInstance();

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
        var cimDocument1 = ModelLoader.CreateCimModelInstance();
        var cimDocument2 = ModelLoader.CreateCimModelInstance();

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
        var cimDocument1 = ModelLoader.CreateCimModelInstance();
        var cimDocument2 = ModelLoader.CreateCimModelInstance();

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
    public void CompareDifferentEnumAttribute()
    {
        var cimDocument1 = ModelLoader.CreateCimModelInstance();
        var cimDocument2 = ModelLoader.CreateCimModelInstance();
        
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

    [Fact]
    public void CompareAssoc11()
    {
        var cimDocument1 = ModelLoader.CreateCimModelInstance();
        var cimDocument2 = ModelLoader.CreateCimModelInstance();
        
        var t1 = cimDocument1.CreateObject<Terminal>(
            new TextDescriptor("t1"));
        var cn1 = cimDocument1.CreateObject<ConnectivityNode>(
            new TextDescriptor("cn1"));
        var t2 = cimDocument2.CreateObject<Terminal>(
            new TextDescriptor("t2"));

        t1.ConnectivityNode = cn1;

        var diff = ModelObjectsComparer.Compare(t1, t2);

        Assert.Single(diff.ModifiedProperties, u => u.ShortName == "ConnectivityNode");     
        Assert.Null(diff.ModifiedObject.GetAssoc1To1("ConnectivityNode"));
        Assert.Equal(diff.OriginalObject?.GetAssoc1To1("ConnectivityNode"), cn1); 

        var cn12 = cimDocument2.CreateObject<ConnectivityNode>(
            new TextDescriptor("cn1"));
        t2.ConnectivityNode = cn12;

        var diff2 = ModelObjectsComparer.Compare(t1, t2);

        Assert.Empty(diff2.ModifiedProperties);
    }

    [Fact]
    public void CompareAssoc1M()
    {
        var cimDocument1 = ModelLoader.CreateCimModelInstance();
        var cimDocument2 = ModelLoader.CreateCimModelInstance();
        
        var s1 = cimDocument1.CreateObject<Substation>(
            new TextDescriptor("s1"));
        var vl11 = cimDocument1.CreateObject<VoltageLevel>(
            new TextDescriptor("vl1"));

        var s2 = cimDocument2.CreateObject<Substation>(
            new TextDescriptor("s2"));
        var vl1 = cimDocument2.CreateObject<VoltageLevel>(
            new TextDescriptor("vl1"));
        var vl2 = cimDocument2.CreateObject<VoltageLevel>(
            new TextDescriptor("vl2"));

        s1.AddToVoltageLevels(vl11);
        s2.AddToVoltageLevels(vl1);
        s2.AddToVoltageLevels(vl2);

        var diff = ModelObjectsComparer.Compare(s1, s2);

        Assert.Single(diff.ModifiedProperties, u => u.ShortName == "VoltageLevels");  

        var vl12 = cimDocument1.CreateObject<VoltageLevel>(
            new TextDescriptor("vl2"));   
        s1.AddToVoltageLevels(vl12);

        var diff2 = ModelObjectsComparer.Compare(s1, s2);

        Assert.Empty(diff2.ModifiedProperties);
    }

    [Fact]
    public void CompareCreatedCompoundAttribute()
    {
        var cimDocument1 = ModelLoader.CreateCimModelInstance();
        var cimDocument2 = ModelLoader.CreateCimModelInstance();
        
        var a1 = cimDocument1.CreateObject<Asset>(
            new TextDescriptor("a1"));
        var a2 = cimDocument2.CreateObject<Asset>(
            new TextDescriptor("a2"));

        var compoundInstance = cimDocument2.TypeLib
            .CreateCompoundInstance<InUseDate>();

        if (compoundInstance == null)
        {
            Assert.Fail();
        }

        a2.SetAttribute("inUseDate", compoundInstance);
        compoundInstance.inUseDate = DateTime.Now;

        var diff = ModelObjectsComparer.Compare(a1, a2);   

        Assert.Single(diff.ModifiedProperties, u => u.ShortName == "inUseDate");   

        Assert.True(true);  
    }
}
