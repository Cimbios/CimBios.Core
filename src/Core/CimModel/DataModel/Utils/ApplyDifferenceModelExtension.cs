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
            else if (diff is UpdatingDifferenceObject updatingDifferenceObject
                && getObject != null)
            {
                var intersectedModifiedProps = getObject.MetaClass
                    .AllProperties.Intersect(diff.ModifiedProperties).ToList();

                getObject.CopyPropertiesFrom(diff.ModifiedObject,
                    intersectedModifiedProps, true);
                    
                ResolveReferencesInModelObject(model, getObject);
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
}
