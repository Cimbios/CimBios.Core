using CimBios.Core.CimModel.Schema;
using CimBios.Utils.ClassTraits;
using System.Reflection;

namespace CimBios.Core.CimModel.CimDatatypeLib;

/// <summary>
/// Concrete model objects types library class.
/// </summary>
public class CimDatatypeLib : ICimDatatypeLib
{
    /// <summary>
    /// Runtime attached typelib assemblies.
    /// </summary>
    public ICollection<Assembly> LoadedAssemblies => _LoadedAssemblies;
    public IReadOnlyDictionary<ICimMetaClass, System.Type> RegisteredTypes 
        => _RegisteredTypes.AsReadOnly();

    public ILogView Log => _Log;

    public CimDatatypeLib(ICimSchema cimSchema)
    {
        _Log = new PlainLogView(this);

        _Schema = cimSchema;

        LoadAssembly(Assembly.GetExecutingAssembly());
    }

    public CimDatatypeLib(string typesAssemblyPath, ICimSchema cimSchema) 
        : this(cimSchema)
    {
        LoadAssembly(typesAssemblyPath);
    }

    public void LoadAssembly(string typesAssemblyPath, bool reset = true)
    {
        var assembly = Assembly.Load(typesAssemblyPath);
        LoadAssembly(assembly, reset);
    }

    public void LoadAssembly(Assembly typesAssembly, bool reset = true)
    {
        if (_Log.DebugLogMode)
        {
            _Log.NewMessage(
                "DatatypeLib: Loading types assembly", 
                LogMessageSeverity.Info,
                typesAssembly.FullName ?? string.Empty               
            );
        }

        if (reset == true)
        {
            _LoadedAssemblies.Clear();
            _RegisteredTypes.Clear();
        }

        _LoadedAssemblies.Add(typesAssembly);

        var cimTypes = typesAssembly.GetTypes()
            .Where(t => t.IsDefined(typeof(CimClassAttribute), true));

        foreach (var type in cimTypes)
        {
            RegisterType(type);
        }
    }

    public void RegisterType(System.Type type)
    {
        if (_Log.DebugLogMode)
        {
            _Log.NewMessage(
                "DatatypeLib: Register type", 
                LogMessageSeverity.Info,
                type.FullName ?? string.Empty               
            );
        }

        var attribute = type.GetCustomAttribute<CimClassAttribute>();
        if (attribute == null)
        {
            _Log.NewMessage(
                "Type does not have CimClass attribute!",
                LogMessageSeverity.Warning,
                type.FullName ?? string.Empty
            );

            return;
        }

        var typeUri = new Uri(attribute.AbsoluteUri);
        var metaType = _Schema.TryGetResource<ICimMetaClass>(typeUri);

        // Not registered in schema.
        if (metaType == null)
        {
            return;
        }

        if (type.IsEnum)
        {
            _RegisteredTypes.Add(metaType, type);
            return;
        }

        var iface = type.GetInterface(nameof(IModelObject));
        if (iface == null)
        {
            _Log.NewMessage(
                "Type does not implement IModelObject interface!",
                LogMessageSeverity.Warning,
                type.FullName ?? string.Empty
            );

            return;
        }

        if (_RegisteredTypes.ContainsKey(metaType))
        {
            _RegisteredTypes[metaType] = type;
        }
        else
        {
            _RegisteredTypes.Add(metaType, type);
        }
    }

    public IModelObject? CreateInstance(IModelObjectFactory modelObjectFactory,
        string oid, ICimMetaClass metaClass, bool isAuto)
    {
        if (_Schema.CanCreateClass(metaClass) == false)
        {
            throw new NotSupportedException(
                $"Class {metaClass.ShortName} cannot be created!");
        }

        if (RegisteredTypes.TryGetValue(metaClass, out var type)
            && type.IsAssignableTo(modelObjectFactory.ProduceType))
        {
            return Activator.CreateInstance(type, oid, 
                metaClass, isAuto) as IModelObject;
        }
        else
        {
            return modelObjectFactory.Create(oid, metaClass, isAuto);
        }
    }

    public T? CreateInstance<T>(string oid, bool isAuto) 
        where T : class, IModelObject
    {   
        var metaClassTypePair = RegisteredTypes
            .Where(p => p.Value == typeof(T)).Single();

        if (_Schema.CanCreateClass(metaClassTypePair.Key) == false)
        {
            throw new NotSupportedException(
                $"Class {metaClassTypePair.Key.ShortName} cannot be created!");
        }

        return Activator.CreateInstance(metaClassTypePair.Value, oid, 
            metaClassTypePair.Key, isAuto) as T;
    }

    private ICimSchema _Schema;

    private HashSet<Assembly> _LoadedAssemblies = [];

    private Dictionary<ICimMetaClass, System.Type> _RegisteredTypes = [];

    private PlainLogView _Log;
}


/// <summary>
/// Attribute for mark CIM concrete class type.
/// </summary>
public class CimClassAttribute : Attribute
{
    public string AbsoluteUri { get; set; }

    public CimClassAttribute(string absoluteUri)
    {
        AbsoluteUri = absoluteUri;
    }
}

