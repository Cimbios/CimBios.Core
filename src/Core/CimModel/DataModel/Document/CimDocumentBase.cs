using System.Collections.Immutable;
using System.ComponentModel;
using System.Text;
using CimBios.Core.CimModel.CimDataModel.Utils;
using CimBios.Core.CimModel.CimDatatypeLib;
using CimBios.Core.CimModel.CimDatatypeLib.EventUtils;
using CimBios.Core.CimModel.CimDatatypeLib.Headers552;
using CimBios.Core.CimModel.RdfSerializer;
using CimBios.Core.CimModel.Schema;
using CimBios.Utils.ClassTraits;

namespace CimBios.Core.CimModel.CimDataModel;

public abstract class CimDocumentBase : ICimDataModel, ICanLog
{
    public virtual ILogView Log => _Log;

    public virtual Model? Description => _Description;

    public virtual ICimSchema Schema => _schema;

    public virtual ICimDatatypeLib TypeLib => _typeLib;

    public IReadOnlyCollection<ICimDataModelChangeStatement> Changes
        => _ChangesCache.Reverse().ToArray();

    /// <summary>
    /// All cached objects collection (uuid to IModelObject).
    /// </summary>
    protected Dictionary<string, IModelObject> _Objects { get; set; }

    protected CimDocumentBase(RdfSerializerBase rdfSerializer)
    {
        _Log = new PlainLogView(this);
        _Objects = [];
        _ChangesCache = [];

        _serializer = rdfSerializer;
        _schema = _serializer.Schema;
        _typeLib = _serializer.TypeLib;
    }

    /// <summary>
    /// Load CIM model to context via stream reader.
    /// </summary>
    public void Load(StreamReader streamReader)
    {
        if (streamReader == null || _serializer == null)
        {
            _Log.NewMessage(
                "CimDocument: Stream reader or serializer has not been initialized!",
                LogMessageSeverity.Error
            );            

            return;
        }

        IEnumerable<IModelObject> deserialized;
        try
        {
            _ChangesCache = [];
            deserialized = _serializer.Deserialize(streamReader);
            PushDeserializedObjects(deserialized);
        }
        catch (Exception ex)
        {
            streamReader.Close();
            _Log.NewMessage(
                "CimDocument: Deserialization failed.",
                LogMessageSeverity.Critical,
                ex.Message
            );
        }
        finally
        {        
            streamReader.Close();
        }
    }

    /// <summary>
    /// Load CIM model to context by path.
    /// </summary>
    public void Load(string path)
    {
        Load(new StreamReader(File.Open(path, FileMode.Open)));
    }

    /// <summary>
    /// Parse CIM model to context from string.
    /// </summary>
    public void Parse(string content, Encoding? encoding = null)
    {
        encoding ??= Encoding.Default;
        var memoryStream = new MemoryStream(encoding.GetBytes(content));
        var stringReader = new StreamReader(memoryStream);
        Load(stringReader);
    }

    /// <summary>
    /// Write CIM model to stream writer.
    /// </summary>
    public void Save(StreamWriter streamWriter)
    {
        if (streamWriter == null || _serializer == null)
        {
            _Log.NewMessage(
                "CimDocument: Stream writer provider has not been initialized!",
                LogMessageSeverity.Error
            );  

            return;
        }  

        var forSerializeObjects = _Objects.Values.ToImmutableList();
        if (Description != null)
        {
            forSerializeObjects.Add(Description);
        }

        try
        {
            _serializer.Serialize(streamWriter, forSerializeObjects);
        }
        catch (Exception ex)
        {
            _Log.NewMessage(
                "CimDocument: Serialization failed.",
                LogMessageSeverity.Critical,
                ex.Message
            );
        }
    }

    /// <summary>
    /// Save CIM model to file.
    /// </summary>
    public void Save(string path)
    {
        Save(new StreamWriter(path));        
    }

