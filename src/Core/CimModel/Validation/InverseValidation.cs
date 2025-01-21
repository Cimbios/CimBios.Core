using CimBios.Core.CimModel.CimDatatypeLib;
using CimBios.Core.CimModel.Schema;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CimBios.Core.CimModel.Validation
{
    [AttributeValidation]
    public class InverseValidation : SchemaValidationRuleBase
    {
        /// <summary>
        /// Объект из фрагмента
        /// </summary>
        private IModelObject? _modelObject;

        /// <summary>
        /// Ассоциации объекта из схемы (RDFS)
        /// </summary>
        private List<CimMetaPropertyKind> _assoc =
            new List<CimMetaPropertyKind>();

        /// <inheritdoc/>
        public override IEnumerable<ValidationResult> Execute(
            IModelObject modelObject)
        {
            _modelObject = modelObject;

            var allProperties = _modelObject.MetaClass.AllProperties;

            return new List<ValidationResult>()
            {
                GetValidationResults(allProperties)
            };
        }

        /// <summary>
        /// Конструктор InverseValidation
        /// </summary>
        /// <param name="modelObject">Объект CIM из фрагмента</param>
        /// <param name="schema">Каноническая схема для проверки</param>
        public InverseValidation(ICimSchema schema) : base(schema)
        {

        }

        /// <summary>
        /// Получение ассоциаций без обратной связи из свойств
        /// </summary>
        /// <param name="cimMetaProperties">Свойства объекта CIM</param>
        /// <returns>Ассоциации без обратной связи объекта CIM</returns>
        private IEnumerable<ICimMetaProperty> GetNonValidAssoc(
            IEnumerable<ICimMetaProperty> cimMetaProperties)
        {
            var assoc = cimMetaProperties.Where(
                x => x.PropertyKind == CimMetaPropertyKind.Assoc1To1 || 
                x.PropertyKind == CimMetaPropertyKind.Assoc1ToM);

            return assoc.Where(a => a.InverseProperty == null ? false : true);
        }

        /// <summary>
        /// Проверка объекта CIM 
        /// на отсутствие двусторонних связей у ассоциаций
        /// </summary>
        /// <returns>Результат проверки</returns>
        private ValidationResult GetValidationResults(
            IEnumerable<ICimMetaProperty> allProperties)
        {
            var assoc = GetNonValidAssoc(allProperties);

            if (assoc.Count() == 0) 
                return new ValidationResult()
                {
                    ResultType = ValidationResultType.pass,
                };

            return new ValidationResult()
            {
                Message = $"Класс \"{_modelObject?.MetaClass?.ShortName}\" " +
                $"содержит следующие ассоциации без двусторонней связи: " +
                $"{string.Join(", ", assoc)}",
                ResultType = ValidationResultType.fail,
                ModelObject = _modelObject
            };
        }
    }
}
