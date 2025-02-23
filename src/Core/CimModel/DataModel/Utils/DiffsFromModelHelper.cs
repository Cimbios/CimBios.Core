using CimBios.Core.CimModel.CimDataModel;
using CimBios.Core.CimModel.CimDataModel.Utils;
using CimBios.Core.CimModel.CimDatatypeLib;

namespace CimBios.Core.CimModel.DataModel.Utils;

internal class DiffsFromModelHelper
{
    public Dictionary<string, IDifferenceObject> Differences => _Differences;

    public DiffsFromModelHelper(ICimDataModel model)
    {
        Extract(model);
    }

    private void Extract(ICimDataModel model)
    {
        _Differences.Clear();

        var changes = model.Changes;

        foreach (var changeStatement in changes)
        {
            if (_Differences.TryGetValue(changeStatement.ModelObject.OID, 
                out IDifferenceObject? diff) == false)
            {
                diff = null;
            }

            if (changeStatement is CimDataModelObjectAddedStatement added)
            {
                if (diff is null)
                {
                    diff = new AdditionDifferenceObject(added.ModelObject.OID);
                }
                else if (diff is DeletionDifferenceObject delDiff)
                {
                    diff = new UpdatingDifferenceObject(
                        added.ModelObject.OID); 

                    _Differences.Remove(delDiff.OID);
                }
                else
                {
                    throw new NotSupportedException(
                        "Unexpected change before adding difference!");
                }
                
                _Differences.Add(diff.OID, diff);
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
                        _Differences.Remove(diff.OID);
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
                    _Differences.Remove(diff.OID);
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
                    diff = new DeletionDifferenceObject(removed.ModelObject.OID);
                    foreach (var prop in removed.ModelObject
                        .MetaClass.AllProperties)
                    {
                        var propValue = removed.ModelObject
                            .TryGetPropertyValue(prop);
                        
                        diff.ChangePropertyValue(prop, null, propValue);
                    }

                    if (tmpDiff is UpdatingDifferenceObject upd)
                    {
                        _Differences.Remove(tmpDiff.OID);
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
                _Differences.TryAdd(diff.OID, diff);
            }
        } 
    }

    private Dictionary<string, IDifferenceObject> _Differences = [];
}
