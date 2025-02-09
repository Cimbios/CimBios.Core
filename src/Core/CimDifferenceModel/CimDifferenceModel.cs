using System.Text;
using CimBios.Core.CimModel.CimDataModel;
using CimBios.Core.CimModel.CimDatatypeLib;
using CimBios.Core.CimModel.CimDatatypeLib.Headers552;
using CimBios.Core.CimModel.DatatypeLib;
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
                PushAddStatement(added);
            }
            else if (changeStatement is CimDataModelObjectUpdatedStatement updated)
            {

            }
        } 

        FlushDiffCahceToDifferenceModel();
    }

    private void PushAddStatement(CimDataModelObjectAddedStatement statement)
    {
        if (_differenceCache.ContainsKey(statement.ModelObject.OID) == false)
        {
            var addDiffObject = _serializer.TypeLib.CreateInstance(
                    new WeakModelObjectFactory(),
                    statement.ModelObject.OID,
                    statement.ModelObject.MetaClass,
                    false
                );

            var addDiffStatement = 
                new AddCimDifferenceStatement(addDiffObject);
            _differenceCache.Add(addDiffStatement.OID, addDiffStatement);
        }
        else
        {
            throw new NotSupportedException("tmp: Adding object already modified!");
        }
    }

    private void PushUpdateStatement(CimDataModelObjectUpdatedStatement statement)
    {
        if (_differenceCache.ContainsKey(statement.ModelObject.OID) == false)
        {
            var descriptionMetaClass = _serializer.Schema
                .TryGetResource<ICimMetaClass>(new(DifferenceModel.ClassUri)
            );

            var fwdDiffObject = _serializer.TypeLib.CreateInstance(
                    new WeakModelObjectFactory(),
                    statement.ModelObject.OID,
                    descriptionMetaClass,
                    false
                );

            var rvsDiffObject = _serializer.TypeLib.CreateInstance(
                    new WeakModelObjectFactory(),
                    statement.ModelObject.OID,
                    descriptionMetaClass,
                    false
                );     

            var updDiffStatement = 
                new UpdateCimDifferenceStatement(
                    fwdDiffObject, 
                    statement.OldValue as IModelObject);

            _differenceCache.Add(updDiffStatement.OID, updDiffStatement);
        }
    }

    private static void SetModelObjectData(IModelObject modelObject, 
        ICimMetaProperty metaProperty, object value)
    {
        if (metaProperty.PropertyKind == CimMetaPropertyKind.Attribute)
        {
            modelObject.SetAttribute(metaProperty, value);
        }
        else if (metaProperty.PropertyKind == CimMetaPropertyKind.Assoc1To1)
        {
            modelObject.SetAssoc1To1(metaProperty, value as IModelObject);
        }
        else if (metaProperty.PropertyKind == CimMetaPropertyKind.Assoc1ToM)
        {
            modelObject.AddAssoc1ToM(metaProperty, value as IModelObject);
        }
    }

    private void FlushDiffCahceToDifferenceModel()
    {
        _InternalDifferenceModel.forwardDifferences.Clear();
        _InternalDifferenceModel.reverseDifferences.Clear();

        foreach (var diff in _differenceCache.Values)
        {
            if (diff is AddCimDifferenceStatement addDiff)
            {
                _InternalDifferenceModel.forwardDifferences
                    .Add(addDiff.AddObject);
            }

            
        }

        _differenceCache.Clear();
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
            throw new NotSupportedException("dm:DifferenceModel instance initialization failed!");
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
                throw new NotSupportedException("Internal difference model has not been initialized!");
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

internal interface ICimDifferenceStatement
{
    public string OID { get; }
}

internal sealed class AddCimDifferenceStatement 
    (IModelObject addDifferenceObject)
    : ICimDifferenceStatement
{
    public string OID => AddObject.OID;

    public IModelObject AddObject { get; } = addDifferenceObject;
}

internal sealed class RemoveCimDifferenceStatement 
    (IModelObject removeDifferenceObject)
    : ICimDifferenceStatement
{
    public string OID => RemoveObject.OID;

    public IModelObject RemoveObject { get; } = removeDifferenceObject;
}

internal sealed class UpdateCimDifferenceStatement 
    : ICimDifferenceStatement
{
    public string OID => ForwardObject.OID;

    public IModelObject ForwardObject { get; } 
    public IModelObject ReverseObject { get; }

    public UpdateCimDifferenceStatement (IModelObject forwardObject, 
        IModelObject reverseObject)
    {
        ForwardObject = forwardObject;
        ReverseObject = reverseObject;        
    }
}