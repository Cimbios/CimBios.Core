using CimBios.Core.CimModel.CimDifferenceModel;
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
