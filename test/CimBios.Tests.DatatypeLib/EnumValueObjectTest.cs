using System.ComponentModel;
using CimBios.Core.CimModel.CimDatatypeLib;
using CimBios.Core.CimModel.CimDatatypeLib.CIM17Types;
using CimBios.Core.CimModel.Schema.AutoSchema;
using CimBios.Tests.Infrastructure;

namespace CimBios.Tests.DatatypeLib;

public enum TestEnum
{
    EnumValue
}

public class EnumValueObjectTest
{

    [Fact]
    public void CreateEnumValueObject ()
    {
        var typeLib = GetTypeLib();

        var typedPhaseA = PhaseCode.A;
        var phaseA = typeLib.CreateEnumValueInstance(typedPhaseA);
        Assert.NotNull(phaseA);

        var phaseAMeta = phaseA.MetaEnumValue;
        var phaseAFromMeta = typeLib.CreateEnumValueInstance(phaseAMeta)?
            .Cast<PhaseCode>();

        Assert.NotNull(phaseAFromMeta);

        Assert.True(typedPhaseA == phaseAFromMeta);
        Assert.False(PhaseCode.B == phaseAFromMeta);
    }

    [Fact]
    public void CreateNoExistEnumValueObject ()
    {
        var typeLib = GetTypeLib();
        
        Assert.Throws<NotSupportedException>(
            () => typeLib.CreateEnumValueInstance(TestEnum.EnumValue));

        var autoIndividual = new CimAutoIndividual(
            new ("urn:unk:enum"), "NoExist", string.Empty);

        Assert.Throws<InvalidEnumArgumentException>(
            () => typeLib.CreateEnumValueInstance(autoIndividual)); 

        Assert.Throws<NotSupportedException>(
            () => typeLib.CreateEnumValueInstance(PotentialTransformerKind.inductive));
    }    

    [Fact]
    public void Equalities ()
    {
        var typeLib = GetTypeLib();

        var phaseA = typeLib.CreateEnumValueInstance(PhaseCode.A);
        var _phaseA = typeLib.CreateEnumValueInstance(PhaseCode.A);
        var refphaseA = phaseA;
        var phaseB = typeLib.CreateEnumValueInstance(PhaseCode.B);

        if (phaseA == null || _phaseA == null || phaseB == null)
        {
            Assert.Fail();
        }

        Assert.True(phaseA == _phaseA);
        Assert.Equal(phaseA, _phaseA);
        Assert.Equal(phaseA, refphaseA);
        Assert.False(phaseA == phaseB);
    }

    private static ICimDatatypeLib GetTypeLib()
    {
        var schema = ModelLoader.LoadTestCimRdfSchema();
        return new CimDatatypeLib(schema);
    }
}
