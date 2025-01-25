namespace CimBios.Tests.Validation;

public class PropertyMultiplicityValidation
{
    [Fact]
    public void StrictlyOne()
    {
        var cimDocument = CimDocumentLoader.LoadCimDocument(
            "../../../assets/testACLineSeriesSection.xml",
            "../../../../../assets/cimrdfs_schemas/cimbios-cim17-RUCIM.rdfs");


        // TODO
    }

    [Fact]
    public void OneN()
    {

    }
}