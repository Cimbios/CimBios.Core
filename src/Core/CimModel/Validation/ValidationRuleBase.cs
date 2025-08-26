using CimBios.Core.CimModel.CimDatatypeLib;
using CimBios.Core.CimModel.Schema;

namespace CimBios.Core.CimModel.Validation;

public abstract class ValidationRuleBase : IValidationRule
{
    /// <summary>
    ///     Конструктор класса ValidationRuleBase
    /// </summary>
    protected ValidationRuleBase()
    {
    }

    /// <inheritdoc />
    public abstract IEnumerable<IValidationResult> Execute(
        IReadOnlyModelObject modelObject);

    /// <inheritdoc />
    public abstract bool NeedExecute(IReadOnlyModelObject modelObject);
}

///
internal static class GetGenericPropExtension
{
    /// <summary>
    ///     Get any value (attribute or assoc) of model object by meta property.
    /// </summary>
    /// <param name="modelObject">Model object instance.</param>
    /// <param name="property">Meta property.</param>
    /// <returns>Object value or null if property value does not exist.</returns>
    internal static object? GetPropertyValueAsObject(
        this IReadOnlyModelObject modelObject, ICimMetaProperty property)
    {
        return property.PropertyKind switch
        {
            CimMetaPropertyKind.Attribute => modelObject.GetAttribute(property),
            CimMetaPropertyKind.Assoc1To1 => modelObject.GetAssoc1To1(property),
            CimMetaPropertyKind.Assoc1ToM => modelObject.GetAssoc1ToM(property),
            _ => null
        };
    }
}