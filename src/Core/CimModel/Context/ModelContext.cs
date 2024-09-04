using System.ComponentModel;
using System.Data;
using CimBios.Core.CimModel.CimDatatypeLib;
using CimBios.Core.CimModel.RdfSerializer;
using CimBios.Core.DataProvider;

namespace CimBios.Core.CimModel.Context;

/// <summary>
/// Instance of CIM model in Rdf/XML format.
/// Supports input and output operations for CIM objects.
/// </summary>
public class ModelContext
{
    public IFullModel? Description { get; set; }

    public ContextSettings Settings { get; set; } = new ContextSettings();

    private Dictionary<string, IModelObject> _Objects { get; set; }

    public ModelContext()
    {
        _Objects = new Dictionary<string, IModelObject>();
    }

    public ModelContext(IModelContextConfig contextDataConfig) : this()
    {
        InitContextDataConfig(contextDataConfig);
    }

    public void Load()
    {
        if (_provider == null || _serializer == null)
        {
            return;
        }

        _serializer.Settings.AllowUnkownClassProperties = false;
        var serialized = _serializer.Deserialize();

        _Objects = new Dictionary<string, IModelObject>(serialized
            .Select(x => new KeyValuePair<string, IModelObject>(x.Uuid, x)));

        var fullModel = _Objects.Values.OfType<IFullModel>()
            .FirstOrDefault();

        if (fullModel != null)
        {
            Description = fullModel;
            _Objects.Remove(fullModel.Uuid);
        }
    }

    public void Load(IModelContextConfig contextDataConfig)
    {
        InitContextDataConfig(contextDataConfig);

        Load();
    }

    public IEnumerable<IModelObject> GetAllObjects()
    {
        return _Objects.ToList().Select(kvp => kvp.Value);
    }

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

    public T? GetObject<T>(string uuid) where T : IModelObject
    {
        IModelObject? modelObject = GetObject(uuid);
        if (modelObject != null && modelObject is T typedObject)
        {
            return typedObject;
        }

        return default;
    }

    private void Notify_ModelObjectPropertyChanged(object? sender, 
        PropertyChangedEventArgs e)
    {
    //    throw new NotImplementedException();
    }

    private void InitContextDataConfig(IModelContextConfig contextDataConfig)
    {
        _provider = contextDataConfig.DataProvider;
        _serializer = contextDataConfig.Serializer;
        _serializer.TypeLib = contextDataConfig.TypeLib;
        _serializer.Schema = contextDataConfig.CimSchema;
    }

    private IDataProvider? _provider;
    private RdfSerializerBase? _serializer;
}

public class ContextSettings
{
    public bool AllowUnkownClassTypes { get; set; } = true;
    public bool AllowUnkownClassProperties { get; set; } = true;
    public bool AllowUriPathMismatches { get; set; } = true;
}
