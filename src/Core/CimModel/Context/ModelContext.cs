using System.Collections.Immutable;
using System.ComponentModel;
using System.Data;
using CimBios.Core.CimModel.CimDatatypeLib;
using CimBios.Core.CimModel.RdfSerializer;
using CimBios.Core.CimModel.Schema;
using CimBios.Core.DataProvider;
using CimBios.Utils.ClassTraits;

namespace CimBios.Core.CimModel.Context;

/// <summary>
/// Instance of CIM model in Rdf/* format.
/// Supports input and output operations for CIM objects.
/// </summary>
public class ModelContext : ICanLog
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

    public ModelContext()
    {
        _Log = new PlainLogView(this);

        _Objects = new Dictionary<string, IModelObject>();
    }

    public ModelContext(IModelObjectsProvider modelObjectsProvider) : this()
    {
        InitContextDataConfig(modelObjectsProvider);
    }

    /// <summary>
    /// Load CIM model to context via config.
    /// </summary>
    public void Load()
    {
        if (_provider == null || _serializer == null)
        {
            _Log.NewMessage(
                "ModelContext: Object data provider has not been initialized!",
                LogMessageSeverity.Error
            );            

            return;
        }

        _Log.NewMessage(
            "ModelContext: Loading model context.",
            LogMessageSeverity.Info,
            _provider.Source.AbsoluteUri
        );

        try
        {
            var serialized = _serializer.Deserialize();
            _Objects = serialized.ToDictionary(k => k.Uuid, v => v);
        }
        catch (Exception ex)
        {
            _Log.NewMessage(
                "ModelContext: Deserialization failed.",
                LogMessageSeverity.Critical,
                ex.Message
            );
        }
        finally
        {        
            ReadModelDescription();

            ModelLoaded?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <summary>
    /// Load CIM model to context via config.
    /// </summary>
    /// <param name="contextDataConfig">Context configuration.</param>
    public void Load(IModelObjectsProvider modelObjectsProvider)
    {
        InitContextDataConfig(modelObjectsProvider);

        Load();
    }

    /// <summary>
    /// Save CIM model to context via config.
    /// </summary>
    public void Save()
    {
        if (_provider == null || _serializer == null)
        {
            _Log.NewMessage(
                "ModelContext: Object data provider has not been initialized!",
                LogMessageSeverity.Error
            );  

            return;
        }  

        _Log.NewMessage(
            "ModelContext: Saving model context.",
            LogMessageSeverity.Info,
            _provider.Source.AbsoluteUri
        ); 

        var forSerializeObjects = _Objects.Values.ToImmutableList();
        if (Description != null)
        {
            forSerializeObjects.Add(Description);
        }

        try
        {
            _serializer.Serialize(forSerializeObjects);
        }
        catch (Exception ex)
        {
            _Log.NewMessage(
                "ModelContext: Serialization failed.",
                LogMessageSeverity.Critical,
                ex.Message
            );
        }
    }

    /// <summary>
    /// Save CIM model to context via config.
    /// </summary>
    /// <param name="contextDataConfig">Context configuration.</param>
    public void Save(IModelObjectsProvider modelObjectsProvider)
    {
        InitContextDataConfig(modelObjectsProvider);

        Save();        
    }

    /// <summary>
    /// Get all model objects.
    /// </summary>
    /// <returns>IModelObject instance collection.</returns>
    public IEnumerable<IModelObject> GetAllObjects()
    {
        return _Objects.ToList().Select(kvp => kvp.Value);
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
    /// Initialize read/write model stategy.
    /// </summary>
    /// <param name="modelObjectsProvider">Context configuration.</param>
    private void InitContextDataConfig(
        IModelObjectsProvider modelObjectsProvider)
    {
        _provider = modelObjectsProvider.DataProvider;
        _serializer = modelObjectsProvider.Serializer;
        _serializer.TypeLib = modelObjectsProvider.TypeLib;
        _serializer.Schema = modelObjectsProvider.CimSchema;
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
                "ModelContext: FullModel description node not found.",
                LogMessageSeverity.Warning
            );  

            // TODO: init FullModel self
        }
    }

    /// <summary>
    /// On model load finish firing event.
    /// </summary>
    public event EventHandler? ModelLoaded;

    private IDataProvider? _provider;
    private RdfSerializerBase? _serializer;

    private PlainLogView _Log;
}
