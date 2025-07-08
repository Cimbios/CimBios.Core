using System.Reflection;
using CimBios.Core.CimModel.CimDatatypeLib.OID;
using CimBios.Core.CimModel.Schema;
using CimBios.Utils.ClassTraits.CanLog;

namespace CimBios.Core.CimModel.CimDatatypeLib;

/// <summary>
///     Structure interface for datatype lib.
/// </summary>
public interface ICimDatatypeLib : ICanLog
{
    /// <summary>
    ///     Dictionary Uri to Type of IModelObject concrete classes.
    /// </summary>
    public IReadOnlyDictionary<ICimMetaClass, Type> RegisteredTypes { get; }

    /// <summary>
    ///     Load assembly by file path.
    /// </summary>
    /// <param name="typesAssemblyPath">Path to assembly .dll file.</param>
    /// <param name="reset">Clear current assemblies collection.</param>
    public void LoadAssembly(string typesAssemblyPath, bool reset = true);

    /// <summary>
    ///     Load assembly by file path.
    /// </summary>
    /// <param name="typesAssembly">Assembly object.</param>
    /// <param name="reset">Clear current assemblies collection.</param>
    public void LoadAssembly(Assembly typesAssembly, bool reset = true);

    /// <summary>
    ///     Register new type in datatype library.
    /// </summary>
    /// <param name="type">Type for register.</param>
    public void RegisterType(Type type);

    /// <summary>
    ///     Create instance of schema meta class.
    /// </summary>
    /// <param name="modelObjectFactory">Model object factory.</param>
    /// <param name="oid">OID of creating instance.</param>
    /// <param name="metaClass">Cim schema meta class.</param>
    /// <param name="isAuto">Is auto class attribute.</param>
    /// <returns>IModelObject instance of meta type.</returns>
    public IModelObject? CreateInstance(IModelObjectFactory modelObjectFactory,
        IOIDDescriptor oid, ICimMetaClass metaClass);

    /// <summary>
    ///     Create instance of schema meta class.
    /// </summary>
    /// <typeparam name="T">IModelObject CIM lib type.</typeparam>
    /// <param name="oid">OID of creating instance.</param>
    /// <param name="metaClass">Cim schema meta class.</param>
    /// <param name="isAuto">Is auto class attribute.</param>
    /// <returns>IModelObject instance of meta type.</returns>
    public T? CreateInstance<T>(IOIDDescriptor oid)
        where T : class, IModelObject;

    /// <summary>
    ///     Create enum value instance of meta individual type.
    /// </summary>
    /// <typeparam name="TEnum">Typed enum generic value.</typeparam>
    /// <param name="metaIndividual">Meta individual instance.</param>
    /// <returns></returns>
    public EnumValueObject? CreateEnumValueInstance(
        ICimMetaIndividual metaIndividual);

    /// <summary>
    ///     Create enum value instance of meta individual type.
    /// </summary>
    /// <typeparam name="TEnum">Typed enum generic value.</typeparam>
    /// <param name="enumValue">Typed enum value instance.</param>
    /// <returns></returns>
    public EnumValueObject<TEnum>? CreateEnumValueInstance<TEnum>(
        TEnum enumValue) where TEnum : struct, Enum;

    /// <summary>
    ///     Create compound meta class instance.
    /// </summary>
    /// <param name="modelObjectFactory">Model object factory.</param>
    /// <param name="metaClass">Cim schema meta class.</param>
    /// <returns></returns>
    public IModelObject? CreateCompoundInstance(
        IModelObjectFactory modelObjectFactory, ICimMetaClass metaClass);

    /// <summary>
    ///     Create compound meta class instance of type T.
    /// </summary>
    /// <typeparam name="T">Type lib CIM type.</typeparam>
    /// <returns></returns>
    public T? CreateCompoundInstance<T>() where T : class, IModelObject;
}