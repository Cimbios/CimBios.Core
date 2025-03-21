using CimBios.Core.CimModel.CimDataModel;
using CimBios.Core.CimModel.CimDatatypeLib;
using CimBios.Core.CimModel.Schema;

namespace CimBios.Core.CimModel.DataModel.Utils;

internal static class DiffsToModelHelper
{
    internal static void ApplyTo(ICimDataModel model, 
        IEnumerable<IDifferenceObject> differences)
    {
        differences.AsParallel().ForAll(diff =>
        {
            if (diff is AdditionDifferenceObject)
            {
                var schemaMetaClass = model.Schema
                    .TryGetResource<ICimMetaClass>(diff.MetaClass.BaseUri);
                
                if (schemaMetaClass == null)
                {
                    return;
                }
                
                var addedObject = model.CreateObject(diff.OID, schemaMetaClass);

                addedObject.CopyPropertiesFrom(diff.ModifiedObject, true);
                ResolveReferencesInModelObject(model, addedObject);

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
}
