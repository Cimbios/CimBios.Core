using CimBios.Core.CimModel.CimDatatypeLib;
using CimBios.Core.CimModel.Schema;

namespace CimBios.Core.CimModel.Validation;

public interface IValidationResult
{
    /// <summary>
    /// Тип результата валидации
    /// </summary>
    public ValidationResultKind ResultType { get; }

    /// <summary>
    /// Сообщение после проверки
    /// </summary>
    public string Message { get; }       
}

/// <summary>
/// Just pass-mock validation result class.
/// </summary>
public class PassValidationResult : IValidationResult
{
    public PassValidationResult()
    {}

    public PassValidationResult(string message)
    {
        Message = message;
    }

    public ValidationResultKind ResultType => ValidationResultKind.Pass;
    
    public string Message { get; } = string.Empty;
}

public class ModelValidationResult (ValidationResultKind resultType, 
        string message)
    : IValidationResult
{
    /// <summary>
    /// Тип результата валидации
    /// </summary>
    public ValidationResultKind ResultType { get; } = resultType;

    /// <summary>
    /// Сообщение после проверки
    /// </summary>
    public string Message { get; } = message;
}

public class ModelObjectValidationResult(ValidationResultKind resultType,
        string message, IReadOnlyModelObject modelObject, 
        ICimMetaProperty? metaProperty = null) 
    : ModelValidationResult(resultType, message) 
{
    /// <summary>
    /// Объект CIM
    /// </summary>
    public IReadOnlyModelObject? ModelObject
    {
        get
        {
            if (_ModelObjectRef.TryGetTarget(out var mo))
            {
                 return mo;
            }

            return null;
        }
    }

    /// <summary>
    /// Свойство объекта
    /// </summary>
    public ICimMetaProperty? Property { get; } = metaProperty;

    protected WeakReference<IReadOnlyModelObject> _ModelObjectRef 
        = new(modelObject);
}

public enum ValidationResultKind
{
    Pass,
    Fail,
    Warning
}
