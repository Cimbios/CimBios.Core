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
    public class CompaundValidation : SchemaValidationRuleBase
    {
        /// <summary>
        /// Объект из фрагмента
        /// </summary>
        private IModelObject? _modelObject;

        /// <inheritdoc/>
        public override IEnumerable<ValidationResult> Execute(
            IModelObject modelObject)
        {
            _modelObject = modelObject;

            return new List<ValidationResult>()
            {
                GetValidationResults()
            };
        }

        /// <summary>
        /// Конструктор EnumValidation
        /// </summary>
        /// <param name="schema">Каноническая схема CIM</param>
        public CompaundValidation(ICimSchema schema) : base(schema)
        {

        }

        /// <summary>
        /// Проверка на преречисление объекта CIM
        /// </summary>
        /// <returns>Результат проверки</returns>
        private ValidationResult GetValidationResults()
        {
            var cimMetaClass = _modelObject?.MetaClass;

            if (cimMetaClass != null && cimMetaClass.IsCompound) 
                return new ValidationResult()
            {
                ResultType = ValidationResultType.pass,
            };
            else return new ValidationResult()
            {
                Message = $"Класс \"{cimMetaClass?.ShortName}\" " +
                $"не является вложенным классом",
                ResultType = ValidationResultType.fail,
                ModelObject = _modelObject
            };
        }
    }
}
