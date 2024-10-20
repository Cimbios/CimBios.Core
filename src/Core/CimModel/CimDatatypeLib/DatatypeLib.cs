using CimBios.Core.RdfXmlIOLib;
using CimBios.Utils.ClassTraits;
using System.Reflection;

namespace CimBios.Core.CimModel.CimDatatypeLib;

/// <summary>
/// Structure interface for datatype lib.
/// </summary>
public interface IDatatypeLib : ICanLog
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
}

/// <summary>
/// Concrete model objects types library class.
/// </summary>
public class DatatypeLib : IDatatypeLib
{
    /// <summary>
    /// Runtime attached typelib assemblies.
    /// </summary>
    public HashSet<Assembly> LoadedAssemblies => _LoadedAssemblies;
    public Dictionary<Uri, System.Type> RegisteredTypes => _RegisteredTypes;

    public ILogView Log => _Log;

    public DatatypeLib()
    {
        _Log = new PlainLogView(this);
        if (_Log.DebugLogMode)
        {
            _Log.NewMessage(
                "DatatypeLib: Log view initialized", 
                LogMessageSeverity.Info             
            );
        }

        LoadAssembly(Assembly.GetExecutingAssembly());
    }

    public DatatypeLib(string typesAssemblyPath) : this()
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

        _RegisteredTypes.Add(new Uri(attribute.AbsoluteUri), type);
    }

    private HashSet<Assembly> _LoadedAssemblies
        = new HashSet<Assembly>();

    private Dictionary<Uri, System.Type> _RegisteredTypes
        = new Dictionary<Uri, Type>(new RdfUriComparer());

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

