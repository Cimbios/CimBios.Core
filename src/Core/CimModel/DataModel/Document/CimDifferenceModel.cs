using System.Text;
using CimBios.Core.CimModel.CimDataModel;
using CimBios.Core.CimModel.CimDatatypeLib;
using CimBios.Core.CimModel.CimDatatypeLib.Headers552;
using CimBios.Core.CimModel.CimDatatypeLib.OID;
using CimBios.Core.CimModel.DataModel.Utils;
using CimBios.Core.CimModel.RdfSerializer;
using CimBios.Core.CimModel.Schema;

namespace CimBios.Core.CimModel.CimDifferenceModel;

public class CimDifferenceModel(ICimSchema cimSchema, ICimDatatypeLib typeLib,
    IOIDDescriptorFactory oidDescriptorFactory) 
    : CimDocumentBase(cimSchema, typeLib, oidDescriptorFactory), 
    ICimDifferenceModel
{
    public IReadOnlyCollection<IDifferenceObject> Differences
         => _DifferencesCache.Values;

    public CimDifferenceModel(ICimSchema cimSchema, ICimDatatypeLib typeLib,
        ICimDataModel cimDataModel)
        : this(cimSchema, typeLib, cimDataModel.OIDDescriptorFactory)
    {
        ExtractFromDataModel(cimDataModel);
    }

     public void ApplyToDataModel(ICimDataModel cimDataModel)
     {
        DiffsToModelHelper.ApplyTo(cimDataModel, _DifferencesCache.Values);
     }

    public void ExtractFromDataModel(ICimDataModel cimDataModel)
    {
        ResetAll();
        
        _DifferencesCache = DiffsFromModelHelper.ExtractFrom(cimDataModel);
        ToDifferenceModel();
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

                    _DifferencesCache.Add(addDiff.OID, addDiff);
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

                    _DifferencesCache.Add(delDiff.OID, delDiff);
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

                    _DifferencesCache.Add(updDiff.OID, updDiff);
                }
            }
        );

        waitingForwardUpdates.Values.AsParallel().ForAll(w =>
            {
                if (w == null)
                {
                    return;
                }
                
                var modified = new WeakModelObject(w.OID, 
                    descriptionMetaClass);

                var updDiff = new UpdatingDifferenceObject(w, modified);

                 _DifferencesCache.Add(updDiff.OID, updDiff);
            }
        );
    }

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
        serializerFactory.Settings = new RdfSerializerSettings()
        {
            UnknownClassesAllowed = true,
            UnknownPropertiesAllowed = true
        };

        base.Load(streamReader, serializerFactory, cimSchema);
    }

    public override void Save(StreamWriter streamWriter,
        IRdfSerializerFactory serializerFactory, ICimSchema cimSchema)
    {
        serializerFactory.Settings = new RdfSerializerSettings()
        {
            UnknownClassesAllowed = true,
            UnknownPropertiesAllowed = true
        };

        base.Save(streamWriter, serializerFactory, cimSchema);
    }

    public override void Parse(string content, 
        IRdfSerializerFactory serializerFactory, 
        ICimSchema cimSchema, Encoding? encoding = null)
    {
        serializerFactory.Settings = new RdfSerializerSettings()
        {
            UnknownClassesAllowed = true,
            UnknownPropertiesAllowed = true
        };

        base.Parse(content, serializerFactory, cimSchema, encoding);
    }

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

    private DifferenceModel? _internalDifferenceModel = null;

    private Dictionary<IOIDDescriptor, IDifferenceObject> _DifferencesCache = [];
}
