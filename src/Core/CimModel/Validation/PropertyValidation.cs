using CimBios.Core.CimModel.CimDatatypeLib;
using CimBios.Core.CimModel.Schema;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace CimBios.Core.CimModel.Validation
{
    [AttributeValidation]
    public class PropertyValidation : SchemaValidationRuleBase
    {
        /// <summary>
        /// Объект из фрагмента
        /// </summary>
        private IModelObject? _modelObject;

        /// <summary>
        /// Свойства объекта из схемы (RDFS)
        /// </summary>
        private IEnumerable<CimMetaPropertyKind>? _propertiesSchemaObject;

        /// <summary>
        /// Свойства объекта из фрагмента
        /// </summary>
        private IEnumerable<ICimMetaProperty>? _cimMetaProperties;

        /// <summary>
        /// Атрибуты объекта из схемы (RDFS)
        /// </summary>
        private List<CimMetaPropertyKind> _validAtributes = 
            new List<CimMetaPropertyKind>();

        /// <summary>
        /// Ассоциации объекта из схемы (RDFS)
        /// </summary>
        private List<CimMetaPropertyKind> _validAssoc = 
            new List<CimMetaPropertyKind>();

        /// <inheritdoc/>
        public override IEnumerable<ValidationResult> Execute(
            IModelObject modelObject)
        {
            _modelObject = modelObject;

            var schemaClass = Schema.Classes.Where(
                x => x == modelObject.MetaClass).FirstOrDefault();

            _propertiesSchemaObject = Schema.Properties.Where(
                x => x.OwnerClass == schemaClass).Select(x => x.PropertyKind);

            _cimMetaProperties = modelObject.MetaClass.AllProperties;

            GetValidProperties();

            var validationResult = TupleResults();

            foreach (var tuple in validationResult)
            {
                if (tuple.CountError != 0)
                {
                    yield return tuple.ValidationResult.Invoke();
                }
            }
        }

        /// <summary>
        /// Конструктор PropertyValidation
        /// </summary>
        /// <param name="schema">Каноническая схема для проверки</param>
        public PropertyValidation(ICimSchema schema) : base(schema)
        {

        }

        /// <summary>
        /// Разделение свойств объекта CIM 
        /// на атрибуты и ассоциации для проверки
        /// </summary>
        private void GetValidProperties()
        {
            if (_cimMetaProperties == null) return;

            foreach (var prop in _cimMetaProperties)
            {
                var validProperty = prop.PropertyKind;
                switch (validProperty)
                {
                    case CimMetaPropertyKind.Attribute:
                        _validAtributes.Add(validProperty);
                        break;
                    case CimMetaPropertyKind.Assoc1To1:
                    case CimMetaPropertyKind.Assoc1ToM:
                        _validAssoc.Add(validProperty);
                        break;
                }
            }
        }

        /// <summary>
        /// Проверка свойств объекта 
        /// на отсутствие атрибута/ассоциации в схеме (RDFS)
        /// </summary>
        /// <param name="validProperties">Свойства объекта из схемы 
        /// для проверки</param>
        /// <returns>Список отсутствующих атрибутов/ассоциаций</returns>
        private IEnumerable<CimMetaPropertyKind?> FailProperties(
            IEnumerable<CimMetaPropertyKind> validProperties)
        {
            foreach (var nonValidProp in validProperties)
            {
                if (_propertiesSchemaObject == null) continue;

                if (!_propertiesSchemaObject.Any(x => x == nonValidProp))
                {
                    yield return nonValidProp;
                }
            }
        }

        /// <summary>
        /// Кортеж для хранения результатов проверки объекта CIM
        /// </summary>
        /// <returns>Результаты проверки объекта CIM</returns>
        private List<(int CountError, Func<ValidationResult> ValidationResult)> TupleResults()
        {
            var tupleResult = new List<(
                int CountError, Func<ValidationResult> ValidationResult)>
            {
                (
                    FailProperties(_validAtributes).Count(),
                    () =>
                    {
                        return GetValidationResults(_validAtributes);
                    }
                ),
                (
                    FailProperties(_validAssoc).Count(),
                    () =>
                    {
                        return GetValidationResults(_validAssoc);
                    }
                ),
            };

            return tupleResult;
        }

        /// <summary>
        /// Проверка на корректность свойств объекта CIM
        /// </summary>
        /// <param name="nonValidProperties">Некорректные свойства 
        /// объекта CIM</param>
        /// <param name="propertyType">Тип свойства</param>
        /// <returns>Результат проверки 
        /// в зависимости от типа свойства объекта CIM</returns>
        private ValidationResult GetValidationResults(
            IEnumerable<CimMetaPropertyKind> nonValidProperties)
        {
            switch(nonValidProperties.FirstOrDefault())
            {
                case CimMetaPropertyKind.Attribute:
                    return new ValidationResult()
                    {
                        Message = $"В схеме отсутствуют " +
                        $"атрибуты: " +
                        $"\"{string.Join(',', nonValidProperties)}\"",
                        ResultType = ValidationResultType.fail,
                        ModelObject = _modelObject
                    };
                case CimMetaPropertyKind.Assoc1ToM:
                case CimMetaPropertyKind.Assoc1To1:
                    return new ValidationResult()
                    {
                        Message = $"В схеме отсутствуют " +
                        $"ассоциации: " +
                        $"\"{string.Join(',', nonValidProperties)}\"",
                        ResultType = ValidationResultType.fail,
                        ModelObject = _modelObject
                    };
            }
            return new ValidationResult()
            {
                ResultType = ValidationResultType.pass,
            };
        }
    }
}
