using System.Collections.Concurrent;
using System.Text;
using CimBios.Core.CimModel.CimDataModel;
using CimBios.Core.CimModel.CimDataModel.Utils;
using CimBios.Core.CimModel.CimDatatypeLib;
using CimBios.Core.CimModel.CimDatatypeLib.EventUtils;
using CimBios.Core.CimModel.CimDatatypeLib.Headers552;
using CimBios.Core.CimModel.CimDatatypeLib.OID;
using CimBios.Core.CimModel.RdfSerializer;
using CimBios.Core.CimModel.Schema;

namespace CimBios.Core.CimModel.CimDifferenceModel;

public class CimDifferenceModel : CimDocumentBase, ICimDifferenceModel
{
    protected ConcurrentDictionary<IOIDDescriptor, IDifferenceObject>
        _DifferencesCache = [];

    protected DifferenceModel? _internalDifferenceModel;

    protected ICimDataModel? _subscribedDataModel;

    public CimDifferenceModel(ICimSchema cimSchema, ICimDatatypeLib typeLib,
        IOIDDescriptorFactory oidDescriptorFactory)
        : base(cimSchema, typeLib, oidDescriptorFactory)
    {
        ResetAll();
    }

    public CimDifferenceModel(ICimSchema cimSchema, ICimDatatypeLib typeLib,
        ICimDataModel cimDataModel)
        : this(cimSchema, typeLib, cimDataModel.OIDDescriptorFactory)
    {
        SubscribeToDataModelChanges(cimDataModel);
    }

    private DifferenceModel _InternalDifferenceModel
    {
        get
        {
            if (_internalDifferenceModel == null)
                throw new NotSupportedException
                    ("Internal difference model has not been initialized!");

            return _internalDifferenceModel;
        }
    }

    public override Model? ModelDescription => _internalDifferenceModel;

    public IReadOnlyCollection<IDifferenceObject> Differences
        => _DifferencesCache.Values.ToHashSet();

    public void CompareDataModels(ICimDataModel originDataModel,
        ICimDataModel modifiedDataModel)
    {
        ResetAll();

        foreach (var forwardObject in modifiedDataModel.GetAllObjects())
        {
            var originObject = originDataModel.GetObject(forwardObject.OID);
            if (originObject == null)
            {
                var addingObject = new AdditionDifferenceObject(forwardObject);
                _DifferencesCache.TryAdd(addingObject.OID, addingObject);
            }
            else
            {
                var existingDiff = ModelObjectsComparer
                    .Compare(originObject, forwardObject);

                if (existingDiff.ModifiedProperties.Count != 0)
                    _DifferencesCache.TryAdd(existingDiff.OID, existingDiff);
            }
        }

        foreach (var reverseObject in originDataModel.GetAllObjects())
        {
            if (modifiedDataModel.GetObject(reverseObject.OID) != null) continue;

            var deletionObject = new DeletionDifferenceObject(reverseObject);
            _DifferencesCache.TryAdd(deletionObject.OID, deletionObject);
        }
    }

    public void ResetAll()
    {
        _Objects.Clear();

        _internalDifferenceModel = TypeLib.CreateInstance<DifferenceModel>(
            new UuidDescriptor());

        if (_internalDifferenceModel == null)
            throw new NotSupportedException
                ("dm:DifferenceModel instance initialization failed!");

        _internalDifferenceModel.created = DateTime.Now.ToUniversalTime();

        _Objects.Add(_internalDifferenceModel.OID, _internalDifferenceModel);
        _DifferencesCache.Clear();
    }

    public void SubscribeToDataModelChanges(ICimDataModel cimDataModel)
    {
        _DifferencesCache.Clear();

        _subscribedDataModel = cimDataModel;

        cimDataModel.ModelObjectPropertyChanged
            += OnSubscribeModelObjectPropertyChanged;

        cimDataModel.ModelObjectStorageChanged
            += OnSubscribeModelObjectStorageChanged;
    }

    public void UnsubscribeFromDataModelChanges()
    {
        if (_subscribedDataModel == null) return;

        _subscribedDataModel.ModelObjectPropertyChanged
            -= OnSubscribeModelObjectPropertyChanged;

        _subscribedDataModel.ModelObjectStorageChanged
            -= OnSubscribeModelObjectStorageChanged;
    }

    private void ToDifferenceModel()
    {
        _InternalDifferenceModel.forwardDifferences.Clear();
        _InternalDifferenceModel.reverseDifferences.Clear();

        _DifferencesCache.Values.AsParallel().ForAll(diff =>
            {
                switch (diff)
                {
                    case AdditionDifferenceObject:
                    case UpdatingDifferenceObject:
                        _InternalDifferenceModel.forwardDifferences.Add(
                            new WeakModelObject(diff.ModifiedObject));
                        break;
                    case DeletionDifferenceObject:
                        _InternalDifferenceModel.reverseDifferences.Add(
                            new WeakModelObject(diff.ModifiedObject));
                        break;
                }

                if (diff is UpdatingDifferenceObject
                    && diff.OriginalObject is not null)
                    _InternalDifferenceModel.reverseDifferences.Add(
                        new WeakModelObject(diff.OriginalObject));
            }
        );
    }

