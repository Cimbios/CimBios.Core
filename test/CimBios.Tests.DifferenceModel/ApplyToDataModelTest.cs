using CimBios.Core.CimModel.CimDatatypeLib;
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

    [Fact]
    public void UpdateAttribute()
    {
        var cimDifferenceModel = ModelLoader.LoadCimDiffModel_v1();
        var cimDocument = ModelLoader.LoadCimModel_v1(); 

        cimDocument.ApplyDifferenceModel(cimDifferenceModel);

        // existing attr
        var _SubstationA = cimDocument.GetObject<Substation>(
            cimDocument.OIDDescriptorFactory.Create("_SubstationA"));
        Assert.NotNull(_SubstationA);
        Assert.Equal("Substation A [RENAMED]", _SubstationA.name);

        // not existing attr
        var _ABreaker110 = cimDocument.GetObject<Breaker>(
            cimDocument.OIDDescriptorFactory.Create("_ABreaker110"));
        Assert.NotNull(_ABreaker110);
        Assert.Equal(true, _ABreaker110.normalOpen);

        // remove attr value
        var _ACurrentTransformer1 = cimDocument.GetObject<CurrentTransformer>(
            cimDocument.OIDDescriptorFactory.Create("_ACurrentTransformer1"));
        Assert.NotNull(_ACurrentTransformer1);
        Assert.Null(_ACurrentTransformer1.isEmbeded);

        // enum attr
        var _APTEndMVT1 = cimDocument.GetObject<Terminal>(
            cimDocument.OIDDescriptorFactory.Create("_APTEndMVT1"));
        Assert.NotNull(_APTEndMVT1);
        Assert.Equal(PhaseCode.ABC, _APTEndMVT1.phases);

        // change compound attr
        var _ASubstationAsset = cimDocument.GetObject<Asset>(
            cimDocument.OIDDescriptorFactory.Create("_ASubstationAsset"));
        Assert.NotNull(_ASubstationAsset);
        Assert.NotNull(_ASubstationAsset.inUseDate);
        Assert.Equal(DateTime.Parse("1995-01-01T00:00:00Z"),
            _ASubstationAsset.inUseDate.inUseDate);  

        // create compound attr
        var _JustLonelyAsset = cimDocument.GetObject<Asset>(
            cimDocument.OIDDescriptorFactory.Create("_JustLonelyAsset"));
        Assert.NotNull(_JustLonelyAsset);
        Assert.NotNull(_JustLonelyAsset.inUseDate);
        Assert.Equal(DateTime.Parse("2000-01-01T00:00:00Z"),
            _JustLonelyAsset.inUseDate.inUseDate);   
    }

    [Fact]
    public void UpdateAssocs()
    {
        var cimDifferenceModel = ModelLoader.LoadCimDiffModel_v1();
        var cimDocument = ModelLoader.LoadCimModel_v1(); 

        cimDocument.ApplyDifferenceModel(cimDifferenceModel);

        // new obj with ref on existing
        var _NewALoadT1 = cimDocument.GetObject<Terminal>(
            cimDocument.OIDDescriptorFactory.Create("_NewALoadT1"));
        Assert.NotNull(_NewALoadT1);

        var _ACN1 = cimDocument.GetObject<ConnectivityNode>(
            cimDocument.OIDDescriptorFactory.Create("_ACN1"));
        Assert.NotNull(_ACN1);

        Assert.Equal(_ACN1, _NewALoadT1.ConnectivityNode);
        Assert.Contains(_NewALoadT1, _ACN1.Terminals);

        // unresolved replacing check
        Assert.Equal(3, _ACN1.GetAssoc1ToM("Terminals").Length);
        Assert.Equal(3, _ACN1.GetAssoc1ToM("Terminals").OfType<ModelObject>().Count());

        // null existing
        var _AGroundDisconnector110 = cimDocument.GetObject<GroundDisconnector>(
            cimDocument.OIDDescriptorFactory.Create("_AGroundDisconnector110"));
        Assert.NotNull(_AGroundDisconnector110);

        var _AInputBay110 = cimDocument.GetObject<Bay>(
            cimDocument.OIDDescriptorFactory.Create("_AInputBay110"));
        Assert.NotNull(_AInputBay110);       

        Assert.Null(_AGroundDisconnector110.EquipmentContainer);
        Assert.DoesNotContain(_AGroundDisconnector110, _AInputBay110.Equipments);
    }
}
