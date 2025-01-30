using CimBios.Core.CimModel.CimDatatypeLib;
using CimBios.Core.CimModel.Schema;

namespace CimBios.Core.CimModel.Validation
{
    /// <summary>
    /// Validation rule for property multiplicity accordance.
    /// </summary>
    [AttributeValidation]
    public class PropertyMultiplicityValidationRule : ValidationRuleBase
    {
        /// <inheritdoc/>
        public override IEnumerable<ValidationResult> Execute(
            IModelObject modelObject)
            =>  modelObject.MetaClass.AllProperties
                .Where(p => p.IsValueRequired)
                .Select(p => GetValidationResult(modelObject, reqProp));

        /// <summary>
        /// Get validation result.
        /// </summary>
        /// <param name="modelObject">Model object instance.</param>
        /// <param name="property">Meta property.</param>
        /// <returns>Validation result</returns>
        private ValidationResult GetValidationResult(
            IModelObject modelObject, ICimMetaProperty property)
        {
            return GetPropertyValueAsObject(property, modelObject) == null
                ? new ValidationResult()
                {
                    Message = "Model object does not contain reuired value " +
                        $"for \"{property}\" property.",
                    ResultType = ValidationResultKind.Fail,
                    ModelObject = modelObject
                }
                : new ValidationResult()
                {
                    Message = string.Empty,
                    ResultType = ValidationResultKind.Pass,
                    ModelObject = modelObject
                };
        }
    }
}
