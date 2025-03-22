using System.Collections.Concurrent;
using System.Text;
using CimBios.Core.CimModel.CimDataModel;
using CimBios.Core.CimModel.CimDataModel.Utils;
using CimBios.Core.CimModel.CimDatatypeLib;
using CimBios.Core.CimModel.CimDatatypeLib.EventUtils;
using CimBios.Core.CimModel.CimDatatypeLib.Headers552;
using CimBios.Core.CimModel.CimDatatypeLib.OID;
using CimBios.Core.CimModel.DataModel.Utils;
using CimBios.Core.CimModel.RdfSerializer;
using CimBios.Core.CimModel.Schema;

namespace CimBios.Core.CimModel.CimDifferenceModel;

public class CimDifferenceModel (ICimSchema cimSchema, ICimDatatypeLib typeLib,
    IOIDDescriptorFactory oidDescriptorFactory) 
    : CimDocumentBase (cimSchema, typeLib, oidDescriptorFactory), 
    ICimDifferenceModel
{
    public override Model? ModelDescription => _internalDifferenceModel;

    public IReadOnlyCollection<IDifferenceObject> Differences
         => _DifferencesCache.Values.ToHashSet();

    public CimDifferenceModel (ICimSchema cimSchema, ICimDatatypeLib typeLib,
        ICimDataModel cimDataModel)
        : this(cimSchema, typeLib, cimDataModel.OIDDescriptorFactory)
    {
        SubscribeToDataModelChanges(cimDataModel);
    }

    public void CompareDataModels(ICimDataModel originDataModel, 
        ICimDataModel modifiedDataModel)
    {
        
    }

    public void FitToDataModelSchema(ICimDataModel cimDataModel)
    {
        throw new NotImplementedException();
    }

    public void ResetAll()
    {
        _Objects.Clear();
        
        _internalDifferenceModel = TypeLib.CreateInstance<DifferenceModel>(
            new UuidDescriptor());

        if (_internalDifferenceModel == null)
        {
            throw new NotSupportedException
            ("dm:DifferenceModel instance initialization failed!");
        }

        _Objects.Add(_internalDifferenceModel.OID, _internalDifferenceModel);
        _DifferencesCache.Clear();
    }

    private void ToDifferenceModel()
    {
        _InternalDifferenceModel.forwardDifferences.Clear();
        _InternalDifferenceModel.reverseDifferences.Clear();

        _DifferencesCache.Values.AsParallel().ForAll(
            diff =>
            {
                if (diff is AdditionDifferenceObject
                    || diff is UpdatingDifferenceObject)
                {
                    _InternalDifferenceModel.forwardDifferences.Add(
                        new WeakModelObject(diff.ModifiedObject));
                }

                if (diff is DeletionDifferenceObject)
                {
                    _InternalDifferenceModel.reverseDifferences.Add(
                        new WeakModelObject(diff.ModifiedObject));       
                }

                if (diff is UpdatingDifferenceObject
                    && diff.OriginalObject is not null)
                {
                    _InternalDifferenceModel.reverseDifferences.Add(
                        new WeakModelObject(diff.OriginalObject)); 
                }
            }
        );
    }

    private void ToDifferencesCache()
    {
        _DifferencesCache.Clear();

        var descriptionMetaClass = Schema.TryGetResource<ICimMetaClass>(
            new(Description.ClassUri));

        if (descriptionMetaClass == null)
        {
            throw new NotSupportedException(
                "Schema does not contains neccessary rdf:Description class!");
        }

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
                    if (waitingForwardUpdates.TryGetValue(s.OID,
                        out var waiting) == false)
                    {
                        waiting = new WeakModelObject(s.OID, 
                            descriptionMetaClass);
                    }
                    else
                    {
                        waitingForwardUpdates.Remove(s.OID);
                    }

                    var updDiff = new UpdatingDifferenceObject(s, waiting);  

                    _DifferencesCache.TryAdd(updDiff.OID, updDiff);
                }
            }
        );

        waitingForwardUpdates.Values.AsParallel().ForAll(w =>
            {
                if (w == null)
                {
                    return;
                }
                
                var nullObj = new WeakModelObject(w.OID, 
                    descriptionMetaClass);

                var updDiff = new UpdatingDifferenceObject(nullObj, w);

                 _DifferencesCache.TryAdd(updDiff.OID, updDiff);
            }
        );
    }

    public void SubscribeToDataModelChanges(ICimDataModel cimDataModel)
    {
        _subscribedDataModel = cimDataModel;

        cimDataModel.ModelObjectPropertyChanged 
            += OnSubscribeModelObjectPropertyChanged;

        cimDataModel.ModelObjectStorageChanged
            += OnSubscribeModelObjectStorageChanged;
    }

    private void OnSubscribeModelObjectStorageChanged(ICimDataModel? sender, 
        IModelObject modelObject, CimDataModelObjectStorageChangedEventArgs e)
    {
        if (_DifferencesCache.TryGetValue(modelObject.OID, 
            out IDifferenceObject? diff) == false)
        {
            diff = null;
        }

        if (e.ChangeType == CimDataModelObjectStorageChangeType.Add)
        {
            if (diff is null)
            {
                diff = new AdditionDifferenceObject(
                    modelObject.OID, modelObject.MetaClass);
            }
            else if (diff is DeletionDifferenceObject delDiff)
            {
                if (!modelObject.MetaClass.Equals(delDiff.MetaClass))
                {
                    throw new Exception("Meta class changed!! Impl!!");
                }

                diff = new UpdatingDifferenceObject(
                    modelObject.OID); 

                _DifferencesCache.Remove(delDiff.OID, out var _);
            }
            else
            {
                throw new NotSupportedException(
                    "Unexpected change before adding difference!");
            }
        }
        else if (e.ChangeType == CimDataModelObjectStorageChangeType.Remove)
        {
            if (diff is AdditionDifferenceObject)
            {
                _DifferencesCache.Remove(diff.OID, out var _);
                diff = null;
                return;
            }

            if (diff is DeletionDifferenceObject)
            {
                throw new NotSupportedException(
                    "Unexpected change before removing difference!");
            }
            else
            {
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
                    _DifferencesCache.Remove(tmpDiff.OID, out var _);
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
            out IDifferenceObject? diff) == false)
        {
            diff = null;
        }

        diff ??= new UpdatingDifferenceObject(modelObject.OID);

        if (diff is UpdatingDifferenceObject 
            || diff is AdditionDifferenceObject)
        {
            diff.ChangePropertyValue(e.MetaProperty, oldData, newData);

            if (diff.ModifiedProperties.Count == 0)
            {
                _DifferencesCache.Remove(diff.OID, out var _);
                diff = null;
            }
        }
        else
        {
            throw new NotSupportedException(
                "Unexpected change before updating difference!");
        } 

        if (diff != null)
        {
            _DifferencesCache.TryAdd(diff.OID, diff);
        }
    }

    public void UnsubscribeFromDataModelChanges()
    {
        if (_subscribedDataModel == null)
        {
            return;
        }

        _subscribedDataModel.ModelObjectPropertyChanged 
            -= OnSubscribeModelObjectPropertyChanged;

        _subscribedDataModel.ModelObjectStorageChanged
            -= OnSubscribeModelObjectStorageChanged;
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

        serializerFactory.Settings = RedefinedDiffSerializerSettings(
            serializerFactory.Settings);

        base.Load(streamReader, serializerFactory, cimSchema);
    }

    public override void Save(StreamWriter streamWriter,
        IRdfSerializerFactory serializerFactory, ICimSchema cimSchema)
    {
        serializerFactory.Settings = RedefinedDiffSerializerSettings(
            serializerFactory.Settings);

        ToDifferenceModel();

        base.Save(streamWriter, serializerFactory, cimSchema);
    }

    public override void Parse(string content, 
        IRdfSerializerFactory serializerFactory, 
        ICimSchema cimSchema, Encoding? encoding = null)
    {
        serializerFactory.Settings = RedefinedDiffSerializerSettings(
            serializerFactory.Settings);

        base.Parse(content, serializerFactory, cimSchema, encoding);
    }

    private static RdfSerializerSettings RedefinedDiffSerializerSettings(
        RdfSerializerSettings rdfSerializerSettings)
    {
        return new RdfSerializerSettings()
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

    protected DifferenceModel? _internalDifferenceModel = null;

    protected ConcurrentDictionary<IOIDDescriptor, IDifferenceObject> 
    _DifferencesCache = [];

    protected ICimDataModel? _subscribedDataModel = null;
}
