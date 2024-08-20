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

    public ModelContext(IModelContextDataFactory contextDataFactory) : this()
    {
        _provider = contextDataFactory.DataProvider;
        _serializer = contextDataFactory.Serializer;
    }

    public void Load()
    {
        if (_provider == null || _serializer == null)
        {
            return;
        }

        var serialized = _serializer.Deserialize(new RdfSerializerSettings());

        _Objects = new Dictionary<string, IModelObject>(serialized
            .Select(x => new KeyValuePair<string, IModelObject>(x.Uuid, x)));
    }

    public void Load(IModelContextDataFactory contextDataFactory)
    {
        _provider = contextDataFactory.DataProvider;
        _serializer = contextDataFactory.Serializer;

        Load();
    }

    public IEnumerable<IModelObject> GetAllObjects()
    {
        return _Objects.ToList().Select(kvp => kvp.Value);
    }

    public IModelObject? GetObject(string uuid)
    {
        if (_Objects.TryGetValue(uuid, out var instance))
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

    private void Notify_ModelObjectPropertyChanged(object? sender, 
        PropertyChangedEventArgs e)
    {
    //    throw new NotImplementedException();
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

public interface IFullModel : IModelObject
{
    public string Created { get; set; }
    public string Version { get; set; }
}

public class FullModel : IFullModel
{
    public string Uuid { get => ObjectData.Uuid; }
    public string Created 
    { 
        get => ObjectData.GetAttribute<string>("Model.created"); 
        set => ObjectData.SetAttribute("Model.created", value); 
    }
    public string Version
    { 
        get => ObjectData.GetAttribute<string>("Model.version"); 
        set => ObjectData.SetAttribute("Model.version", value);
    }

    public IDataFacade ObjectData { get; }

    public FullModel(DataFacade objectData)
    {
        ObjectData = objectData;
    }
}

