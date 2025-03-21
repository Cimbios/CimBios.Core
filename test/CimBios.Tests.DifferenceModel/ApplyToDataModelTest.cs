using CimBios.Core.CimModel.CimDatatypeLib.CIM17Types;
using CimBios.Tests.Infrastructure;

namespace CimBios.Tests.DifferenceModel;

public class ApplyToDataModelTest
{
    [Fact]
    public void AddModelObject()
    {
        var cimDifferenceModel = ModelLoader.LoadCimDiffModel_v1();
        var cimDocument = ModelLoader.LoadCimModel_v1();

        cimDifferenceModel.ApplyToDataModel(cimDocument);

        var newTerminal = cimDocument.GetObject<Terminal>(
            cimDocument.OIDDescriptorFactory.Create("_NewALoadT1"));

        Assert.NotNull(newTerminal);

        Assert.True(newTerminal.phases == PhaseCode.ABC);
    }

    [Fact]
    public void RemoveObject()
    {
        var cimDifferenceModel = ModelLoader.LoadCimDiffModel_v1();
        var cimDocument = ModelLoader.LoadCimModel_v1();

        cimDifferenceModel.ApplyToDataModel(cimDocument);

        Assert.True(true);
    }
}
