using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CimBios.Core.CimModel.Validation.UserCustomRules
{
    /// 
    public class CustomValidationRulesBuilder
    {
        public static ICollection<IValidationRule> GetRules() =>
            [
                new ACLineSegmentCustomValidationRule(),
                //
            ];
    }
}
