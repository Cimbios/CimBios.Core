using CimBios.Core.CimModel.CimDatatypeLib.CIM17Types;
using CimBios.Core.CimModel.CimDatatypeLib;

namespace CimBios.Core.CimModel.Validation.UserCustomRules
{
    public class ACLineSegmentCustomValidationRule : ValidationRuleBase
    {
        public override bool NeedExecute(IReadOnlyModelObject modelObject) =>
            modelObject is ACLineSegment;

        public override IEnumerable<IValidationResult> Execute(
            IReadOnlyModelObject modelObject)
        {
            var ac = (ACLineSegment)modelObject;
            var results = new List<IValidationResult>();

            if (!(ac.BaseVoltage?.nominalVoltage.HasValue == true &&
                 ac.BaseVoltage.nominalVoltage.Value >= 110f))
            {
                results.Add(new PassValidationResult());
                return results;
            }

            if (ac.r0 == null)
                results.Add(new ModelObjectValidationResult(
                    ValidationResultKind.Fail, 
                        $"ACLineSegment {ac.OID} has null r0", ac)
                    );
            else if (ac.r0 > 1)
                results.Add(new ModelObjectValidationResult(
                    ValidationResultKind.Fail,
                        $"ACLineSegment {ac.OID} has invalid r0 {ac.r0}", ac)
                    );

            if (ac.x0 == null)
                results.Add(new ModelObjectValidationResult(
                    ValidationResultKind.Fail, 
                        $"ACLineSegment {ac.OID} has null x0", ac)
                    );
            else if (ac.x0 > 1)
                results.Add(new ModelObjectValidationResult(
                    ValidationResultKind.Fail, 
                    $"ACLineSegment {ac.OID} has invalid x0 {ac.x0}", ac)
                    );

            if (results.Count == 0)
                results.Add(new PassValidationResult());

            return results;
        }
    }
}
