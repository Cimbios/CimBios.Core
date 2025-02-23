using CimBios.Core.CimModel.CimDataModel;
using CimBios.Core.CimModel.CimDatatypeLib;
using CimBios.Core.CimModel.CimDatatypeLib.Headers552;
using CimBios.Core.CimModel.RdfSerializer;
using CimBios.Core.CimModel.Schema;

namespace CimBios.Core.CimModel.CimDifferenceModel;

public class CimDifferenceModel : CimDocumentBase, ICimDifferenceModel
{
    public IReadOnlyCollection<IDifferenceObject> Differences
         => _DifferencesCache.Values;

    public CimDifferenceModel(RdfSerializerBase rdfSerializer)
        : base (rdfSerializer)
    {
        _serializer.Settings.UnknownClassesAllowed = true;
        _serializer.Settings.UnknownPropertiesAllowed = true;

        InitInternalDifferenceModel();
    }

    public CimDifferenceModel(RdfSerializerBase rdfSerializer, 
        ICimDataModel cimDataModel)
        : this(rdfSerializer)
    {
        ExtractFromDataModel(cimDataModel);
    }

    public void ExtractFromDataModel(ICimDataModel cimDataModel)
    {
        var changes = cimDataModel.Changes;

        foreach (var changeStatement in changes)
        {
            if (_DifferencesCache.TryGetValue(changeStatement.ModelObject.OID, 
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

                    _DifferencesCache.Remove(delDiff.OID);
                }
                else
                {
                    throw new NotSupportedException(
                        "Unexpected change before adding difference!");
                }
                
                _DifferencesCache.Add(diff.OID, diff);
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
                        _DifferencesCache.Remove(diff.OID);
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
                    _DifferencesCache.Remove(diff.OID);
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
                        _DifferencesCache.Remove(tmpDiff.OID);
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
                _DifferencesCache.TryAdd(diff.OID, diff);
            }
        } 
    }

    public void InvalidateDataWithModel(ICimDataModel cimDataModel)
    {
        throw new NotImplementedException();
    }

    public void ResetAll()
    {
        InitInternalDifferenceModel();
    }

    private void InitInternalDifferenceModel()
    {
        //_differenceCache.Clear();

        var diffModelInstance =
        _serializer.TypeLib.CreateInstance<DifferenceModel>(
            Guid.NewGuid().ToString(),
            isAuto: false
        );

        if (diffModelInstance == null)
        {
            throw new NotSupportedException
            ("dm:DifferenceModel instance initialization failed!");
        }

        _internalDifferenceModel = diffModelInstance;
    }

    protected override void PushDeserializedObjects(
        IEnumerable<IModelObject> cache)
    {
        _internalDifferenceModel = cache
            .OfType<DifferenceModel>()
            .Single();
    }

    public override IEnumerable<IModelObject> GetAllObjects()
    {
        throw new NotImplementedException();
    }

    public override IEnumerable<T> GetObjects<T>()
    {
        throw new NotImplementedException();
    }

    public override IEnumerable<IModelObject> GetObjects(ICimMetaClass metaClass)
    {
        throw new NotImplementedException();
    }

    public override IModelObject? GetObject(string oid)
    {
        throw new NotImplementedException();
    }

    public override T? GetObject<T>(string oid) where T : default
    {
        throw new NotImplementedException();
    }

    public override bool RemoveObject(string oid)
    {
        throw new NotImplementedException();
    }

    public override bool RemoveObject(IModelObject modelObject)
    {
        throw new NotImplementedException();
    }

    public override void RemoveObjects(IEnumerable<IModelObject> modelObjects)
    {
        throw new NotImplementedException();
    }

    public override IModelObject CreateObject(string oid, ICimMetaClass metaClass)
    {
        throw new NotImplementedException();
    }

    public override T CreateObject<T>(string oid)
    {
        throw new NotImplementedException();
    }

    private DifferenceModel _InternalDifferenceModel
    {
        get
        {
            if (_internalDifferenceModel == null)
            {
                throw new NotSupportedException
                ("Internal difference model has not been initialized!");
            }

            return _internalDifferenceModel;
        }
    }

    private DifferenceModel? _internalDifferenceModel = null;

    private Dictionary<string, IDifferenceObject> _DifferencesCache = [];
}
