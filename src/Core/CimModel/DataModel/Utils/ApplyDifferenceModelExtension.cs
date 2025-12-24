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

using CimBios.Core.CimModel.DatatypeLib.DifferenceObject;
using CimBios.Core.CimModel.DatatypeLib.ModelObject;
using CimBios.Core.CimModel.Schema;

namespace CimBios.Core.CimModel.DataModel.Utils;

/// <summary>
///     Apply diffs to ICimModelObject helper.
/// </summary>
public static class ApplyDifferenceModelExtension
{
    /// <summary>
    /// Apply difference model to ICimDataModel instance method.
    /// Implementing add, remove and update operations via Importer and references resolver.
    /// </summary>
    /// <param name="model">Target ICimDataModel instance.</param>
    /// <param name="differenceModel">ICimDifference model instance to apply.</param>
    public static void ApplyDifferenceModel(this ICimDataModel model,
        ICimDifferenceModel differenceModel)
    {
        foreach (var diff in differenceModel.Differences)
        {
            if (diff is AdditionDifferenceObject addDiff)
                model.ImportModelObject(addDiff.ModifiedObject);
            else if (diff is DeletionDifferenceObject)
                model.RemoveObject(diff.OID);
            else if (diff is UpdatingDifferenceObject updatingDifferenceObject)
                ApplyUpdating(model, updatingDifferenceObject);
        }
    }

    private static void ApplyUpdating(ICimDataModel model,
        UpdatingDifferenceObject diff)
    {
        var getObject = model.GetObject(diff.OID);

        if (getObject == null) return;

        var intersectedModifiedProps = getObject.MetaClass
            .AllProperties.Intersect(diff.ModifiedProperties).ToList();

        getObject.CopyPropertiesFrom(diff.ModifiedObject,
            intersectedModifiedProps, true);

        model.ResolveReferencesInModelObject(getObject);
        
        // reverse assocs M removing
        if (diff.OriginalObject != null)
            foreach (var metaProperty in intersectedModifiedProps
                         .Where(p => p.PropertyKind == CimMetaPropertyKind.Assoc1ToM))
            {
                var assocsToRemove = diff.OriginalObject
                    .GetAssoc1ToM(metaProperty);

                var currentAssocs = getObject.GetAssoc1ToM(metaProperty);
                var handledAssocs = assocsToRemove.Join(currentAssocs, a1 => a1.OID,
                    a2 => a2.OID, (a1, a2) => a2);

                foreach (var assoc in handledAssocs) getObject.RemoveAssoc1ToM(metaProperty, assoc);
            }
    }
}
