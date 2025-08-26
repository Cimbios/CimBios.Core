using CimBios.Core.CimModel.CimDatatypeLib;
using CimBios.Core.CimModel.Schema;

namespace CimBios.Core.CimModel.Validation;

/// <summary>
///     Validation rule for property multiplicity accordance.
/// </summary>
public class PropertyMultiplicityValidationRule : ValidationRuleBase
{
    /// <inheritdoc />
    public override IEnumerable<IValidationResult> Execute(
        IReadOnlyModelObject modelObject)
    {
        return modelObject.MetaClass.AllProperties
            .Where(p => p.IsValueRequired)
            .Select(p => GetValidationResult(modelObject, p));
    }

    public override bool NeedExecute(IReadOnlyModelObject modelObject)
    {
        return modelObject is ModelObject;
    }

    /// <summary>
    ///     Get validation result.
    /// </summary>
    /// <param name="modelObject">Model object instance.</param>
    /// <param name="property">Meta property.</param>
    /// <returns>Validation result</returns>
    private static IValidationResult GetValidationResult(
        IReadOnlyModelObject modelObject, ICimMetaProperty property)
    {
        var value = modelObject.GetPropertyValueAsObject(property);

        if (value is ICollection<object> collection
                ? collection.Count == 0
                : value == null)
            return new ModelObjectValidationResult(
                ValidationResultKind.Fail,
                "Model object does not contain required value " +
                $"for \"{property}\" property.",
                modelObject, property);

        return new PassValidationResult();
    }
}