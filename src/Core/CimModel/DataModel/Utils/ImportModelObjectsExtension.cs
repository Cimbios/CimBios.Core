using CimBios.Core.CimModel.CimDataModel;
using CimBios.Core.CimModel.CimDatatypeLib;
using CimBios.Core.CimModel.Schema;

namespace CimBios.Core.CimModel.DataModel.Utils;

/// <summary>
/// Safe model objects importer extension methods.
/// </summary>
public static class ImportModelObjectsExtension
{
    /// <summary>
    /// Import model object to ICimDataModel instance with properties replacing.
    /// If objects with same OIDs have different meta class - class changing will be produce.
    /// </summary>
    /// <param name="dataModel">Target ICimDataModel instance.</param>
    /// <param name="modelObject">An object to import.</param>
    public static void ImportModelObject(this ICimDataModel dataModel, 
        IReadOnlyModelObject modelObject)
    {
        var getObject = dataModel.GetObject(modelObject.OID);

        var schemaMetaClass = dataModel.Schema
            .TryGetResource<ICimMetaClass>(modelObject.MetaClass.BaseUri);

        if (schemaMetaClass == null) return;

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
                dataModel.RemoveObject(getObject);
                targetObject = dataModel.CreateObject(modelObject.OID,
                    schemaMetaClass);
            }
        }
        else
        {
            targetObject = dataModel.CreateObject(modelObject.OID, schemaMetaClass);
        }
        
        var intersectedModifiedProps = targetObject.MetaClass
            .AllProperties.Intersect(modelObject.MetaClass.AllProperties).ToList();

        targetObject.CopyPropertiesFrom(modelObject,
            intersectedModifiedProps, true);

        dataModel.ResolveReferencesInModelObject(targetObject);
    }
    
    /// <summary>
    /// Import model object to ICimDataModel instance with properties replacing.
    /// If objects with same OIDs have different meta class - class changing will be produce.
    /// </summary>
    /// <param name="dataModel">Target ICimDataModel instance.</param>
    /// <param name="modelObjects">Objects to import.</param>
    public static void ImportModelObjects(this ICimDataModel dataModel, 
        IEnumerable<IReadOnlyModelObject> modelObjects)
    {
        foreach (var modelObject in modelObjects) 
            dataModel.ImportModelObject(modelObject);
    }
    
    /// <summary>
    /// Import model object to ICimDataModel instance with properties replacing.
    /// If objects with same OIDs have different meta class - class changing will be produce.
    /// </summary>
    /// <param name="dataModel">Target ICimDataModel instance.</param>
    /// <param name="dataModel2">Model objects ICimDataModel container to import.</param>
    public static void ImportModelObjects(this ICimDataModel dataModel, 
        ICimDataModel dataModel2)
    {
        dataModel.ImportModelObjects(dataModel2.GetAllObjects());
    }
    
    /// <summary>
    /// Resolve references of unresolved model objects.
    /// </summary>
    /// <param name="model">Target ICimDataModel instance.</param>
    /// <param name="modelObject">An object to resolve.</param>
    internal static void ResolveReferencesInModelObject(this ICimDataModel model,
        IModelObject modelObject)
    {
        foreach (var metaProperty in modelObject.MetaClass.AllProperties)
        {
            var refs = new List<ModelObjectUnresolvedReference>();
            if (metaProperty.PropertyKind == CimMetaPropertyKind.Assoc1To1)
            {
                if (modelObject.GetAssoc1To1<IModelObject>(metaProperty) 
                    is ModelObjectUnresolvedReference assocObj) 
                    refs.Add(assocObj);
            }
            else if (metaProperty.PropertyKind == CimMetaPropertyKind.Assoc1ToM)
            {
                var assocObjs = modelObject
                    .GetAssoc1ToM<IModelObject>(metaProperty)
                    .OfType<ModelObjectUnresolvedReference>();

                refs.AddRange(assocObjs);
            }
            else
            {
                continue;
            }

            foreach (var refObj in refs)
            {
                var referenceObject = model.GetObject(refObj.OID);
                if (referenceObject == null) continue;

                refObj.WaitingObjects.Add(modelObject);
                refObj.ResolveWith(referenceObject);
            }
        }
    }
}
