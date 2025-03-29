using CimBios.Core.CimModel.CimDataModel;
using CimBios.Core.CimModel.CimDatatypeLib;
using CimBios.Core.CimModel.CimDatatypeLib.OID;
using CimBios.Core.CimModel.CimDifferenceModel;
using CimBios.Core.CimModel.DataModel.Utils;
using CimBios.Core.CimModel.RdfSerializer;
using CimBios.Core.RdfIOLib;
using CimBios.Tests.Infrastructure;

namespace CimBios.Tests.DifferenceModel;

public class LoadTest
{
    [Fact]
    public void LoadAndSaveDifferenceModel()
    {
        var cimDifferenceModel = ModelLoader.LoadCimDiffModel_v1()
            as CimDifferenceModel;

        Assert.NotNull(cimDifferenceModel);

        Assert.True(CheckLoadedModel(cimDifferenceModel));

        var lDiffsProfile = cimDifferenceModel.Differences
            .Select(d => (d.OID, d.ModifiedProperties.Count)).ToList();
        
        cimDifferenceModel.Save("~$tmpdiff-LoadTest.xml", new RdfXmlSerializerFactory() 
        { 
            Settings = new RdfSerializerSettings()
            { 
                WritingIRIMode = RdfIRIModeKind.ID 
            } 
        });

        cimDifferenceModel.Load("~$tmpdiff-LoadTest.xml", new RdfXmlSerializerFactory());
        
        Assert.True(CheckLoadedModel(cimDifferenceModel));

        var rDiffsProfile = cimDifferenceModel.Differences
            .Select(d => (d.OID, d.ModifiedProperties.Count)).ToList();

        Assert.True(lDiffsProfile.Intersect(rDiffsProfile).Count() 
            == lDiffsProfile.Count);
    }

    [Fact]
    public void CompareModels()
    {
        var originModel = ModelLoader.LoadCimModel_v1() as CimDocument;
        Assert.NotNull(originModel);

        var modifiedModel = ModelLoader.LoadCimModel_v1_changed() as CimDocument;
        Assert.NotNull(modifiedModel);

        var diffSchema = ModelLoader.Load552HeadersCimRdfSchema();
        var cimDifferenceModel = new CimDifferenceModel(
            diffSchema,
            new CimDatatypeLib(diffSchema),
            new TextDescriptorFactory()
        );

        cimDifferenceModel.CompareDataModels(originModel, modifiedModel);
    }

    [Fact]
    public void LoadApplySaveFullModel()
    {
        var cimDifferenceModel = ModelLoader.LoadCimDiffModel_v1()
            as CimDifferenceModel;

        Assert.NotNull(cimDifferenceModel);

        Assert.True(CheckLoadedModel(cimDifferenceModel));

        var cimDocument = ModelLoader.LoadCimModel_v1() as CimDocument;
        
        Assert.NotNull(cimDocument);

        cimDocument.ApplyDifferenceModel(cimDifferenceModel);
        cimDocument.Save("~$tmpdiff-LoadApplySave.xml", new RdfXmlSerializerFactory() 
        { 
            Settings = new RdfSerializerSettings()
            { 
                WritingIRIMode = RdfIRIModeKind.ID
            } 
        });

        var modifiedModel = ModelLoader.LoadCimModel_v1_changed() as CimDocument;
        Assert.NotNull(modifiedModel);

        var diffSchema = ModelLoader.Load552HeadersCimRdfSchema();
        var cimDifferenceModelCheck = new CimDifferenceModel(
            diffSchema,
            new CimDatatypeLib(diffSchema),
            new TextDescriptorFactory()
        );

        cimDifferenceModelCheck.CompareDataModels(modifiedModel, cimDocument);  
        Assert.Empty(cimDifferenceModelCheck.Differences);
    }

    [Fact]
    public void LoadSubscribeSaveDifferenceModel()
    {
        var cimDocument = ModelLoader.LoadCimModel_v1() as CimDocument;
        Assert.NotNull(cimDocument);

        var diffSchema = ModelLoader.Load552HeadersCimRdfSchema();
        var cimDifferenceModel = new CimDifferenceModel(
            diffSchema,
            new CimDatatypeLib(diffSchema),
            cimDocument
        );

        var applyDM = ModelLoader.LoadCimDiffModel_v1();
        cimDocument.ApplyDifferenceModel(applyDM);

        cimDifferenceModel.Save("~$tmpdiff-LoadSubscribeSave.xml", 
        new RdfXmlSerializerFactory() 
        { 
            Settings = new RdfSerializerSettings()
            { 
                WritingIRIMode = RdfIRIModeKind.ID
            } 
        });
    }

    private bool CheckLoadedModel(ICimDifferenceModel cimDifferenceModel)
    {
        if (cimDifferenceModel.ModelDescription == null
            || cimDifferenceModel.ModelDescription.OID.ToString()
                != "_DifferenceModelHeader")
        {
            throw new InvalidDataException("dm header is not defined!");
        }

        if (cimDifferenceModel.Differences.Count == 0)
        {
            throw new InvalidDataException("diffs set is empty!");
        }

        return true;
    }
}
