using CimBios.Core.CimModel.CimDataModel;
using CimBios.Core.CimModel.CimDatatypeLib;
using CimBios.Core.CimModel.CimDifferenceModel;
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
