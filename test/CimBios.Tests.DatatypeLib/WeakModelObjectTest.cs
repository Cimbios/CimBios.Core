using CimBios.Core.CimModel.CimDatatypeLib;
using CimBios.Core.CimModel.CimDatatypeLib.CIM17Types;
using CimBios.Core.CimModel.CimDatatypeLib.OID;
using CimBios.Tests.Infrastructure;

namespace CimBios.Tests.DatatypeLib;

public class WeakModelObjectTests
{
    [Fact]
    public void UnkClassCreating()
    {
        var cimModel = ModelLoader.LoadCimModel_v1(true);

        var checkObject = cimModel.GetObject<WeakModelObject>(
            cimModel.OIDDescriptorFactory.Create("_Dummy")
        );
            
        Assert.NotNull(checkObject);
    }

    [Fact]
    public void JustCheckKnownClassCreating()
    {
        var cimModel = ModelLoader.LoadCimModel_v1();

        var checkObject = cimModel
            .GetObject<Terminal>("_APTEndLVT1");
            
        Assert.NotNull(checkObject);
    }

    [Fact]
    public void UnkClassKnownProperty()
    {
        var cimModel = ModelLoader.LoadCimModel_v1(true);

        var checkObject = cimModel.GetObject<WeakModelObject>("_Dummy");

        var checkAttrName = checkObject?.GetAttribute("name");
            
        Assert.NotNull(checkAttrName);
    }    

    [Fact]
    public void UnkClassUnknownProperty()
    {
        var cimModel = ModelLoader.LoadCimModel_v1(true);

        var checkObject = cimModel.GetObject<WeakModelObject>("_Dummy");
            
        var checkAttrName = checkObject?.GetAttribute("prop");
            
        Assert.NotNull(checkAttrName);
    }  

    [Fact]
    public void ClassUnkRefAsAssoc()
    {
        var cimModel = ModelLoader.LoadCimModel_v1(true);

        var checkObject = cimModel.GetObject<Substation>("_SubstationA");
            
        var checkEnumAssocType = checkObject?.GetAssoc1ToM("UndefinedAssoc");
            
        Assert.IsType<ModelObjectUnresolvedReference>(checkEnumAssocType?.Single());
    }  

    [Fact]
    public void UnkCompound()
    {
        var cimModel = ModelLoader.LoadCimModel_v1(true);

        var checkObject = cimModel.GetObject<Asset>("_ASubstationAsset");
            
        var checkCompound = checkObject?.GetAttribute<IModelObject>("inUseDate");

        Assert.IsType<InUseDate>(checkCompound);
        Assert.True(checkCompound.OID is AutoDescriptor or TextDescriptor);
    }

    [Fact]
    public void InitializeWeakCompound()
    {
        var cimModel = ModelLoader.LoadCimModel_v1(true);

        var checkObject = cimModel.GetObject<WeakModelObject>(
            cimModel.OIDDescriptorFactory.Create("_Dummy")
        );

        Assert.NotNull(checkObject);

        var autoCompound = checkObject
            .InitializeCompoundAttribute("autoCompound");

        autoCompound.SetAttribute("attr", 888);
    }
}
