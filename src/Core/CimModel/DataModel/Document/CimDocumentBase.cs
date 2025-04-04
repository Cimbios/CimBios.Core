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
using CimBios.Utils.ClassTraits.CanLog;

namespace CimBios.Core.CimModel.CimDataModel;

public abstract class CimDocumentBase : ICimDataModel, ICanLog
{
    public virtual ILogView Log => _Log;

    public virtual Model? ModelDescription { get; protected set; }

    public virtual ICimSchema Schema { get; }

    public virtual ICimDatatypeLib TypeLib { get; }

    public virtual IOIDDescriptorFactory OIDDescriptorFactory { get; } 
        = new UuidDescriptorFactory();


    protected IReadOnlyCollection<ModelObjectUnresolvedReference> 
    _UnresolvedReferences { get; set; } = [];

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
            _Objects = [];
            deserialized = serializer.Deserialize(streamReader);   
            PushDeserializedObjects(deserialized);         
        }
        catch (Exception ex)
        {
            _Log.Critical($"Deserialization failed: {ex.Message}");
            throw;
        }
        finally
        {        
            streamReader.Close();

            _UnresolvedReferences = serializer.UnresolvedReferences
                .ToList().AsReadOnly();

            _Log.FlushFrom(serializer.Log);
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
        Load(new StreamReader(path), serializerFactory, cimSchema);
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

        var serializer = serializerFactory.Create(cimSchema, 
            TypeLib, OIDDescriptorFactory);

        try
        {
            serializer.BaseUri = new(OIDDescriptorFactory.Namespace);
            serializer.Serialize(streamWriter, forSerializeObjects);
        }
        catch (Exception ex)
        {
            _Log.Critical($"Serialization failed: {ex.Message}");
            throw;
        }
        finally
        {
            streamWriter.Close();

            _Log.FlushFrom(serializer.Log);
        }
    }

    /// <summary>
    /// Write CIM model to stream writer.
    /// </summary>
    public virtual void Save(StreamWriter streamWriter, 
        IRdfSerializerFactory serializerFactory)
    {
        Save(streamWriter, serializerFactory, Schema);
    }

    /// <summary>
    /// Save CIM model to file.
    /// </summary>
    public virtual void Save(string path, IRdfSerializerFactory serializerFactory,
        ICimSchema cimSchema)
    {
        Save(new StreamWriter(path),serializerFactory, cimSchema);        
    }

    /// <summary>
    /// Save CIM model to file.
    /// </summary>
    public virtual void Save(string path, IRdfSerializerFactory serializerFactory)
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

    public event CimDataModelObjectPropertyChangedEventHandler? 
        ModelObjectPropertyChanged;
    public event CimDataModelObjectStorageChangedEventHandler? 
        ModelObjectStorageChanged;

    /// <summary>
    /// Event fires on any model object property changed.
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

        ModelObjectPropertyChanged?.Invoke(this, modelObject, cimEv);
    }

    /// <summary>
    /// Event fires on any model object property changing request.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    /// <exception cref="NotImplementedException"></exception>
    protected void OnModelObjectPropertyChanging(object? sender, 
        CanCancelPropertyChangingEventArgs e)
    {
        if (e is CanCancelAssocChangingEventArgs assocChanging)
        {
            if (assocChanging.ModelObject != null)
            {
                if (GetObject(assocChanging.ModelObject.OID) 
                    != assocChanging.ModelObject)
                {
                    e.Cancel = true;
                    throw new InvalidDataException(
                        "This context does not contains sending association object!");
                }
            }
        }
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

        ModelObjectStorageChanged?.Invoke(this, modelObject, 
            new CimDataModelObjectStorageChangedEventArgs(changeType));
    }

    protected readonly PlainLogView _Log;
}
