using CimBios.Core.CimModel.CimDatatypeLib.CIM17Types;
using CimBios.Core.CimModel.CimDatatypeLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CimBios.Core.CimModel.Validation.UserCustomRules
{
    public class SubstationVoltageLevelCustomValidationRule : ValidationRuleBase
    {
        public override bool NeedExecute(IReadOnlyModelObject modelObject) =>
            modelObject is VoltageLevel;

        public override IEnumerable<IValidationResult> Execute(
            IReadOnlyModelObject modelObject)
        {
            var vl = (VoltageLevel)modelObject;
            var results = new List<IValidationResult>();

            if (vl.BaseVoltage == null)
            {
                results.Add(new ModelObjectValidationResult(
                    ValidationResultKind.Fail,
                        $"VoltageLevel {vl.OID} " +
                        $"has null BaseVoltage. " +
                        $"Related Substation: {vl.Substation?.OID}", vl)
                        );
            }

            if (results.Count == 0)
                results.Add(new PassValidationResult());

            return results;
        }
    }
}
