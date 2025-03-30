using CimBios.Core.CimModel.CimDatatypeLib;
using CimBios.Core.CimModel.CimDatatypeLib.CIM17Types;
using CimBios.Core.CimModel.CimDifferenceModel;
using CimBios.Core.CimModel.CimDatatypeLib.OID;
using CimBios.Tests.Infrastructure;

namespace CimBios.Tests.DifferenceModel;

public class ExtractFromDataModelTest
{
    [Fact]
    public void AddModelObject()
    {
        var diffSchema = ModelLoader.Load552HeadersCimRdfSchema();

        var cimDifferenceModel = new CimDifferenceModel(diffSchema, 
            new CimDatatypeLib(diffSchema), new TextDescriptorFactory());

        var cimDocument = ModelLoader.CreateCimModelInstance();
        cimDifferenceModel.SubscribeToDataModelChanges(cimDocument);

        cimDocument.CreateObject<Substation>(new TextDescriptor("test1"));

        Assert.Contains(
            cimDifferenceModel.Differences
                .OfType<AdditionDifferenceObject>(),
            d => (TextDescriptor)d.OID == "test1"
        );
    }

    [Fact]
    public void UpdateModelObjectAttribute()
    {
        var diffSchema = ModelLoader.Load552HeadersCimRdfSchema();

        var cimDifferenceModel = new CimDifferenceModel(diffSchema, 
            new CimDatatypeLib(diffSchema), new TextDescriptorFactory());

        var cimDocument = ModelLoader.CreateCimModelInstance();

        var substation = cimDocument.CreateObject<Substation>(
            new TextDescriptor("test1"));

        cimDifferenceModel.SubscribeToDataModelChanges(cimDocument);

        substation.name = "Test name";
       
        Assert.Contains(
            cimDifferenceModel.Differences
                .OfType<UpdatingDifferenceObject>(),
            d => (TextDescriptor)d.OID == "test1" && d.ModifiedObject
                .GetAttribute<string>("name") == "Test name"
        );
    }

    [Fact]
    public void UpdateModelObjectCyclic()
    {
        var diffSchema = ModelLoader.Load552HeadersCimRdfSchema();

        var cimDifferenceModel = new CimDifferenceModel(diffSchema, 
            new CimDatatypeLib(diffSchema), new TextDescriptorFactory());

        var cimDocument = ModelLoader.CreateCimModelInstance();

        var substation = cimDocument.CreateObject<Substation>(
            new TextDescriptor("test1"));

        var voltageLevel = cimDocument.CreateObject<VoltageLevel>(
            new TextDescriptor("voltageLevel1"));
        
        var asset = cimDocument.CreateObject<Asset>(
            new TextDescriptor("asset1"));

        var asset2 = cimDocument.CreateObject<Asset>(
            new TextDescriptor("asset2"));
        var inUseDate2 = asset2.InitializeCompoundAttribute("inUseDate")
            as InUseDate;
        Assert.NotNull(inUseDate2);
        inUseDate2.inUseDate = DateTime.Parse("2000-01-01T00:00:00Z");

        cimDifferenceModel.SubscribeToDataModelChanges(cimDocument);

        // change primitive attribute
        substation.name = "Test name";
        substation.name = "Test name2";
        substation.name = null;

        // add remove object
        var bay = cimDocument.CreateObject<Bay>(new TextDescriptor("bay"));  
        cimDocument.RemoveObject(bay);

        // change assoc
        voltageLevel.Substation = substation;
        voltageLevel.Substation = null;

        // change compound attribute
        var inUseDate1 = asset.InitializeCompoundAttribute("inUseDate");
        asset.inUseDate = null;
        
        inUseDate2.inUseDate = DateTime.Now;
        inUseDate2.inUseDate = DateTime.Parse("2000-01-01T00:00:00Z");
       
        Assert.Empty(cimDifferenceModel.Differences);
    }

    [Fact]
    public void UpdateModelObjectAssocs()
    {
        var diffSchema = ModelLoader.Load552HeadersCimRdfSchema();

        var cimDifferenceModel = new CimDifferenceModel(diffSchema, 
            new CimDatatypeLib(diffSchema), new TextDescriptorFactory());

        var cimDocument = ModelLoader.CreateCimModelInstance();

        var substation = cimDocument.CreateObject<Substation>(
            new TextDescriptor("test1"));
        var voltageLevel = cimDocument.CreateObject<VoltageLevel>(
            new TextDescriptor("test2"));
        cimDifferenceModel.SubscribeToDataModelChanges(cimDocument);

        voltageLevel.Substation = substation;
       
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
        var diffSchema = ModelLoader.Load552HeadersCimRdfSchema();

        var cimDifferenceModel = new CimDifferenceModel(diffSchema, 
            new CimDatatypeLib(diffSchema), new TextDescriptorFactory());

        var cimDocument = ModelLoader.CreateCimModelInstance();
        cimDifferenceModel.SubscribeToDataModelChanges(cimDocument);

        var substation = cimDocument.CreateObject<Substation>(
            new TextDescriptor("test1"));

        substation.name = "Test name";
        substation.name = "New name";
       
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
        var diffSchema = ModelLoader.Load552HeadersCimRdfSchema();

        var cimDifferenceModel = new CimDifferenceModel(diffSchema, 
            new CimDatatypeLib(diffSchema), new TextDescriptorFactory());

        var cimDocument = ModelLoader.CreateCimModelInstance();

        var terminal = cimDocument.CreateObject<Terminal>(
            new TextDescriptor("test1"));

        terminal.name = "Test name";
        terminal.sequenceNumber = 2;
        cimDifferenceModel.SubscribeToDataModelChanges(cimDocument);

        cimDocument.RemoveObject(terminal);

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
        var diffSchema = ModelLoader.Load552HeadersCimRdfSchema();

        var cimDifferenceModel = new CimDifferenceModel(diffSchema, 
            new CimDatatypeLib(diffSchema), new TextDescriptorFactory());

        var cimDocument = ModelLoader.CreateCimModelInstance();

        var terminal = cimDocument.CreateObject<Terminal>(
            new TextDescriptor("test1"));
        terminal.name = "Test name";
        cimDifferenceModel.SubscribeToDataModelChanges(cimDocument);

        terminal.name = "New name";
        cimDocument.RemoveObject(terminal);

        Assert.Contains(
            cimDifferenceModel.Differences
                .OfType<DeletionDifferenceObject>(),
            d => (TextDescriptor)d.OID == "test1" 
                && d.ModifiedObject.GetAttribute<string>("name") == "Test name"
        );
    }
}

