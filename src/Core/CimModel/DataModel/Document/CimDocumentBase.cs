using System.Collections.Immutable;
using System.ComponentModel;
using System.Text;
using CimBios.Core.CimModel.CimDataModel.Utils;
using CimBios.Core.CimModel.CimDatatypeLib;
using CimBios.Core.CimModel.CimDatatypeLib.EventUtils;
using CimBios.Core.CimModel.CimDatatypeLib.Headers552;
using CimBios.Core.CimModel.CimDatatypeLib.OID;
using CimBios.Core.CimModel.RdfSerializer;
using CimBios.Core.CimModel.Schema;
using CimBios.Utils.ClassTraits;

namespace CimBios.Core.CimModel.CimDataModel;

public abstract class CimDocumentBase : ICimDataModel, ICanLog
{
    public virtual ILogView Log => Log;

    public virtual Model? ModelDescription { get; protected set; }

    public virtual ICimSchema Schema { get; }

    public virtual ICimDatatypeLib TypeLib { get; }

    public virtual IOIDDescriptorFactory OIDDescriptorFactory { get; } 
        = new GuidDescriptorFactory();

    public IReadOnlyCollection<ICimDataModelChangeStatement> Changes
        => _ChangesCache.Reverse().ToArray();

    /// <summary>
    /// All cached objects collection (uuid to IModelObject).
    /// </summary>
    protected virtual Dictionary<IOIDDescriptor, IModelObject> _Objects 
    { get; set; }

    protected CimDocumentBase(ICimSchema cimSchema, ICimDatatypeLib typeLib,
        IOIDDescriptorFactory oidDescriptorFactory)
    {
        _Log = new PlainLogView(this);
        _Objects = [];
        _ChangesCache = [];

        Schema = cimSchema;
        TypeLib = typeLib;
        OIDDescriptorFactory = oidDescriptorFactory;
    }

    /// <summary>
    /// Load CIM model to context via stream reader and custom schema.
    /// </summary>
    public virtual void Load(StreamReader streamReader, 
        IRdfSerializerFactory serializerFactory,
        ICimSchema cimSchema)
    {
        var serializer = serializerFactory.Create(cimSchema, 
            TypeLib, OIDDescriptorFactory);

        serializer.BaseUri = new(OIDDescriptorFactory.Namespace);
        IEnumerable<IModelObject> deserialized;

        try
        {
            _ChangesCache = [];
            deserialized = serializer.Deserialize(streamReader);
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
    /// Load CIM model to context via stream reader.
    /// </summary>
    public void Load(StreamReader streamReader, 
        IRdfSerializerFactory serializerFactory)
    {
        Load(streamReader, serializerFactory, Schema);
    }

    /// <summary>
    /// Load CIM model to context by path.
    /// </summary>
    public void Load(string path, 
        IRdfSerializerFactory serializerFactory,
        ICimSchema cimSchema)
    {
        Load(new StreamReader(File.Open(path, FileMode.Open)), 
            serializerFactory, cimSchema);
    }

    /// <summary>
    /// Load CIM model to context by path.
    /// </summary>
    public void Load(string path, IRdfSerializerFactory serializerFactory)
    {
        Load(path, serializerFactory, Schema);
    }

    /// <summary>
    /// Parse CIM model to context from string.
    /// </summary>
    public virtual void Parse(string content, IRdfSerializerFactory serializerFactory,
        ICimSchema cimSchema, Encoding? encoding = null)
    {
        encoding ??= Encoding.Default;
        var memoryStream = new MemoryStream(encoding.GetBytes(content));
        var stringReader = new StreamReader(memoryStream);
        Load(stringReader, serializerFactory, cimSchema);
    }

    /// <summary>
    /// Parse CIM model to context from string.
    /// </summary>
    public void Parse(string content, IRdfSerializerFactory serializerFactory,
        Encoding? encoding = null)
    {
        Parse(content, serializerFactory, Schema, encoding);
    }

    /// <summary>
    /// Write CIM model to stream writer.
    /// </summary>
    public virtual void Save(StreamWriter streamWriter,
        IRdfSerializerFactory serializerFactory,
        ICimSchema cimSchema)
    {
        var forSerializeObjects = _Objects.Values.ToImmutableList();
        if (ModelDescription != null)
        {
            forSerializeObjects.Add(ModelDescription);
        }

        try
        {
            var serializer = serializerFactory.Create(cimSchema, 
                TypeLib, OIDDescriptorFactory);

            serializer.BaseUri = new(OIDDescriptorFactory.Namespace);
            serializer.Serialize(streamWriter, forSerializeObjects);
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
    /// Write CIM model to stream writer.
    /// </summary>
    public void Save(StreamWriter streamWriter, 
        IRdfSerializerFactory serializerFactory)
    {
        Save(streamWriter, serializerFactory, Schema);
    }

    /// <summary>
    /// Save CIM model to file.
    /// </summary>
    public void Save(string path, IRdfSerializerFactory serializerFactory,
        ICimSchema cimSchema)
    {
        Save(new StreamWriter(path),serializerFactory, cimSchema);        
    }

    /// <summary>
    /// Save CIM model to file.
    /// </summary>
    public void Save(string path, IRdfSerializerFactory serializerFactory)
    {
        Save(new StreamWriter(path),serializerFactory, Schema);        
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
    public abstract IModelObject? GetObject(IOIDDescriptor oid);
    public abstract T? GetObject<T>(IOIDDescriptor oid) where T : IModelObject;
    public abstract bool RemoveObject(IOIDDescriptor oid);
    public abstract bool RemoveObject(IModelObject modelObject);
    public abstract void RemoveObjects(IEnumerable<IModelObject> modelObjects);
    public abstract IModelObject CreateObject(IOIDDescriptor oid, ICimMetaClass metaClass);
    public abstract T CreateObject<T>(IOIDDescriptor oid) where T : class, IModelObject;

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
        if (modelObject.OID is AutoDescriptor)
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

    protected Stack<ICimDataModelChangeStatement> _ChangesCache;

    protected readonly PlainLogView _Log;
}
