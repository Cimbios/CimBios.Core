using CimBios.Core.CimModel.CimDatatypeLib.CIM17Types;
using CimBios.Core.CimModel.CimDatatypeLib;

namespace CimBios.Core.CimModel.Validation.UserCustomRules
{
    public class CurrentTransformerCustomValidationRule : ValidationRuleBase
    {
        public override bool NeedExecute(IReadOnlyModelObject modelObject) =>
            modelObject is CurrentTransformer;

        public override IEnumerable<IValidationResult> Execute(
            IReadOnlyModelObject modelObject)
        {
            var ct = (CurrentTransformer)modelObject;
            var results = new List<IValidationResult>();

            float? val = ct.ratedSecondaryCurrent;

            if (val == null)
            {
                results.Add(new ModelObjectValidationResult(
                    ValidationResultKind.Fail,
                        $"CurrentTransformer {ct.OID} " +
                        $"has null ratedSecondaryCurrent", ct)
                        );
            }
            else
            {
                var validValues = new[] { 1.0f, 5.0f };
                if (!validValues.Any(v => Math.Abs(val.Value - v) < 1e-6f))
                {
                    results.Add(new ModelObjectValidationResult(
                        ValidationResultKind.Fail,
                            $"CurrentTransformer {ct.OID}" +
                            $" has invalid ratedSecondaryCurrent {val}", ct)
                            );
                }
            }

            if (results.Count == 0)
                results.Add(new PassValidationResult());

            return results;
        }
    }
}
