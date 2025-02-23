using CimBios.Core.CimModel.CimDataModel;
using CimBios.Core.CimModel.CimDatatypeLib;
using CimBios.Core.CimModel.CimDatatypeLib.Headers552;
using CimBios.Core.CimModel.DataModel.Utils;
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
    }

    public CimDifferenceModel(RdfSerializerBase rdfSerializer, 
        ICimDataModel cimDataModel)
        : this(rdfSerializer)
    {
        ExtractFromDataModel(cimDataModel);
    }

    public void ExtractFromDataModel(ICimDataModel cimDataModel)
    {
        ResetAll();
        
        var diffsHelper = new DiffsFromModelHelper(cimDataModel);
        _DifferencesCache = diffsHelper.Differences;
        ToDifferenceModel();
    }

    public void InvalidateWithDataModel(ICimDataModel cimDataModel)
    {
        throw new NotImplementedException();
    }

    public void ResetAll()
    {
        _internalDifferenceModel = TypeLib.CreateInstance<DifferenceModel>(
            Guid.NewGuid().ToString(),
            isAuto: false
        );

        if (_internalDifferenceModel == null)
        {
            throw new NotSupportedException
            ("dm:DifferenceModel instance initialization failed!");
        }

        _DifferencesCache.Clear();
    }

    private void ToDifferenceModel()
    {
        _DifferencesCache.Values.AsParallel().ForAll(
            diff =>
            {
                if (diff.ModifiedObject is not IModelObject modifiedObject)
                {
                    return;
                }

                if (diff is AdditionDifferenceObject
                    || diff is UpdatingDifferenceObject)
                {
                    _InternalDifferenceModel.forwardDifferences.Add(
                        modifiedObject);
                }

                if (diff is DeletionDifferenceObject
                    || diff is UpdatingDifferenceObject)
                {
                    _InternalDifferenceModel.reverseDifferences.Add(
                        modifiedObject);       
                }

                if (diff is UpdatingDifferenceObject
                    && diff.OriginalObject is IModelObject originalObject)
                {
                    _InternalDifferenceModel.reverseDifferences.Add(
                        originalObject); 
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

        _InternalDifferenceModel.forwardDifferences
            .AsParallel().ForAll(s => 
            {
                if (s.MetaClass == descriptionMetaClass)
                {
                    var updDiff = new UpdatingDifferenceObject(s.OID);
                    
                }
                else
                {
                    var addDiff = new AdditionDifferenceObject(
                        s.OID, s.MetaClass);

                    _DifferencesCache.Add(addDiff.OID, addDiff);
                }
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

    public override IModelObject? GetObject(string oid)
    {
        throw new NotImplementedException(
            "DifferenceModel does not provides this method implementation!");
    }

    public override T? GetObject<T>(string oid) where T : default
    {
        throw new NotImplementedException(
            "DifferenceModel does not provides this method implementation!");
    }

    public override bool RemoveObject(string oid)
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

    public override IModelObject CreateObject(string oid, ICimMetaClass metaClass)
    {
        throw new NotImplementedException(
            "DifferenceModel does not provides this method implementation!");
    }

    public override T CreateObject<T>(string oid)
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

    private Dictionary<string, IDifferenceObject> _DifferencesCache = [];
}
