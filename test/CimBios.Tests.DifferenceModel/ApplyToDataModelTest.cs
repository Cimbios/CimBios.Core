using CimBios.Core.CimModel.CimDatatypeLib.CIM17Types;
using CimBios.Core.CimModel.DataModel.Utils;
using CimBios.Tests.Infrastructure;

namespace CimBios.Tests.DifferenceModel;

public class ApplyToDataModelTest
{
    [Fact]
    public void AddModelObject()
    {
        var cimDifferenceModel = ModelLoader.LoadCimDiffModel_v1();
        var cimDocument = ModelLoader.LoadCimModel_v1();

        cimDocument.ApplyDifferenceModel(cimDifferenceModel);

        var _NewALoadT1 = cimDocument.GetObject<Terminal>(
            cimDocument.OIDDescriptorFactory.Create("_NewALoadT1"));

        var _ACN1 = cimDocument.GetObject<ConnectivityNode>(
            cimDocument.OIDDescriptorFactory.Create("_ACN1"));

        Assert.NotNull(_NewALoadT1);
        Assert.NotNull(_ACN1);

        Assert.Null(_NewALoadT1.name);
        Assert.Equal(1, _NewALoadT1.sequenceNumber);
        Assert.Equal(PhaseCode.ABC, _NewALoadT1.phases);
        Assert.Equal(_ACN1, _NewALoadT1.ConnectivityNode);
    }

    [Fact]
    public void RemoveObject()
    {
        var cimDifferenceModel = ModelLoader.LoadCimDiffModel_v1();
        var cimDocument = ModelLoader.LoadCimModel_v1();

        var _RemoveMeAsset = cimDocument.GetObject<Asset>(
            cimDocument.OIDDescriptorFactory.Create("_RemoveMeAsset"));

        Assert.NotNull(_RemoveMeAsset);

        cimDocument.ApplyDifferenceModel(cimDifferenceModel);

        var _RemoveMeAssetAfter = cimDocument.GetObject<Asset>(
            cimDocument.OIDDescriptorFactory.Create("_RemoveMeAsset"));

        Assert.Null(_RemoveMeAssetAfter);
    }
}
