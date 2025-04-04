using CimBios.Core.CimModel.CimDatatypeLib;

namespace CimBios.Core.CimModel.Validation;

public class ScriptValidationRule : ValidationRuleBase
{
    public override IEnumerable<IValidationResult> Execute(
        IReadOnlyModelObject modelObject)
    {
        throw new NotImplementedException();
    }

    public override bool NeedExecute(IReadOnlyModelObject modelObject) => true;
}