    private void ToDifferencesCache()
    {
        _DifferencesCache.Clear();

        var descriptionMetaClass = Schema.TryGetResource<ICimMetaClass>(
            new Uri(Description.ClassUri));

        if (descriptionMetaClass == null)
            throw new NotSupportedException(
                "Schema does not contains neccessary rdf:Description class!");

        var waitingForwardUpdates = new Dictionary<IOIDDescriptor, IModelObject>();
        _InternalDifferenceModel.forwardDifferences.AsParallel().ForAll(s =>
            {
                if (s.MetaClass != descriptionMetaClass)
                {
                    var addDiff = new AdditionDifferenceObject(s);

                    _DifferencesCache.TryAdd(addDiff.OID, addDiff);
                }
                else
                {
                    waitingForwardUpdates.TryAdd(s.OID, s);
                }
            }
        );

        _InternalDifferenceModel.reverseDifferences.AsParallel().ForAll(s =>
            {
                if (s.MetaClass != descriptionMetaClass)
                {
                    var delDiff = new DeletionDifferenceObject(s);

                    _DifferencesCache.TryAdd(delDiff.OID, delDiff);
                }
                else
                {
                    if (waitingForwardUpdates.Remove(s.OID,
                            out var waiting) == false)
                        waiting = new WeakModelObject(s.OID,
                            descriptionMetaClass);

                    var updDiff = new UpdatingDifferenceObject(s, waiting);

                    _DifferencesCache.TryAdd(updDiff.OID, updDiff);
                }
            }
        );

        waitingForwardUpdates.Values.AsParallel().ForAll(w =>
            {
                var nullObj = new WeakModelObject(w.OID,
                    descriptionMetaClass);

                var updDiff = new UpdatingDifferenceObject(nullObj, w);

                _DifferencesCache.TryAdd(updDiff.OID, updDiff);
            }
        );
    }

    private void OnSubscribeModelObjectStorageChanged(ICimDataModel? sender,
        IModelObject modelObject, CimDataModelObjectStorageChangedEventArgs e)
    {
        var diff = _DifferencesCache.GetValueOrDefault(modelObject.OID);

        switch (e.ChangeType)
        {
            case CimDataModelObjectStorageChangeType.Add:
            {
                if (diff is null)
                {
                    diff = new AdditionDifferenceObject(
                        modelObject.OID, modelObject.MetaClass);
                }
                else if (diff is DeletionDifferenceObject delDiff)
                {
                    if (!modelObject.MetaClass.Equals(delDiff.MetaClass))
                        diff = new AdditionDifferenceObject(
                            modelObject.OID, modelObject.MetaClass);

                    _DifferencesCache.Remove(delDiff.OID, out _);
                }
                else
                {
                    throw new NotSupportedException(
                        "Unexpected change before adding difference!");
                }

                break;
            }
            case CimDataModelObjectStorageChangeType.Remove:
            {
                if (diff is AdditionDifferenceObject)
                {
                    _DifferencesCache.Remove(diff.OID, out _);
                    diff = null;
                    return;
                }

                if (diff is DeletionDifferenceObject)
                    throw new NotSupportedException(
                        "Unexpected change before removing difference!");

                var tmpDiff = diff;
                diff = new DeletionDifferenceObject(modelObject.OID,
                    modelObject.MetaClass);

                foreach (var prop in modelObject.MetaClass.AllProperties)
                {
                    var propValue = modelObject.TryGetPropertyValue(prop);

                    diff.ChangePropertyValue(prop, null, propValue);
                }

                if (tmpDiff is UpdatingDifferenceObject upd)
                {
                    _DifferencesCache.Remove(tmpDiff.OID, out _);
                    foreach (var prop in upd.ModifiedProperties)
                    {
                        var propValue = upd.OriginalObject?.TryGetPropertyValue(prop);

                        diff.ChangePropertyValue(prop, null, propValue);
                    }
                }

                break;
            }
        }

        if (diff != null) _DifferencesCache.TryAdd(diff.OID, diff);
    }

