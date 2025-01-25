using CimBios.Core.CimModel.CimDatatypeLib;
using System;
using System.Reflection;
using CimBios.Core.CimModel.Schema;

namespace CimBios.Core.CimModel.Validation
{
    public class ValidationManager
    {
        public IEnumerable<SchemaValidationRuleBase> ValidationRules 
        {
            get 
            {
                if (_validationRules == null)
                {
                    throw new ArgumentNullException();
                }
                return _validationRules;
            }
            private set 
            {
                _validationRules = value;
            }
        }

        /// <summary>
        /// Создание экземпляров проверок
        /// </summary>
        /// <param name="schema">Каноническая схема CIM</param>
        /// <returns>Список проверок</returns>
        /// <exception cref="ArgumentNullException">ArgumentNullException</exception>
        public IEnumerable<IEnumerable<ValidationResult>> Validate(IModelObject modelObject)
        {
            LoadAssembly();

            foreach (var rule in ValidationRules)
            {
                yield return rule.Execute(modelObject);
            }
        }

        /// <summary>
        /// Загрузка типов валидации из сборки
        /// </summary>
        /// <returns>Типы валидации</returns>
        private void LoadAssembly()
        {
            if (_validationRules != null) return;

            var assembly = Assembly.GetExecutingAssembly();

            var validationTypes = assembly.GetTypes()
                .Where(t => t.IsDefined(typeof(AttributeValidation), true));

            _validationRules = validationTypes.Select(
                vr =>
                {
                    var validInstance = Activator.CreateInstance(
                        vr);

                    if (validInstance == null)
                        throw new ArgumentNullException();

                    return ((SchemaValidationRuleBase)validInstance);
                }
            );
        }

        /// <summary>
        /// Список правил проверок
        /// </summary>
        private IEnumerable<SchemaValidationRuleBase>? _validationRules;
    }
}