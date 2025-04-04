using CimBios.Core.CimModel.CimDatatypeLib;

namespace CimBios.Core.CimModel.Validation;

public interface IValidationRule
{
    /// <summary>
    /// Метод для получения списка результатов проверки объекта CIM
    /// </summary>
    /// <param name="modelObject">Объект CIM из фрагмента</param>
    /// <returns>Список результатов проверки объекта CIM</returns>
    IEnumerable<IValidationResult> Execute(IReadOnlyModelObject modelObject);

    ///
    bool NeedExecute(IReadOnlyModelObject modelObject);
}
