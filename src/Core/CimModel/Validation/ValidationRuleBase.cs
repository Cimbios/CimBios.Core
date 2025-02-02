using CimBios.Core.CimModel.CimDatatypeLib;
using CimBios.Core.CimModel.Schema;

namespace CimBios.Core.CimModel.Validation;

public abstract class ValidationRuleBase : IValidationRule
{
    /// <inheritdoc/>
    public abstract IEnumerable<ValidationResult> Execute(
        IModelObject modelObject);

    /// <summary>
    /// Конструктор класса ValidationRuleBase
    /// </summary>
    protected ValidationRuleBase()
    {

    }

    /// <summary>
    /// Get any value (attribute or assoc) of model object by meta property.
    /// </summary>
    /// <param name="modelObject">Model object instance.</param>
    /// <param name="property">Meta property.</param>
    /// <returns>Object value or null if property value does not exist.</returns>
    protected object? GetPropertyValueAsObject(
        IModelObject modelObject, ICimMetaProperty property)
    {
        switch (property.PropertyKind)
        {
            case CimMetaPropertyKind.Attribute:
                return modelObject.
                    GetAttribute(property);
            case CimMetaPropertyKind.Assoc1To1:
                return modelObject.
                    GetAssoc1ToM(property);
            case CimMetaPropertyKind.Assoc1ToM:
                return modelObject.
                    GetAssoc1ToM(property);
        }

        return null;
    }
}
