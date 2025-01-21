using CimBios.Core.CimModel.CimDatatypeLib;
using System;
using System.Reflection;

namespace CimBios.Core.CimModel.Validation
{
    public class ValidationManager
    {

        /// <summary>
        /// Результаты проверки при чтении фрагмента
        /// </summary>
        /// <param name="modelObject">Объект CIM из фрагмента</param>
        /// <returns>Список результатов проверки</returns>
        /// <exception cref="ArgumentNullException">ArgumentNullException</exception>
        public List<(string? Uuid, ValidationResultType ResultType, string? Message)> Validate(
            IModelObject modelObject)
        {
            var validationResults = new List<Validation.ValidationResult>();

            var typeValidationAttributes = new ValidationManager(
                ).LoadValidAssembly();

            try
            {
                validationResults = typeValidationAttributes.Select(
                    va =>
                    {
                        var validInstance = Activator.CreateInstance(
                            va, modelObject, Schema);

                        if (validInstance == null)
                            throw new ArgumentNullException();

                        return ((SchemaValidationRuleBase)validInstance).Execute();
                    }
                ).First().
                  ToList();
            }
            catch (Exception ex)
            {
                return new List<(
                    string? Uuid,
                    ValidationResultType ResultType,
                    string? Message)>()
            {
                (modelObject.Uuid,
                ValidationResultType.fail,
                ex.Message)
            };
            }

            var validMessage = new List<(
                string? Uuid,
                ValidationResultType ResultType,
                string? Message)>();

            foreach (var vr in validationResults)
            {
                validMessage.Add(
                    (vr.ModelObject?.Uuid, vr.ResultType, vr.Message)
                    );
            }

            return validMessage;
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
