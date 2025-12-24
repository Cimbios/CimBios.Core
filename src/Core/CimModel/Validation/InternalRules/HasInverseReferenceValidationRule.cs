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
///     Validation rule for property inverse accordance
///     - opposite object has reference to source by property.
/// </summary>
public class HasInverseReferenceValidationRule : ValidationRuleBase
{
    /// <inheritdoc />
    public override IEnumerable<IValidationResult> Execute(
        IReadOnlyModelObject modelObject)
    {
        return modelObject.MetaClass.AllProperties
            .Where(p => p.PropertyKind is 
                CimMetaPropertyKind.Assoc1To1 or CimMetaPropertyKind.Assoc1ToM)
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
        if (property.InverseProperty is null)
        {
            return new ModelObjectValidationResult(
                ValidationResultKind.Fail,
                "Model object association does not have inverse property in schema: " +
                property.ShortName,
                modelObject, property);
        }
        
        var checkReferences = new List<IModelObject>();
        switch (property.PropertyKind)
        {
            case CimMetaPropertyKind.Assoc1To1:
            {
                if (modelObject.GetAssoc1To1(property) is ModelObject reference1) 
                    checkReferences.Add(reference1);
                break;
            }
            case CimMetaPropertyKind.Assoc1ToM:
                checkReferences.AddRange(modelObject
                    .GetAssoc1ToM(property)
                    .OfType<ModelObject>());
                break;
            default:
                throw new NotSupportedException("Not association property received!");
        }

        foreach (var reference in checkReferences)
        {
            switch (property.InverseProperty.PropertyKind)
            {
                case CimMetaPropertyKind.Assoc1To1:
                {
                    if (modelObject != reference.GetAssoc1To1(property.InverseProperty))
                    {
                        return new ModelObjectValidationResult(
                            ValidationResultKind.Fail,
                            $"There is no two-side link with object {reference.OID} by properties:" +
                            property.ShortName + " - " + property.InverseProperty.ShortName,
                            modelObject, property);
                    }

                    break;
                }
                case CimMetaPropertyKind.Assoc1ToM:
                {
                    if (!reference.GetAssoc1ToM(property.InverseProperty).Contains(modelObject))
                    {
                        return new ModelObjectValidationResult(
                            ValidationResultKind.Fail,
                            $"There is no two-side link with object {reference.OID} by properties:" +
                            property.ShortName + " - " + property.InverseProperty.ShortName,
                            modelObject, property);
                    }
                    
                    break;
                }
                default:
                    throw new NotSupportedException(
                        "Inverse property is not association!");
            }
        }

        return new PassValidationResult();
    }
}