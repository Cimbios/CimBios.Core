using System.Reflection;
using CimBios.Core.CimModel.Schema;
using CimBios.Utils.ClassTraits;

namespace CimBios.Core.CimModel.CimDatatypeLib;

/// <summary>
/// Structure interface for datatype lib.
/// </summary>
public interface ICimDatatypeLib : ICanLog
{
    /// <summary>
    /// Dictionary Uri to Type of IModelObject concrete classes.
    /// </summary>
    public IReadOnlyDictionary<ICimMetaClass, System.Type> RegisteredTypes { get; }

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
    /// Create instance of schema meta class.
    /// </summary>
    /// <param name="modelObjectFactory">Model object factory.</param>
    /// <param name="oid">OID of creating instance.</param>
    /// <param name="metaClass">Cim schema meta class.</param>
    /// <param name="isAuto">Is auto class attribute.</param>
    /// <returns>IModelObject instance of meta type.</returns>
    public IModelObject? CreateInstance(IModelObjectFactory modelObjectFactory,
        string oid, ICimMetaClass metaClass, bool isAuto);

    /// <summary>
    /// Create instance of schema meta class.
    /// </summary>
    /// <typeparam name="T">IModelObject CIM lib type.</typeparam>
    /// <param name="oid">OID of creating instance.</param>
    /// <param name="metaClass">Cim schema meta class.</param>
    /// <param name="isAuto">Is auto class attribute.</param>
    /// <returns>IModelObject instance of meta type.</returns>
    public T? CreateInstance<T>(string oid, bool isAuto) 
        where T : class, IModelObject;
}