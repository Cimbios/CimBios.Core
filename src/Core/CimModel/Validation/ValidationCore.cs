using System.Collections.Concurrent;
using CimBios.Core.CimModel.CimDataModel;

namespace CimBios.Core.CimModel.Validation;

///
public static class ValidationCimDataModelExtensions
{
    ///
    public static IEnumerable<IValidationResult> Validate(
        this ICimDataModel dataModel, IEnumerable<IValidationRule> rulesSet, 
        bool executeInternalRules = true)
    {
        var results = new BlockingCollection<IValidationResult>();

        var executeRules = new List<IValidationRule>();
        if (executeInternalRules)
        {
            executeRules.AddRange(InternalValidationRulesBuilder.GetRules());
        }

        executeRules.AddRange(rulesSet);

        dataModel.GetAllObjects().AsParallel().ForAll(modelObject =>
        {
            foreach (var rule in executeRules)
            {
                if (rule.NeedExecute(modelObject) == false)
                {
                    continue;
                }

                var executionResults = rule.Execute(modelObject);
                foreach (var result in executionResults
                    .Where(r => r.ResultType != ValidationResultKind.Pass))
                {
                    results.Add(result);
                }
            }
        });

        return results;
    }

    ///
    public static async Task<IEnumerable<IValidationResult>> ValidateAsync(
        this ICimDataModel dataModel, IEnumerable<IValidationRule> rulesSet, 
        bool executeInternalRules = true)
    {
        var result = await Task.Run(
            () => dataModel.Validate(rulesSet, executeInternalRules));

        return result;
    }
}

///
internal static class InternalValidationRulesBuilder
{
    internal static ICollection<IValidationRule> GetRules()
    {
        return
        [
            new PropertyMultiplicityValidationRule(),
            //
        ];
    }
}
