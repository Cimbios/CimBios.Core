using CimBios.Core.CimModel.DatatypeLib.ModelObject;
using CimBios.Core.CimModel.Schema;
/*
 *    CimBios.Core - Common Information Model (IEC61970) I/O Library
 *    Copyright (C) 2025 Yuri A. Kovalenko a.k.a belizahrt <belizahrt@gmail.com>
 *
 *    This program is free software: you can redistribute it and/or modify
 *    it under the terms of the GNU General Public License as published by
 *    the Free Software Foundation, either version 3 of the License, or
 *    (at your option) any later version.
 *
 *    This program is distributed in the hope that it will be useful,
 *    but WITHOUT ANY WARRANTY; without even the implied warranty of
 *    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *    GNU General Public License for more details.
 *
 *    You should have received a copy of the GNU General Public License
 *    along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

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
