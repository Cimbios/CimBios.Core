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

namespace CimBios.Core.CimModel.Validation.InternalRules;

/// <summary>
///     Validation rule for property multiplicity accordance.
/// </summary>
public class PropertyMultiplicityValidationRule : ValidationRuleBase
{
    /// <inheritdoc />
    public override IEnumerable<IValidationResult> Execute(
        IReadOnlyModelObject modelObject)
    {
        return modelObject.MetaClass.AllProperties
            .Where(p => p.IsValueRequired)
            .Select(p => GetValidationResult(modelObject, p));
    }

    public override bool NeedExecute(IReadOnlyModelObject modelObject)
    {
        return modelObject is ModelObject;
    }

    /// <summary>
    ///     Get validation result.
    /// </summary>
    /// <param name="modelObject">Model object instance.</param>
    /// <param name="property">Meta property.</param>
    /// <returns>Validation result</returns>
    private static IValidationResult GetValidationResult(
        IReadOnlyModelObject modelObject, ICimMetaProperty property)
    {
        var value = modelObject.GetPropertyValueAsObject(property);

        if (value is ICollection<object> collection
                ? collection.Count == 0
                : value == null)
            return new ModelObjectValidationResult(
                ValidationResultKind.Fail,
                "Model object does not contain required value " +
                $"for \"{property}\" property.",
                modelObject, property);

        return new PassValidationResult();
    }
}