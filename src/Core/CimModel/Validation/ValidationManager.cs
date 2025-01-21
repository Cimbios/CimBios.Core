using CimBios.Core.CimModel.CimDatatypeLib;
using System;
using System.Reflection;
using CimBios.Core.CimModel.Schema;

namespace CimBios.Core.CimModel.Validation
{
    public class ValidationManager
    {
        /// <summary>
        /// Список правил проверок
        /// </summary>
        private IEnumerable<SchemaValidationRuleBase>? _validationRules;

        /// <summary>
        /// Результаты проверки при чтении фрагмента
        /// </summary>
        /// <param name="modelObject">Объект CIM из фрагмента</param>
        /// <returns>Список результатов проверки</returns>
        /// <exception cref="ArgumentNullException">ArgumentNullException</exception>
        public IEnumerable<SchemaValidationRuleBase>? Validate(ICimSchema schema)
        {
            var typeValidationAttributes = new ValidationManager(
                ).LoadValidAssembly();

            _validationRules = typeValidationAttributes.Select(
                vr =>
                {
                    var validInstance = Activator.CreateInstance(
                        vr, schema);

                    if (validInstance == null)
                        throw new ArgumentNullException();

                    return ((SchemaValidationRuleBase)validInstance);
                }
            );

            return _validationRules;
        }

        private IEnumerable<Type> LoadValidAssembly()
        {
            var assembly = Assembly.GetExecutingAssembly();

            var validationTypes = assembly.GetTypes()
                .Where(t => t.IsDefined(typeof(AttributeValidation), true));

            return validationTypes;
        }
    }
}
