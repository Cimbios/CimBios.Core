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
    public class EnumValidation : SchemaValidationRuleBase
    {
        /// <summary>
        /// Объект из фрагмента
        /// </summary>
        private IModelObject _modelObject;

        /// <inheritdoc/>
        public override IEnumerable<ValidationResult> Execute(IModelObject modelObject)
        {
            return new List<ValidationResult>() 
            { 
                GetValidationResults()
            };
        }

        /// <summary>
        /// Конструктор EnumValidation
        /// </summary>
        /// <param name="modelObject">Объект CIM из фрагмента</param>
        /// <param name="schema">Каноническая схема для проверки</param>
        public EnumValidation(IModelObject modelObject,
            ICimSchema schema) : base(schema)
        {
            _modelObject = modelObject;
        }

        /// <summary>
        /// Проверка на преречисление объекта CIM
        /// </summary>
        /// <returns>Результат проверки</returns>
        private ValidationResult GetValidationResults()
        {
            var cimMetaClass = _modelObject.MetaClass;

            if (cimMetaClass.IsEnum) return new ValidationResult()
            {
                ResultType = ValidationResultType.pass,
            };
            else return new ValidationResult()
            {
                Message = $"Класс \"{cimMetaClass.ShortName}\" " +
                $"не является перечислением",
                ResultType = ValidationResultType.fail,
                ModelObject = _modelObject
            };
        }
    }
}
