using System.Collections.ObjectModel;
using System.Text;
using CimBios.Core.CimModel.CimDataModel;
using CimBios.Core.CimModel.CimDatatypeLib;
using CimBios.Core.CimModel.CimDatatypeLib.Headers552;
using CimBios.Core.CimModel.RdfSerializer;
using CimBios.Core.CimModel.Schema;
using CimBios.Utils.ClassTraits;

namespace CimBios.Core.CimDifferenceModel;

public class CimDifferenceModel : ICimDifferenceModel, ICanLog
{
    public ILogView Log => _Log;

    public IReadOnlyCollection<IDifferenceObject> Differences
         => _DifferencesCache.Values;

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
                    var tmoDiff = diff;
                    diff = new DeletionDifferenceObject(removed.ModelObject.OID);
                    foreach (var prop in removed.ModelObject
                        .MetaClass.AllProperties)
                    {
                        var propValue = removed.ModelObject
                            .TryGetPropertyValue(prop);
                        
                        diff.ChangePropertyValue(prop, null, propValue);
                    }

                    if (tmoDiff is UpdatingDifferenceObject upd)
                    {
                        _DifferencesCache.Remove(tmoDiff.OID);
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

    private RdfSerializerBase _serializer;

    private ICimSchema _schema => _serializer.Schema;

    private PlainLogView _Log;
}
