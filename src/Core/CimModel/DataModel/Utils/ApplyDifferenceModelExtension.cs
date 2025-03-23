using CimBios.Core.CimModel.CimDataModel;
using CimBios.Core.CimModel.CimDatatypeLib;
using CimBios.Core.CimModel.CimDifferenceModel;
using CimBios.Core.CimModel.Schema;

namespace CimBios.Core.CimModel.DataModel.Utils;

/// <summary>
/// Apply diffs to ICimModelObject helper.
/// </summary>
public static class ApplyDifferenceModelExtension
{
    public static void ApplyDifferenceModel(this ICimDataModel model, 
        ICimDifferenceModel differenceModel)
    {
        var differences = differenceModel.Differences;
        differences.AsParallel().ForAll(diff =>
        {
            var getObject = model.GetObject(diff.OID);
            
            if (diff is AdditionDifferenceObject addDiff)
            {
                ApplyAddition(model, addDiff);
            }
            else if (diff is DeletionDifferenceObject)
            {
                model.RemoveObject(diff.OID);
            }
            else if (diff is UpdatingDifferenceObject updatingDifferenceObject)
            {
                ApplyUpdating(model, updatingDifferenceObject);
            }
        });
    }

    private static void ResolveReferencesInModelObject(ICimDataModel model,
        IModelObject modelObject)
    {
        foreach (var metaProperty in modelObject.MetaClass
            .AllProperties
            .Where(p => p.PropertyKind == CimMetaPropertyKind.Assoc1To1))
        {
            var assocObj = modelObject.GetAssoc1To1<IModelObject>(metaProperty);
            if (assocObj is not ModelObjectUnresolvedReference)
            {
                continue;
            }

            var referenceObject = model.GetObject(assocObj.OID);
            if (referenceObject == null)
            {
                continue;
            }

            modelObject.SetAssoc1To1(metaProperty, referenceObject);
        }
    }

    private static void ApplyAddition(ICimDataModel model, 
        AdditionDifferenceObject diff)
    {
        var getObject = model.GetObject(diff.OID);
        
        var schemaMetaClass = model.Schema
            .TryGetResource<ICimMetaClass>(diff.MetaClass.BaseUri);
        
        if (schemaMetaClass == null)
        {
            return;
        }

        IModelObject targetObject;
        if (getObject != null)
        {
            if (schemaMetaClass.Equals(getObject.MetaClass))
            {
                targetObject = getObject;
            }
            // Class changing.
            else
            {
                model.RemoveObject(getObject);
                targetObject = model.CreateObject(diff.OID, 
                    schemaMetaClass);
            }
        }
        else
        {
            targetObject = model.CreateObject(diff.OID, schemaMetaClass);
        }

        var intersectedModifiedProps = targetObject.MetaClass
            .AllProperties.Intersect(diff.ModifiedProperties).ToList();

        targetObject.CopyPropertiesFrom(diff.ModifiedObject,
            intersectedModifiedProps, true);
            
        ResolveReferencesInModelObject(model, targetObject);
    }

    private static void ApplyUpdating(ICimDataModel model, 
        UpdatingDifferenceObject diff)
    {
        var getObject = model.GetObject(diff.OID);

        if (getObject == null)
        {
            return;
        }

        var intersectedModifiedProps = getObject.MetaClass
            .AllProperties.Intersect(diff.ModifiedProperties).ToList();

        getObject.CopyPropertiesFrom(diff.ModifiedObject,
            intersectedModifiedProps, true);

        ResolveReferencesInModelObject(model, getObject);

        // reverse assocs M removing
        if (diff.OriginalObject != null)
        {
            foreach (var metaProperty in intersectedModifiedProps
                .Where(p => p.PropertyKind == CimMetaPropertyKind.Assoc1ToM))
            {
                var assocsToRemove = diff.OriginalObject
                    .GetAssoc1ToM(metaProperty);

                var currentAssocs = getObject.GetAssoc1ToM(metaProperty);
                var handledAssocs = assocsToRemove.Join(currentAssocs, a1 => a1.OID, 
                    a2 => a2.OID, (a1, a2) => a2);
                
                foreach (var assoc in handledAssocs)
                {
                    getObject.RemoveAssoc1ToM(metaProperty, assoc);
                }
            }
        }
    }
}
