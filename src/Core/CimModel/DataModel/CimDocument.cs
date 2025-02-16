using System.Collections.Immutable;
using System.ComponentModel;
using System.Data;
using System.Text;
using CimBios.Core.CimModel.CimDatatypeLib;
using CimBios.Core.CimModel.CimDatatypeLib.Headers552;
using CimBios.Core.CimModel.RdfSerializer;
using CimBios.Core.CimModel.Schema;
using CimBios.Core.CimModel.Schema.RdfSchema;
using CimBios.Utils.ClassTraits;

namespace CimBios.Core.CimModel.CimDataModel;

/// <summary>
/// Instance of CIM model in Rdf/* format.
/// Supports input and output operations for CIM objects.
/// </summary>
public class CimDocument : ICimDataModel
{
    public ILogView Log => _Log;

    public FullModel? Description => _Description;

    public ICimSchema Schema => _schema;

    public ICimDatatypeLib TypeLib => _typeLib;

    public IReadOnlyCollection<ICimDataModelChangeStatement> Changes
        => _ChangesCache.Reverse().ToArray();

    /// <summary>
    /// All cached objects collection (uuid to IModelObject).
    /// </summary>
    private Dictionary<string, IModelObject> _Objects { get; set; }

    public CimDocument()
    {
        _Log = new PlainLogView(this);
        _Objects = [];
        _ChangesCache = [];

        var rdfSchema = new CimRdfSchemaXmlFactory().CreateSchema();
        _serializer = new RdfXmlSerializer(
            rdfSchema, 
            new CimDatatypeLib.CimDatatypeLib(rdfSchema))
            {
                Settings = new RdfSerializerSettings()
                {
                    UnknownClassesAllowed = true,
                    UnknownPropertiesAllowed = true
                }
            };

        _schema = _serializer.Schema;
        _typeLib = _serializer.TypeLib;
    }

    public CimDocument(RdfSerializerBase rdfSerializer)
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

        try
        {
            _ChangesCache = [];
            var serialized = _serializer.Deserialize(streamReader);
            _Objects = serialized.ToDictionary(k => k.OID, v => v);

            foreach (var obj in _Objects.Values)
            {
                obj.PropertyChanged += OnModelObjectPropertyChanged;
            }
        }
        catch (Exception ex)
        {
            _Log.NewMessage(
                "CimDocument: Deserialization failed.",
                LogMessageSeverity.Critical,
                ex.Message
            );
        }
        finally
        {        
            ReadModelDescription();
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

    public IEnumerable<IModelObject> GetAllObjects()
    {
        return _Objects.Values;
    }

    public IEnumerable<T> GetObjects<T>() where T : IModelObject
    {
        return _Objects.Values.OfType<T>();
    }

    public IEnumerable<IModelObject> GetObjects(ICimMetaClass metaClass)
    {
        return _Objects.Values.Where(o => o.MetaClass == metaClass);
    }

    public IModelObject? GetObject(string oid)
    {
        if (_Objects.TryGetValue(oid, out var instance)
            && !instance.IsAuto
            && !instance.MetaClass.IsCompound)
        {
            return instance;
        }
        else
        {
            return null;
        }
    }

    public T? GetObject<T>(string oid) where T : IModelObject
    {
        IModelObject? modelObject = GetObject(oid);
        if (modelObject != null && modelObject is T typedObject)
        {
            return typedObject;
        }

        return default;
    }

    public bool RemoveObject(string oid)
    {
        if (_Objects.TryGetValue(oid, out var removingObject)
            && _Objects.Remove(oid) == true)
        {
            UnlinkAllModelObjectAssocs(removingObject);

            removingObject.PropertyChanged -= OnModelObjectPropertyChanged;

            OnModelObjectStorageChanged(removingObject, 
                CimDataModelObjectStorageChangeType.Remove);

            return true;
        }

        return false;
    }

    public bool RemoveObject(IModelObject modelObject)
    {
        return RemoveObject(modelObject.OID);
    }

    public void RemoveObjects(IEnumerable<IModelObject> modelObjects)
    {
        foreach (var modelObject in modelObjects)
        {
            RemoveObject(modelObject);
        }
    }

    public IModelObject CreateObject(string oid, ICimMetaClass metaClass)
    {
        if (oid.Length == 0)
        {
            throw new ArgumentException("OID cannot be empty!");
        }       

        if (_Objects.ContainsKey(oid))
        {
            throw new ArgumentException($"Object with OID:{oid} already exists!");
        }

        var instance = TypeLib.CreateInstance(
            new ModelObjectFactory(), oid, metaClass, false);

        if (instance == null)
        {
            throw new NotSupportedException("TypeLib instance creation failed!");
        }

        AddObjectToStorage(instance);

        return instance;
    }

    public T CreateObject<T>(string oid) where T : class, IModelObject
    {
        if (oid.Length == 0)
        {
            throw new ArgumentException("OID cannot be empty!");
        }   

        var instance = TypeLib.CreateInstance<T>(oid, false);

        if (instance == null)
        {
            throw new NotSupportedException("TypeLib instance creation failed!");
        }

        AddObjectToStorage(instance);

        return instance;
    }

    public event CimDataModelObjectPropertyChangedEventHandler? 
        ModelObjectPropertyChanged;
    public event CimDataModelObjectStorageChangedEventHandler? 
        ModelObjectStorageChanged;


    public void DiscardLastChange()
    {
        throw new NotImplementedException();
    }
    
    public void DiscardAllChanges()
    {
        throw new NotImplementedException();
    }

    public void CommitAllChanges()
    {
        _ChangesCache.Clear();
    }

    private void AddObjectToStorage(IModelObject modelObject)
    {
        _Objects.Add(modelObject.OID, modelObject);
        modelObject.PropertyChanged += OnModelObjectPropertyChanged;

        OnModelObjectStorageChanged(modelObject, 
            CimDataModelObjectStorageChangeType.Add);
    }

    /// <summary>
    /// Find and extract IFullModel description from _objects cache.
    /// </summary>
    private void ReadModelDescription()
    {
        var fullModel = _Objects.Values.OfType<FullModel>()
            .FirstOrDefault();

        if (fullModel != null)
        {
            _Description = fullModel;
            _Objects.Remove(fullModel.OID);
        }
        else
        {
            _Log.NewMessage(
                "CimDocument: FullModel description node not found.",
                LogMessageSeverity.Warning
            );  

            // TODO: init FullModel self
        }
    }

    private void OnModelObjectPropertyChanged(object? sender, 
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

    private void OnModelObjectStorageChanged(IModelObject modelObject,
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

    private static void UnlinkAllModelObjectAssocs(IModelObject modelObject)
    {
        foreach (var assoc in modelObject.MetaClass.AllProperties)
        {
            if (assoc.PropertyKind == CimMetaPropertyKind.Assoc1To1)
            {
                modelObject.SetAssoc1To1(assoc, null);
            }
            else if (assoc.PropertyKind == CimMetaPropertyKind.Assoc1ToM)
            {
                modelObject.RemoveAllAssocs1ToM(assoc);
            }
        }
    }

    private PlainLogView _Log;

    private RdfSerializerBase   _serializer;
    private ICimSchema          _schema;
    private ICimDatatypeLib     _typeLib;

    private FullModel? _Description = null;

    private Stack<ICimDataModelChangeStatement> _ChangesCache = [];
}
