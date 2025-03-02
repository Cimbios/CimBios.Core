using CimBios.Core.CimModel.CimDataModel;
using CimBios.Core.CimModel.CimDataModel.Utils;
using CimBios.Core.CimModel.CimDatatypeLib;

namespace CimBios.Core.CimModel.DataModel.Utils;

internal static class DiffsFromModelHelper
{
    internal static Dictionary<string, IDifferenceObject> ExtractFrom(
        ICimDataModel model)
    {
        Dictionary<string, IDifferenceObject> differences = [];

        var changes = model.Changes;

        foreach (var changeStatement in changes)
        {
            if (differences.TryGetValue(changeStatement.ModelObject.OID, 
                out IDifferenceObject? diff) == false)
            {
                diff = null;
            }

            if (changeStatement is CimDataModelObjectAddedStatement added)
            {
                if (diff is null)
                {
                    diff = new AdditionDifferenceObject(added.ModelObject.OID,
                        added.ModelObject.MetaClass);
                }
                else if (diff is DeletionDifferenceObject delDiff)
                {
                    // TODO: check class changing
                    diff = new UpdatingDifferenceObject(
                        added.ModelObject.OID); 

                    differences.Remove(delDiff.OID);
                }
                else
                {
                    throw new NotSupportedException(
                        "Unexpected change before adding difference!");
                }
                
                differences.Add(diff.OID, diff);
            }
            else if (changeStatement is 
                CimDataModelObjectUpdatedStatement updated)
            {
                diff ??= new UpdatingDifferenceObject(updated.ModelObject.OID);

                if (diff is UpdatingDifferenceObject 
                    || diff is AdditionDifferenceObject)
                {
                    diff.ChangePropertyValue(updated.MetaProperty,
                        updated.OldValue, updated.NewValue);

                    if (diff.ModifiedProperties.Count == 0)
                    {
                        differences.Remove(diff.OID);
                        diff = null;
                    }
                }
                else
                {
                    throw new NotSupportedException(
                        "Unexpected change before updating difference!");
                }
            }
            else if (changeStatement is 
                CimDataModelObjectRemovedStatement removed)
            {
                if (diff is AdditionDifferenceObject)
                {
                    differences.Remove(diff.OID);
                    diff = null;
                    continue;
                }

                 if (diff is DeletionDifferenceObject)
                 {
                    throw new NotSupportedException(
                        "Unexpected change before removing difference!");
                 }
                else
                {
                    var tmpDiff = diff;
                    diff = new DeletionDifferenceObject(removed.ModelObject.OID,
                        removed.ModelObject.MetaClass);
                        
                    foreach (var prop in removed.ModelObject
                        .MetaClass.AllProperties)
                    {
                        var propValue = removed.ModelObject
                            .TryGetPropertyValue(prop);
                        
                        diff.ChangePropertyValue(prop, null, propValue);
                    }

                    if (tmpDiff is UpdatingDifferenceObject upd)
                    {
                        differences.Remove(tmpDiff.OID);
                        foreach (var prop in upd.ModifiedProperties)
                        {
                            var propValue = upd.OriginalObject?.
                                TryGetPropertyValue(prop);

                            diff.ChangePropertyValue(prop, null, propValue);
                        }
                    }
                }
            }

            if (diff != null)
            {
                differences.TryAdd(diff.OID, diff);
            }
        } 

        return differences;
    }
}
