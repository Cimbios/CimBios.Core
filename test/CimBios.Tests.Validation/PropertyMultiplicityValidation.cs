using CimBios.Core.CimModel.Validation;

namespace CimBios.Tests.Validation;

public class PropertyMultiplicityValidation
{
    [Fact]
    public void StrictlyOne()
    {

    }

    [Fact]
    public void OneN()
    {
        var cimDocument = CimDocumentLoader.LoadCimDocument(
            "../../../assets/testACLineSeriesSection.xml",
            "../../../../../assets/cimrdfs_schemas/cimbios-cim17-RUCIM.rdfs");

        var objects = cimDocument.GetAllObjects().Where(x => x != null);

        var manager = new ValidationManager();

        var rules = manager.GetValidationRules;

        var multiplicityRule = rules.Where(
            r => r.GetType() == 
            typeof(PropertyMultiplisityValidationRule)
            ).First();

        var resultValidation = objects.Select(
            o => multiplicityRule.Execute(o));

        var isCorrectValid = resultValidation.All(
            x => x.All(y => y.ResultType == ValidationResultKind.Pass));

        Assert.True(isCorrectValid);
    }
}