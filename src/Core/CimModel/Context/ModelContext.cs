using System.Collections.Immutable;
using System.ComponentModel;
using System.Data;
using CimBios.Core.CimModel.CimDatatypeLib;
using CimBios.Core.CimModel.RdfSerializer;
using CimBios.Core.CimModel.Schema;
using CimBios.Core.DataProvider;

namespace CimBios.Core.CimModel.Context;

/// <summary>
/// Instance of CIM model in Rdf/* format.
/// Supports input and output operations for CIM objects.
/// </summary>
public class ModelContext
{
    /// <summary>
    /// Model description.
    /// </summary>
    public IFullModel? Description { get; set; }

    /// <summary>
    /// Applied schema to this context serializer.
    /// </summary>
    public ICimSchema? Schema { get => _serializer?.Schema; }

    /// <summary>
    /// All cached objects collection (uuid to IModelObject).
    /// </summary>
    private Dictionary<string, IModelObject> _Objects { get; set; }

    public ModelContext()
    {
        _Objects = new Dictionary<string, IModelObject>();
    }

    public ModelContext(IModelDataContext contextDataConfig) : this()
    {
        InitContextDataConfig(contextDataConfig);
    }

    /// <summary>
    /// Load CIM model to context via config.
    /// </summary>
    public void Load()
    {
        if (_provider == null || _serializer == null)
        {
            return;
        }

        var serialized = _serializer.Deserialize();

        _Objects = new Dictionary<string, IModelObject>(serialized
            .Select(x => new KeyValuePair<string, IModelObject>(x.Uuid, x)));

        ReadModelDescription();

        ModelLoaded?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Load CIM model to context via config.
    /// </summary>
    /// <param name="contextDataConfig">Context configuration.</param>
    public void Load(IModelDataContext contextDataConfig)
    {
        InitContextDataConfig(contextDataConfig);

        Load();
    }

    /// <summary>
    /// Save CIM model to context via config.
    /// </summary>
    public void Save()
    {
        if (_provider == null || _serializer == null)
        {
            return;
        }   

        var forSerializeObjects = _Objects.Values.ToImmutableList();
        if (Description != null)
        {
            forSerializeObjects.Add(Description);
        }
        _serializer.Serialize(forSerializeObjects);
    }

    /// <summary>
    /// Save CIM model to context via config.
    /// </summary>
    /// <param name="contextDataConfig">Context configuration.</param>
    public void Save(IModelDataContext contextDataConfig)
    {
        InitContextDataConfig(contextDataConfig);

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
            && !instance.ObjectData.IsAuto
            && !instance.ObjectData.IsCompound)
        {
            instance.ObjectData.PropertyChanged 
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
    /// <param name="contextDataConfig">Context configuration.</param>
    private void InitContextDataConfig(IModelDataContext contextDataConfig)
    {
        _provider = contextDataConfig.DataProvider;
        _serializer = contextDataConfig.Serializer;
        _serializer.TypeLib = contextDataConfig.TypeLib;
        _serializer.Schema = contextDataConfig.CimSchema;
    }

    /// <summary>
    /// Find and extract IFullModel description from _objects cache.
    /// </summary>
    private void ReadModelDescription()
    {
        var fullModel = _Objects.Values.OfType<IFullModel>()
            .FirstOrDefault();

        if (fullModel != null)
        {
            Description = fullModel;
            _Objects.Remove(fullModel.Uuid);
        }
    }

    /// <summary>
    /// On model load finish firing event.
    /// </summary>
    public event EventHandler? ModelLoaded;

    private IDataProvider? _provider;
    private RdfSerializerBase? _serializer;
}
