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
    public class DataTypeValidation : SchemaValidationRuleBase
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
        /// Конструктор DataTypeValidation
        /// </summary>
        /// <param name="modelObject">Объект CIM из фрагмента</param>
        /// <param name="schema">Каноническая схема для проверки</param>
        public DataTypeValidation(IModelObject modelObject,
            ICimSchema schema) : base(schema)
        {
            _modelObject = modelObject;
        }

        /// <summary>
        /// Проверка на соответствия тип объекта CIM схеме
        /// </summary>
        /// <returns>Результат проверки</returns>
        private ValidationResult GetValidationResults()
        {
            var cimMetaClass = _modelObject.MetaClass;

            var schemaClass = Schema.Classes.Where(
                x => x == _modelObject.MetaClass).FirstOrDefault();

            if (cimMetaClass.BaseUri == schemaClass?.BaseUri) 
                return new ValidationResult()
            {
                ResultType = ValidationResultType.pass,
            };
            else return new ValidationResult()
            {
                Message = $"Тип класса \"{cimMetaClass.ShortName}\" " +
                $"не совпадает со схемой",
                ResultType = ValidationResultType.fail,
                ModelObject = _modelObject
            };
        }
    }
}
