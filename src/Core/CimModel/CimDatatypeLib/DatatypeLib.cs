using CimBios.Core.RdfXmlIOLib;
using System.Reflection;

namespace CimBios.Core.CimModel.CimDatatypeLib
{
    /// <summary>
    /// Structure interface for datatype lib.
    /// </summary>
    public interface IDatatypeLib
    {
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
        /// Dictionary Uri to Type of IModelObject concrete classes.
        /// </summary>
        public Dictionary<Uri, System.Type> RegisteredTypes { get; }
    }

    /// <summary>
    /// Concrete model objects types library class.
    /// </summary>
    public class DatatypeLib : IDatatypeLib
    {
        public DatatypeLib()
        {
            LoadAssembly(Assembly.GetExecutingAssembly());
        }

        public DatatypeLib(string typesAssemblyPath)
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
            var iface = type.GetInterface(nameof(IModelObject));

            if (iface == null)
            {
                throw new ArgumentException("Type does not implement IModelObject interface!");
            }

            var attribute = type.GetCustomAttribute<CimClassAttribute>();

            if (attribute == null)
            {
                throw new ArgumentException("Type does not have CimClass attribute!");
            }

            _RegisteredTypes.Add(new Uri(attribute.AbsoluteUri), type);
        }

        /// <summary>
        /// Runtime attached typelib assemblies.
        /// </summary>
        public HashSet<Assembly> LoadedAssemblies { get => _LoadedAssemblies; }

        public Dictionary<Uri, System.Type> RegisteredTypes { get => _RegisteredTypes; }

        private HashSet<Assembly> _LoadedAssemblies
            = new HashSet<Assembly>();

        private Dictionary<Uri, System.Type> _RegisteredTypes
            = new Dictionary<Uri, Type>(new RdfUriComparer());
    }

    /// <summary>
    /// Attribute for mark CIM concrete class type.
    /// </summary>
    public class CimClassAttribute : Attribute
    {
        public CimClassAttribute(string absoluteUri)
        {
            AbsoluteUri = absoluteUri;
        }

        public string AbsoluteUri { get; set; }
    }

}
