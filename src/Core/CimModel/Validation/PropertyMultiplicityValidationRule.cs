using CimBios.Core.CimModel.CimDatatypeLib;
using CimBios.Core.CimModel.Schema;

namespace CimBios.Core.CimModel.Validation
{
    [AttributeValidation]
    public class PropertyMultiplicityValidationRule : ValidationRuleBase
    {
        /// <inheritdoc/>
        public override IEnumerable<ValidationResult> Execute(
            IModelObject modelObject)
        {
            var multiplisityRequied = MultiplisityRequired(modelObject);

            foreach (var or in multiplisityRequied)
            {
                yield return ValidationResults(or, modelObject);
            }
        }

        /// <summary>
        /// Рузультаты проверки
        /// </summary>
        /// <param name="property">Свойства объекта</param>
        /// <param name="modelObject">Объект модели</param>
        /// <returns>Результаты проверки</returns>
        private ValidationResult ValidationResults(
            ICimMetaProperty property, IModelObject modelObject)
        {
            return GetPropertiesValue(property, modelObject) == null
                ? new ValidationResult()
                {
                    Message = $"Атрибут / ассоцияция: \"{property}\" " +
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

        /// <summary>
        /// Запрос величины атрибута / ассоциации
        /// </summary>
        /// <param name="propertiesRequied">Свойства объекта 
        /// с необходимой множественностью</param>
        /// <param name="modelObject">Объект модели</param>
        /// <returns>Величину атрибута / ассоциацию</returns>
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
                        GetAssoc1ToM(propertiesRequied).FirstOrDefault();
            }
            return null;
        }

        /// <summary>
        /// Запрос свойств объекта с необходимой множественностью
        /// </summary>
        /// <param name="modelObject">Объект модели</param>
        /// <returns>Свойства объекта с необходимой множественностью</returns>
        private IEnumerable<ICimMetaProperty> MultiplisityRequired(
            IModelObject modelObject)
        {
            var properties = modelObject.MetaClass.AllProperties;

            foreach (var property in properties)
            {
                if (property.IsValueRequired)
                {
                    yield return property;
                }
            }
        }
    }
}