    /// <summary>
    /// Push deserialized model objects to storage.
    /// </summary>
    /// <param name="cache">Model objects collection to push.</param>
    protected abstract void PushDeserializedObjects(
        IEnumerable<IModelObject> cache);

    public abstract IEnumerable<IModelObject> GetAllObjects();
    public abstract IEnumerable<T> GetObjects<T>() where T : IModelObject;
    public abstract IEnumerable<IModelObject> GetObjects(ICimMetaClass metaClass);
    public abstract IModelObject? GetObject(string oid);
    public abstract T? GetObject<T>(string oid) where T : IModelObject;
    public abstract bool RemoveObject(string oid);
    public abstract bool RemoveObject(IModelObject modelObject);
    public abstract void RemoveObjects(IEnumerable<IModelObject> modelObjects);
    public abstract IModelObject CreateObject(string oid, ICimMetaClass metaClass);
    public abstract T CreateObject<T>(string oid) where T : class, IModelObject;


    public virtual void DiscardLastChange()
    {
        throw new NotImplementedException();
    }
    
    public virtual void DiscardAllChanges()
    {
        throw new NotImplementedException();
    }

    public virtual void CommitAllChanges()
    {
        _ChangesCache.Clear();
    }

    public event CimDataModelObjectPropertyChangedEventHandler? 
        ModelObjectPropertyChanged;
    public event CimDataModelObjectStorageChangedEventHandler? 
        ModelObjectStorageChanged;

    /// <summary>
    /// Event fires on any model object proprty changed.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void OnModelObjectPropertyChanged(object? sender, 
        PropertyChangedEventArgs e)
    {
        if (sender is not IModelObject modelObject
            || e is not CimMetaPropertyChangedEventArgs cimEv)
        {
            return;
        }

        var updateStatement = new CimDataModelObjectUpdatedStatement(
            modelObject, cimEv);

        if (_ChangesCache.TryPeek(out var lastChange)
            && lastChange.ModelObject == modelObject
            && lastChange is CimDataModelObjectUpdatedStatement luStatement)
        {
             // Discarding last change.
            if (luStatement.NewValue == updateStatement.OldValue
                && luStatement.OldValue == updateStatement.NewValue)
            {
                _ChangesCache.Pop();
                return;
            }
        }

        _ChangesCache.Push(updateStatement);

        ModelObjectPropertyChanged?.Invoke(this, modelObject, cimEv);
    }

    /// <summary>
    /// Event fires on object add or removed from document storage.
    /// </summary>
    /// <param name="modelObject"></param>
    /// <param name="changeType"></param>
    protected void OnModelObjectStorageChanged(IModelObject modelObject,
        CimDataModelObjectStorageChangeType changeType)
    {
        if (modelObject.IsAuto)
        {
            return;
        }

        if (_ChangesCache.TryPeek(out var lastChange)
            && lastChange.ModelObject == modelObject)
        {
            // Discarding last change.
            if ((lastChange is CimDataModelObjectAddedStatement
                    && changeType == CimDataModelObjectStorageChangeType.Remove)
                || (lastChange is CimDataModelObjectRemovedStatement
                    && changeType == CimDataModelObjectStorageChangeType.Add))
            {
                _ChangesCache.Pop();
                return;
            }
        }

        if (changeType == CimDataModelObjectStorageChangeType.Add)
        {
            _ChangesCache.Push(
                new CimDataModelObjectAddedStatement(modelObject));
        }
        if (changeType == CimDataModelObjectStorageChangeType.Remove)
        {
            _ChangesCache.Push(
                new CimDataModelObjectRemovedStatement(modelObject));
        }

        ModelObjectStorageChanged?.Invoke(this, modelObject, 
            new CimDataModelObjectStorageChangedEventArgs(changeType));
    }

    protected readonly PlainLogView _Log;

    protected readonly RdfSerializerBase _serializer;
    protected readonly ICimSchema _schema;
    protected readonly ICimDatatypeLib _typeLib;

    protected Model? _Description = null;

    protected Stack<ICimDataModelChangeStatement> _ChangesCache;
}
