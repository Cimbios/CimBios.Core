using CimBios.Core.CimModel.Schema;
using CimBios.Core.RdfIOLib;
using CimBios.Utils.ClassTraits;
using System.Reflection;

namespace CimBios.Core.CimModel.CimDatatypeLib;

/// <summary>
/// Structure interface for datatype lib.
/// </summary>
public interface ICimDatatypeLib : ICanLog
{
    /// <summary>
    /// Dictionary Uri to Type of IModelObject concrete classes.
    /// </summary>
    public Dictionary<Uri, System.Type> RegisteredTypes { get; }

    /// <summary>
    /// Load assembly by file path.
    /// </summary>
    /// <param name="typesAssemblyPath">Path to assembly .dll file.</param>
    /// <param name="reset">Clear current assemblies collection.</param>
    public void LoadAssembly(string typesAssemblyPath, bool reset = true);

    /// <summary>
    /// Load assembly by file path.
    /// </summary>
    /// <param name="typesAssembly">Assembly object.</param>
    /// <param name="reset">Clear current assemblies collection.</param>
    public void LoadAssembly(Assembly typesAssembly, bool reset = true);

    /// <summary>
    /// Register new type in datatype library.
    /// </summary>
    /// <param name="type">Type for register.</param>
    public void RegisterType(System.Type type);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="uuid"></param>
    /// <param name="metaClass"></param>
    /// <param name="isAuto"></param>
    /// <returns></returns>
    public IModelObject? CreateInstance(IModelObjectFactory modelObjectFactory,
        string uuid, ICimMetaClass metaClass, bool isAuto);
}

/// <summary>
/// Concrete model objects types library class.
/// </summary>
public class CimDatatypeLib : ICimDatatypeLib
{
    /// <summary>
    /// Runtime attached typelib assemblies.
    /// </summary>
    public HashSet<Assembly> LoadedAssemblies => _LoadedAssemblies;
    public Dictionary<Uri, System.Type> RegisteredTypes => _RegisteredTypes;

    public ILogView Log => _Log;

    public CimDatatypeLib()
    {
        _Log = new PlainLogView(this);

        LoadAssembly(Assembly.GetExecutingAssembly());
    }

    public CimDatatypeLib(string typesAssemblyPath) : this()
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

        if (type.IsEnum)
        {
            _RegisteredTypes.Add(new Uri(attribute.AbsoluteUri), type);
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

        _RegisteredTypes.Add(new Uri(attribute.AbsoluteUri), type);
    }

    public IModelObject? CreateInstance(IModelObjectFactory modelObjectFactory,
        string uuid, ICimMetaClass metaClass, bool isAuto)
    {
        if (RegisteredTypes.TryGetValue(metaClass.BaseUri, out var type)
            && type.IsAssignableTo(modelObjectFactory.ProduceType))
        {
            return Activator.CreateInstance(type, uuid, 
                metaClass, isAuto) as IModelObject;
        }
        else
        {
            return modelObjectFactory.Create(uuid, metaClass, isAuto);
        }
    }

    private HashSet<Assembly> _LoadedAssemblies = [];

    private Dictionary<Uri, System.Type> _RegisteredTypes 
        = new(new RdfUriComparer());

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

