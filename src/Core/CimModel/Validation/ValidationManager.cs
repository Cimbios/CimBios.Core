using CimBios.Core.CimModel.CimDatatypeLib;
using System;
using System.Reflection;
using CimBios.Core.CimModel.Schema;
using CimBios.Core.CimModel.ObjectModel;

namespace CimBios.Core.CimModel.Validation
{
    public class ValidationManager
    {
        /// <summary>
        /// Список правил проверок
        /// </summary>
        private IEnumerable<IValidationRule> _validationRules = [];

        /// <summary>
        /// Конструктор класса ValidationManager
        /// </summary>
        public ValidationManager()
        {
            LoadAssembly();
        }

        /// <summary>
        /// Проверяет контекст модели
        /// </summary>
        /// <param name="objectModel">Контекст модели</param>
        /// <returns>Массив результатов проверки</returns>
        public ValidationResult[] Validate(IObjectModel objectModel)
        {
            List<ValidationResult> listRule = new List<ValidationResult>();

            var objects = objectModel.GetAllObjects();

            foreach (var rule in _validationRules)
            {
                foreach (var obj in objects)
                {
                    rule.Execute(obj);
                }
            }

            return listRule.ToArray();
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

                    return ((ValidationRuleBase)validInstance);
                }
            );
        }
    }
}