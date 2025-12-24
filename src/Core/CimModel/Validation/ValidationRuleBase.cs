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

using CimBios.Core.CimModel.DatatypeLib.ModelObject;
using CimBios.Core.CimModel.Schema;

namespace CimBios.Core.CimModel.Validation;

public abstract class ValidationRuleBase : IValidationRule
{
    /// <summary>
    ///     Конструктор класса ValidationRuleBase
    /// </summary>
    protected ValidationRuleBase()
    {
    }

    /// <inheritdoc />
    public abstract IEnumerable<IValidationResult> Execute(
        IReadOnlyModelObject modelObject);

    /// <inheritdoc />
    public abstract bool NeedExecute(IReadOnlyModelObject modelObject);
}

///
internal static class GetGenericPropExtension
{
    /// <summary>
    ///     Get any value (attribute or assoc) of model object by meta property.
    /// </summary>
    /// <param name="modelObject">Model object instance.</param>
    /// <param name="property">Meta property.</param>
    /// <returns>Object value or null if property value does not exist.</returns>
    internal static object? GetPropertyValueAsObject(
        this IReadOnlyModelObject modelObject, ICimMetaProperty property)
    {
        return property.PropertyKind switch
        {
            CimMetaPropertyKind.Attribute => modelObject.GetAttribute(property),
            CimMetaPropertyKind.Assoc1To1 => modelObject.GetAssoc1To1(property),
            CimMetaPropertyKind.Assoc1ToM => modelObject.GetAssoc1ToM(property),
            _ => null
        };
    }
}