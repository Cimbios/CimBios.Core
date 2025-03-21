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

        Assert.True(true);
    }
}
