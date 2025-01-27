using CimBios.Core.CimModel.CimDatatypeLib;
using CimBios.Core.CimModel.Schema;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CimBios.Core.CimModel.Validation
{
    [AttributeValidation]
    public class PropertyMultiplisityValidationRule : ValidationRuleBase
    {
        /// <inheritdoc/>
        public override IEnumerable<ValidationResult> Execute(
            IModelObject modelObject)
        {
            var multiplisityRequied = MultiplisityRequired(modelObject).ToList();

            foreach (var or in multiplisityRequied)
            {
                yield return ValidationResults(or, modelObject);
            }
        }

        private ValidationResult ValidationResults(
            ICimMetaProperty property, IModelObject modelObject)
        {
            return GetPropertiesValue(property, modelObject) == null
                ? new ValidationResult()
                {
                    Message = $"Атрибут / ассоцияция: {property} " +
                    "не удовлетворяет требованиям множественности",
                    ResultType = ValidationResultKind.Fail,
                    ModelObject = modelObject
                }
                : new ValidationResult()
                {
                    Message = "Ошибки отсутствуют",
                    ResultType = ValidationResultKind.Pass,
                    ModelObject = modelObject
                };
        }

        public object? GetPropertiesValue(
            ICimMetaProperty propertiesRequied, 
            IModelObject modelObject)
        {
            switch (propertiesRequied.PropertyKind)
            {
                case CimMetaPropertyKind.Attribute:
                    return modelObject.
                        GetAttribute(propertiesRequied);
                case CimMetaPropertyKind.Assoc1ToM:
                    return modelObject.
                        GetAssoc1ToM(propertiesRequied);
            }
            return null;
        }

        private IEnumerable<ICimMetaProperty> MultiplisityRequired(
            IModelObject modelObject)
        {
            var properties = modelObject.MetaClass.AllProperties;

            foreach (var property in properties)
            {
                if(property.IsValueRequired)
                {
                    yield return property;
                }
            }
        }
    }
}
