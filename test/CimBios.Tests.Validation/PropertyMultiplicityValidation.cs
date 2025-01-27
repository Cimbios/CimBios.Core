using CimBios.Core.CimModel.Validation;

namespace CimBios.Tests.Validation;

public class PropertyMultiplicityValidation
{
    [Fact]
    public void StrictlyOne()
    {
        var cimDocument = CimDocumentLoader.LoadCimDocument(
            "../../../assets/testMetrologyRequirement.xml",
            "../../../../../assets/cimrdfs_schemas/cimbios-cim17-RUCIM.rdfs");


        var a = cimDocument.GetAllObjects();

        var obj = a.FirstOrDefault();

        if (obj == null)
        {

        }

        var aa = obj.MetaClass.AllProperties.FirstOrDefault().PropertyKind;

        var propertyRule = new PropertyMultiplisityValidationRule();

        var result = propertyRule.Execute(obj).ToList();

        Assert.NotNull(result);
    }

    [Fact]
    public void OneN()
    {

    }
}