    private void OnSubscribeModelObjectPropertyChanged(ICimDataModel? sender,
        IModelObject modelObject, CimMetaPropertyChangedEventArgs e)
    {
        object? oldData = null;
        object? newData = null;
        if (e is CimMetaAttributeChangedEventArgs attrEv)
        {
            oldData = attrEv.OldValue;
            newData = attrEv.NewValue;
        }
        else if (e is CimMetaAssocChangedEventArgs assocEv)
        {
            oldData = assocEv.OldModelObject;
            newData = assocEv.NewModelObject;
        }
        else
        {
            throw new NotSupportedException();
        }

        if (_DifferencesCache.TryGetValue(modelObject.OID,
                out var diff) == false)
            diff = null;

        diff ??= new UpdatingDifferenceObject(modelObject.OID);

        if (diff is UpdatingDifferenceObject
            || diff is AdditionDifferenceObject)
        {
            diff.ChangePropertyValue(e.MetaProperty, oldData, newData);

            if (diff.ModifiedProperties.Count == 0)
            {
                _DifferencesCache.Remove(diff.OID, out _);
                diff = null;
            }
        }
        else
        {
            throw new NotSupportedException(
                "Unexpected change before updating difference!");
        }

        if (diff != null) _DifferencesCache.TryAdd(diff.OID, diff);
    }

    #region SaveLoadLogic

    protected override void PushDeserializedObjects(
        IEnumerable<IModelObject> cache)
    {
        _internalDifferenceModel = cache
            .OfType<DifferenceModel>()
            .Single();

        _Objects.Add(_internalDifferenceModel.OID, _internalDifferenceModel);

        ToDifferencesCache();
    }

    public override void Load(StreamReader streamReader,
        IRdfSerializerFactory serializerFactory, ICimSchema cimSchema)
    {
        _internalDifferenceModel = null;
        _DifferencesCache.Clear();

        serializerFactory.Settings = RedefineDiffSerializerSettings(
            serializerFactory.Settings);

        base.Load(streamReader, serializerFactory, cimSchema);
    }

    public override void Save(StreamWriter streamWriter,
        IRdfSerializerFactory serializerFactory, ICimSchema cimSchema)
    {
        serializerFactory.Settings = RedefineDiffSerializerSettings(
            serializerFactory.Settings);

        ToDifferenceModel();

        base.Save(streamWriter, serializerFactory, cimSchema);
    }

    public override void Save(string path, IRdfSerializerFactory serializerFactory,
        ICimSchema cimSchema)
    {
        serializerFactory.Settings = RedefineDiffSerializerSettings(
            serializerFactory.Settings);

        ToDifferenceModel();

        base.Save(path, serializerFactory, cimSchema);
    }

    public override void Save(string path, IRdfSerializerFactory serializerFactory)
    {
        serializerFactory.Settings = RedefineDiffSerializerSettings(
            serializerFactory.Settings);

        ToDifferenceModel();

        base.Save(path, serializerFactory);
    }

    public override void Parse(string content,
        IRdfSerializerFactory serializerFactory,
        ICimSchema cimSchema, Encoding? encoding = null)
    {
        serializerFactory.Settings = RedefineDiffSerializerSettings(
            serializerFactory.Settings);

        base.Parse(content, serializerFactory, cimSchema, encoding);
    }

    private static RdfSerializerSettings RedefineDiffSerializerSettings(
        RdfSerializerSettings rdfSerializerSettings)
    {
        return new RdfSerializerSettings
        {
            UnknownClassesAllowed = true,
            UnknownPropertiesAllowed = true,
            IncludeUnresolvedReferences = rdfSerializerSettings
                .IncludeUnresolvedReferences,
            WritingIRIMode = rdfSerializerSettings.WritingIRIMode
        };
    }

    #endregion SaveLoadLogic

    #region NotImpl

    public override IEnumerable<IModelObject> GetAllObjects()
    {
        throw new NotImplementedException(
            "DifferenceModel does not provides this method implementation!");
    }

    public override IEnumerable<T> GetObjects<T>()
    {
        throw new NotImplementedException(
            "DifferenceModel does not provides this method implementation!");
    }

    public override IEnumerable<IModelObject> GetObjects(ICimMetaClass metaClass)
    {
        throw new NotImplementedException(
            "DifferenceModel does not provides this method implementation!");
    }

    public override IModelObject? GetObject(IOIDDescriptor oid)
    {
        throw new NotImplementedException(
            "DifferenceModel does not provides this method implementation!");
    }

    public override T? GetObject<T>(IOIDDescriptor oid) where T : default
    {
        throw new NotImplementedException(
            "DifferenceModel does not provides this method implementation!");
    }

    public override bool RemoveObject(IOIDDescriptor oid)
    {
        throw new NotImplementedException(
            "DifferenceModel does not provides this method implementation!");
    }

    public override bool RemoveObject(IModelObject modelObject)
    {
        throw new NotImplementedException(
            "DifferenceModel does not provides this method implementation!");
    }

    public override void RemoveObjects(IEnumerable<IModelObject> modelObjects)
    {
        throw new NotImplementedException(
            "DifferenceModel does not provides this method implementation!");
    }

    public override IModelObject CreateObject(IOIDDescriptor oid, ICimMetaClass metaClass)
    {
        throw new NotImplementedException(
            "DifferenceModel does not provides this method implementation!");
    }

    public override T CreateObject<T>(IOIDDescriptor oid)
    {
        throw new NotImplementedException(
            "DifferenceModel does not provides this method implementation!");
    }

    #endregion NotImpl
}