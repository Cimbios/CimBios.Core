using System.Collections.Immutable;
using System.ComponentModel;
using System.Data;
using System.Text;
using CimBios.Core.CimModel.CimDatatypeLib;
using CimBios.Core.CimModel.RdfSerializer;
using CimBios.Core.CimModel.Schema;
using CimBios.Core.CimModel.Schema.AutoSchema;
using CimBios.Utils.ClassTraits;

namespace CimBios.Core.CimModel.Document;

/// <summary>
/// Instance of CIM model in Rdf/* format.
/// Supports input and output operations for CIM objects.
/// </summary>
public class CimDocument : ICanLog
{
    public ILogView Log => _Log;

    /// <summary>
    /// Model description.
    /// </summary>
    public FullModel? Description { get; set; }

    /// <summary>
    /// Applied schema to this context serializer.
    /// </summary>
    public ICimSchema? Schema => _serializer?.Schema;

    /// <summary>
    /// All cached objects collection (uuid to IModelObject).
    /// </summary>
    private Dictionary<string, IModelObject> _Objects { get; set; }

    public CimDocument()
    {
        _Log = new PlainLogView(this);
        _Objects = [];

        _serializer = new RdfXmlSerializer(
            new CimAutoSchemaXmlFactory().CreateSchema(), 
            new CimDatatypeLib.CimDatatypeLib());
    }

    public CimDocument(RdfSerializerBase rdfSerializer)
    {
        _Log = new PlainLogView(this);
        _Objects = [];

        _serializer = rdfSerializer;
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
            var serialized = _serializer.Deserialize(streamReader);
            _Objects = serialized.ToDictionary(k => k.Uuid, v => v);
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
    /// Get all model objects.
    /// </summary>
    /// <returns>IModelObject instance collection.</returns>
    public IEnumerable<IModelObject> GetAllObjects()
    {
        return _Objects.Values;
    }

    /// <summary>
    /// Get all typed model objects.
    /// </summary>
    /// <returns>IModelObject instance collection.</returns>
    public IEnumerable<T> GetObjects<T>() where T : IModelObject
    {
        return _Objects.Values.OfType<T>();
    }

    /// <summary>
    /// Get all meta typed model objects.
    /// </summary>
    /// <returns>IModelObject instance collection.</returns>
    public IEnumerable<IModelObject> GetObjects(ICimMetaClass metaClass)
    {
        return _Objects.Values.Where(o => o.MetaClass == metaClass);
    }

    /// <summary>
    /// Get generalized model object by uuid.
    /// </summary>
    /// <param name="uuid"></param>
    /// <returns>IModelObject instance or null.</returns>
    public IModelObject? GetObject(string uuid)
    {
        if (_Objects.TryGetValue(uuid, out var instance)
            && !instance.IsAuto
            && !instance.MetaClass.IsCompound)
        {
            instance.PropertyChanged 
                += Notify_ModelObjectPropertyChanged;

            return instance;
        }
        else
        {
            return null;
        }
    }

    /// <summary>
    /// Get typed model object by uuid.
    /// </summary>
    /// <typeparam name="T">IModelObject generalized class.</typeparam>
    /// <param name="uuid">Model object string identifier.</param>
    /// <returns>T casted IModelObject instance or null.</returns>
    public T? GetObject<T>(string uuid) where T : IModelObject
    {
        IModelObject? modelObject = GetObject(uuid);
        if (modelObject != null && modelObject is T typedObject)
        {
            return typedObject;
        }

        return default;
    }

    /// <summary>
    /// Remove object from model context.
    /// </summary>
    /// <param name="uuid">Model object string identifier.</param>
    /// <returns>True if object found and removed.</returns>
    public bool RemoveObject(string uuid)
    {
        return _Objects.Remove(uuid);
    }

    /// <summary>
    /// Fires on IModelObject property changed.
    /// </summary>
    /// <param name="sender">Firing IModelObject instance.</param>
    /// <param name="e">Changed property info.</param>
    private void Notify_ModelObjectPropertyChanged(object? sender, 
        PropertyChangedEventArgs e)
    {
    //    throw new NotImplementedException();
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
            Description = fullModel;
            _Objects.Remove(fullModel.Uuid);
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

    private RdfSerializerBase _serializer;

    private PlainLogView _Log;
}
