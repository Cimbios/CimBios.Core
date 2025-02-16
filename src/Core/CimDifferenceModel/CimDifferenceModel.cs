using System.Text;
using CimBios.Core.CimModel.CimDataModel;
using CimBios.Core.CimModel.CimDatatypeLib;
using CimBios.Core.CimModel.CimDatatypeLib.Headers552;
using CimBios.Core.CimModel.DatatypeLib;
using CimBios.Core.CimModel.DatatypeLib.ModelObject;
using CimBios.Core.CimModel.RdfSerializer;
using CimBios.Core.CimModel.Schema;
using CimBios.Utils.ClassTraits;

namespace CimBios.Core.CimDifferenceModel;

public class CimDifferenceModel : ICimDifferenceModel, ICanLog
{
    public ILogView Log => _Log;

    public CimDifferenceModel(RdfSerializerBase rdfSerializer)
    {
        _serializer = rdfSerializer;
        _serializer.Settings.UnknownClassesAllowed = true;
        _serializer.Settings.UnknownPropertiesAllowed = true;

        _Log = new PlainLogView(this);

        InitInternalDifferenceModel();
    }

    public CimDifferenceModel(RdfSerializerBase rdfSerializer, 
        ICimDataModel cimDataModel)
        : this(rdfSerializer)
    {
        ExtractFromDataModel(cimDataModel);
    }

    public void Load(StreamReader streamReader)
    {
        if (streamReader == null || _serializer == null)
        {
            _Log.NewMessage(
                "CimDifferenceModel: Stream reader or serializer has not been initialized!",
                LogMessageSeverity.Error
            );            

            return;
        }

        try
        {
            var serialized = _serializer.Deserialize(streamReader);
            var _objects = serialized.ToDictionary(k => k.OID, v => v);
            
            _internalDifferenceModel = _objects.Values
                .OfType<DifferenceModel>()
                .FirstOrDefault();
        }
        catch (Exception ex)
        {
            _Log.NewMessage(
                "CimDifferenceModel: Deserialization failed.",
                LogMessageSeverity.Critical,
                ex.Message
            );
        }
        finally
        {        
            streamReader.Close();
        }
    }

    public void Load(string path)
    {
        throw new NotImplementedException();
    }

    public void Parse(string content, Encoding? encoding = null)
    {
        throw new NotImplementedException();
    }

    public void Save(StreamWriter streamWriter)
    {
        if (streamWriter == null || _serializer == null)
        {
            _Log.NewMessage(
                "CimDifferenceModel: Stream writer provider has not been initialized!",
                LogMessageSeverity.Error
             );  

            return;
        }  

        List<IModelObject> forSerializeObjects = [_internalDifferenceModel];

        try
        {
            _serializer.Serialize(streamWriter, forSerializeObjects);
        }
        catch (Exception ex)
        {
            _Log.NewMessage(
                "CimDifferenceModel: Serialization failed.",
                LogMessageSeverity.Critical,
                ex.Message
            );
        }
    }

    public void Save(string path)
    {
        throw new NotImplementedException();
    }

    public void ExtractFromDataModel(ICimDataModel cimDataModel)
    {
        _differenceCache.Clear();

        var changes = cimDataModel.Changes;

        foreach (var changeStatement in changes)
        {
            if (changeStatement is CimDataModelObjectAddedStatement added)
            {

            }
            else if (changeStatement is CimDataModelObjectUpdatedStatement updated)
            {
                
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
        _differenceCache.Clear();

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

    private 
    DifferenceModel _InternalDifferenceModel
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

    private Dictionary<string, ICimDifferenceStatement> _differenceCache = [];

    private RdfSerializerBase _serializer;

    private ICimSchema _schema => _serializer.Schema;

    private PlainLogView _Log;
}

public interface IDifferenceObject : IModelObject
{
    public IReadOnlyModelObject? OriginalObject { get; }
}

public class DifferenceObject : WeakModelObject, IDifferenceObject
{
    public IReadOnlyModelObject? OriginalObject => _originalObject;

    public DifferenceObject(string oid, CimMetaClassBase metaClass)
        : base(oid, metaClass, false)
    {

    }

    private WeakModelObject? _originalObject;
}

public interface ICimDifferenceStatement
{
    public string OID { get; }

    public IDifferenceObject? OriginalObjectData { get; }
    public IDifferenceObject? ModifiedObjectData { get; }
}

public class CimDifferenceStatement : ICimDifferenceStatement
{
    public string OID => throw new NotImplementedException();

    public IDifferenceObject? OriginalObjectData => throw new NotImplementedException();

    public IDifferenceObject? ModifiedObjectData => throw new NotImplementedException();
}