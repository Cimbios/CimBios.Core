using CimBios.Core.CimModel.Validation;

namespace CimBios.Tests.Validation;

public class PropertyMultiplicityValidation
{
    [Fact]
    public void StrictlyOne()
    {
        var isCorrectValid = IsValidMultiplicity(
            "testReactiveCapabilityCurve");

        Assert.True(isCorrectValid);
    }

    [Fact]
    public void OneN()
    {
        var isCorrectValid = IsValidMultiplicity(
            "testACLineSeriesSection");

        Assert.True(isCorrectValid);
    }

    private bool IsValidMultiplicity(string inputData)
    {
        var cimDocument = CimDocumentLoader.LoadCimDocument(
            $"../../../assets/{inputData}.xml",
            "../../../../../assets/cimrdfs_schemas/cimbios-cim17-RUCIM.rdfs");

        var objects = cimDocument.GetAllObjects().Where(x => x != null);

        var manager = new ValidationManager();

        var rules = manager.GetValidationRules;

        var multiplicityRule = rules.Where(
            r => r.GetType() ==
            typeof(PropertyMultiplicityValidationRule)
        ).First();

        var resultValidation = objects.Select(multiplicityRule.Execute);

        var isCorrectValid = resultValidation.Any(
            x =>
            {
                var correctResult = x.Where(
                    y => y.ResultType == ValidationResultKind.Pass);
                return correctResult != null;
            }
        );

        return isCorrectValid;
    }
}