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
    // Почему OwnerClass у всех свойств, а не только у атрибутов!!!
    [AttributeValidation]
    public class AttributeOwnerValidation : SchemaValidationRuleBase
    {
        /// <summary>
        /// Объект из фрагмента
        /// </summary>
        private IModelObject _modelObject;

        /// <inheritdoc/>
        public override ICollection<ValidationResult> Execute(IModelObject modelObject)
        {
            var schemaClass = Schema.Classes.Where(
                x => x == _modelObject.MetaClass).FirstOrDefault();

            var propertySchema = Schema.Properties.Where(
                x => x.OwnerClass == schemaClass);

            var propertyFragment = 
                _modelObject.MetaClass.AllProperties.ToList();

            return new List<ValidationResult>()
            {
                GetValidationResults(propertySchema, propertyFragment)
            };
        }

        /// <summary>
        /// Конструктор AttributeOwnerValidation
        /// </summary>
        /// <param name="modelObject">Объект CIM из фрагмента</param>
        /// <param name="schema">Каноническая схема для проверки</param>
        public AttributeOwnerValidation(IModelObject modelObject,
            ICimSchema schema) : base(schema)
        {
            _modelObject = modelObject;
        }

        /// <summary>
        /// Получение атрибутов из свойств
        /// </summary>
        /// <param name="cimMetaProperties">Свойства объекта CIM</param>
        /// <returns>Атрибуты объекта CIM</returns>
        private IEnumerable<CimMetaPropertyKind> GetAttribute(
            IEnumerable<ICimMetaProperty> cimMetaProperties)
        {
            var attributes = cimMetaProperties.Where(
                x => x.PropertyKind == CimMetaPropertyKind.Attribute);

            foreach (var attr in attributes) yield return attr.PropertyKind;
        }

        /// <summary>
        /// Проверка объекта CIM 
        /// на соответствие владельца атрибута
        /// </summary>
        /// <returns>Результат проверки</returns>
        private ValidationResult GetValidationResults(
            IEnumerable<ICimMetaProperty> schemaProperties,
            IEnumerable<ICimMetaProperty> fragmentProperties)
        {
            var failPropertyOwner = new List<ICimMetaProperty>(); 

            foreach (var sProp in schemaProperties)
            {
                foreach (var fProp in fragmentProperties)
                {
                    if (sProp != fProp) continue;

                    if (sProp.OwnerClass != fProp.OwnerClass)
                    {
                        failPropertyOwner.Add(fProp);
                    }
                }
            }

            if (failPropertyOwner.Count() == 0) return new ValidationResult()
            {
                ResultType = ValidationResultType.pass,
            };
            else return new ValidationResult()
            {
                Message = $"Класс \"{_modelObject.MetaClass.ShortName}\" " +
                $"содержит следующие атрибуты, у которых владелец атрибута " +
                $"не совпадает со схемой (RDFS): " +
                $"{string.Join(", ", failPropertyOwner)}",
                ResultType = ValidationResultType.fail,
                ModelObject = _modelObject
            };
        }
    }
}
