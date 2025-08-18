using System.ComponentModel;
using System.Reflection;
using CimBios.Core.CimModel.CimDatatypeLib.OID;
using CimBios.Core.CimModel.Schema;
using CimBios.Utils.ClassTraits.CanLog;

namespace CimBios.Core.CimModel.CimDatatypeLib;

/// <summary>
///     Concrete model objects types library class.
/// </summary>
public class CimDatatypeLib : ICimDatatypeLib
{
    private readonly HashSet<Assembly> _LoadedAssemblies = [];

    private readonly PlainLogView _Log;

    private readonly Dictionary<ICimMetaClass, Type> _RegisteredTypes = [];

    private readonly ICimSchema _Schema;

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

    /// <summary>
    ///     Runtime attached typelib assemblies.
    /// </summary>
    public ICollection<Assembly> LoadedAssemblies => _LoadedAssemblies;

    public IReadOnlyDictionary<ICimMetaClass, Type> RegisteredTypes
        => _RegisteredTypes.AsReadOnly();

    public ILogView Log => _Log.AsReadOnly();

    public void LoadAssembly(string typesAssemblyPath, bool reset = true)
    {
        var assembly = Assembly.Load(typesAssemblyPath);
        LoadAssembly(assembly, reset);
    }

    public void LoadAssembly(Assembly typesAssembly, bool reset = true)
    {
        if (_Log.DebugLogMode) _Log.Info($"Loading types assembly {typesAssembly.FullName}");

        if (reset)
        {
            _LoadedAssemblies.Clear();
            _RegisteredTypes.Clear();
        }

        _LoadedAssemblies.Add(typesAssembly);

        var cimTypes = typesAssembly.GetTypes()
            .Where(t => t.IsDefined(typeof(CimClassAttribute), true));

        foreach (var type in cimTypes) RegisterType(type);
    }

    public void RegisterType(Type type)
    {
        if (_Log.DebugLogMode) _Log.Info($"Register type {type.FullName}");

        var attribute = type.GetCustomAttribute<CimClassAttribute>();
        if (attribute == null)
        {
            _Log.Warn($"Type {type.FullName} does not have CimClass attribute!");

            return;
        }

        var typeUri = new Uri(attribute.AbsoluteUri);
        var metaType = _Schema.TryGetResource<ICimMetaClass>(typeUri);

        // Not registered in schema.
        if (metaType == null) return;

        if (type.IsEnum)
        {
            _RegisteredTypes.Add(metaType, type);
            return;
        }

        var iface = type.GetInterface(nameof(IModelObject));
        if (iface == null)
        {
            _Log.Warn($"Type {type.FullName} does not implement IModelObject interface!");

            return;
        }

        if (!_RegisteredTypes.TryAdd(metaType, type)) _RegisteredTypes[metaType] = type;
    }

    public IModelObject? CreateInstance(IModelObjectFactory modelObjectFactory,
        IOIDDescriptor oid, ICimMetaClass metaClass)
    {
        if (_Schema.CanCreateClass(metaClass) == false)
            throw new NotSupportedException(
                $"Class {metaClass.ShortName} cannot be created!");

        var isRegisteredType = RegisteredTypes
            .TryGetValue(metaClass, out var type);

        IModelObject? instance = null;
        if (isRegisteredType && type!.IsAssignableTo(modelObjectFactory.ProduceType))
            instance = Activator.CreateInstance(type, oid, metaClass) as IModelObject;

        instance ??= modelObjectFactory.Create(oid, metaClass);

        if (instance is DynamicModelObjectBase dynamicModelObject) dynamicModelObject.InternalTypeLib = this;

        return instance;
    }

    public T? CreateInstance<T>(IOIDDescriptor oid)
        where T : class, IModelObject
    {
        var metaClass = TypedToMetaClass<T>();
        var type = RegisteredTypes[metaClass];

        if (_Schema.CanCreateClass(metaClass) == false)
            throw new NotSupportedException(
                $"Class {metaClass.ShortName} cannot be created!");

        var instance = Activator.CreateInstance(type, oid, metaClass) as T;
        if (instance is DynamicModelObjectBase dynamicModelObject) dynamicModelObject.InternalTypeLib = this;

        return instance;
    }

    public EnumValueObject? CreateEnumValueInstance(
        ICimMetaIndividual metaIndividual)
    {
        if (metaIndividual.InstanceOf == null)
            throw new InvalidEnumArgumentException(
                $"Invalid meta enum value {metaIndividual.ShortName}!");

        if (RegisteredTypes.TryGetValue(metaIndividual.InstanceOf,
                out var enumType))
        {
            var constructType = typeof(EnumValueObject<>)
                .MakeGenericType(enumType);

            var enumValueInstance = Activator.CreateInstance(constructType,
                BindingFlags.NonPublic | BindingFlags.Instance,
                null, [metaIndividual], null);

            return enumValueInstance as EnumValueObject;
        }

        if (_Schema.Individuals.Contains(metaIndividual)) return new EnumValueObject(metaIndividual);

        throw new NotSupportedException(
            $"Enum value {metaIndividual.ShortName} is not registered!");
    }

    public EnumValueObject<TEnum>? CreateEnumValueInstance<TEnum>(
        TEnum enumValue) where TEnum : struct, Enum
    {
        var metaClass = TypedToMetaClass<TEnum>();
        var metaIndividual = metaClass
            .AllIndividuals.FirstOrDefault(i => i.ShortName == enumValue.ToString());

        if (metaIndividual != null) return new EnumValueObject<TEnum>(metaIndividual);

        throw new NotSupportedException(
            $"Enum value {enumValue} does not align typelib schema!");
    }

    public IModelObject? CreateCompoundInstance(
        IModelObjectFactory modelObjectFactory, ICimMetaClass metaClass)
    {
        if (metaClass.IsCompound == false)
            throw new NotSupportedException(
                $"Meta class {metaClass.ShortName} is not compound!");

        return CreateInstance(modelObjectFactory,
            new AutoDescriptor(), metaClass);
    }

    public T? CreateCompoundInstance<T>() where T : class, IModelObject
    {
        var metaClass = TypedToMetaClass<T>();

        if (metaClass.IsCompound == false)
            throw new NotSupportedException(
                $"Meta class {metaClass.ShortName} is not compound!");

        return CreateInstance<T>(new AutoDescriptor());
    }

    private ICimMetaClass TypedToMetaClass<T>()
    {
        var metaClass = RegisteredTypes.Keys
            .Where(c => RegisteredTypes[c] == typeof(T))
            .FirstOrDefault();

        if (metaClass == null)
            throw new NotSupportedException(
                $"Meta class of type {typeof(T).Name} is not registered!");

        return metaClass;
    }
}

/// <summary>
///     Attribute for mark CIM concrete class type.
/// </summary>
public class CimClassAttribute : Attribute
{
    public CimClassAttribute(string absoluteUri)
    {
        AbsoluteUri = absoluteUri;
    }

    public string AbsoluteUri { get; set; }
}