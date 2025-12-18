/*
*    CimBios.Core - Common Information Model (IEC61970) I/O Library
*    Copyright (C) 2025 Yuri A. Kovalenko a.k.a belizahrt <belizahrt@gmail.com>
*
*    This program is free software: you can redistribute it and/or modify
*    it under the terms of the GNU General Public License as published by
*    the Free Software Foundation, either version 3 of the License, or
*    (at your option) any later version.
*
*    This program is distributed in the hope that it will be useful,
*    but WITHOUT ANY WARRANTY; without even the implied warranty of
*    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
*    GNU General Public License for more details.
*
*    You should have received a copy of the GNU General Public License
*    along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/

using System.ComponentModel;
using System.Reflection;
using CimBios.Core.CimModel.DatatypeLib.ModelObject;
using CimBios.Core.CimModel.DatatypeLib.OID;
using CimBios.Core.CimModel.Schema;
using Serilog;

namespace CimBios.Core.CimModel.DatatypeLib;

/// <summary>
///     Concrete model objects types library class.
/// </summary>
public class CimDatatypeLib : ICimDatatypeLib
{
    public ICimSchema Schema { get; }

    private readonly HashSet<Assembly> _LoadedAssemblies = [];

    private readonly Dictionary<ICimMetaClass, Type> _RegisteredTypes = [];

    protected ILogger? Logger { get; }

    public CimDatatypeLib(ICimSchema cimSchema, ILogger? logger = null)
    {
        Schema = cimSchema;
        Logger = logger;

        LoadAssembly(Assembly.GetExecutingAssembly());
    }

    public CimDatatypeLib(string typesAssemblyPath,
        ICimSchema cimSchema, ILogger? logger=null)
        : this(cimSchema, logger)
    {
        LoadAssembly(typesAssemblyPath);
    }

    /// <summary>
    ///     Runtime attached typelib assemblies.
    /// </summary>
    public ICollection<Assembly> LoadedAssemblies => _LoadedAssemblies;

    public IReadOnlyDictionary<ICimMetaClass, Type> RegisteredTypes
        => _RegisteredTypes.AsReadOnly();

    public void LoadAssembly(string typesAssemblyPath, bool reset = true)
    {
        var assembly = Assembly.Load(typesAssemblyPath);
        LoadAssembly(assembly, reset);
    }

    public void LoadAssembly(Assembly typesAssembly, bool reset = true)
    {
        Logger?.ForContext<CimDatatypeLib>()
            .Debug("Loading types assembly {name}", typesAssembly.FullName);

        if (reset)
        {
            _LoadedAssemblies.Clear();
            _RegisteredTypes.Clear();
            
            // Restore default core types
            LoadAssembly(Assembly.GetExecutingAssembly(), reset: false);
        }

        _LoadedAssemblies.Add(typesAssembly);

        var cimTypes = typesAssembly.GetTypes()
            .Where(t => t.IsDefined(typeof(CimClassAttribute), true));

        foreach (var type in cimTypes) RegisterType(type);
    }

    public void RegisterType(Type type)
    {
        Logger?.ForContext<CimDatatypeLib>()
            .Debug("Register type {name}", type.FullName);

        var attribute = type.GetCustomAttribute<CimClassAttribute>();
        if (attribute == null)
        {
            Logger?.ForContext<CimDatatypeLib>()
                .Warning("Type {name} does not have CimClass attribute", type.FullName);
            return;
        }

        var typeUri = new Uri(attribute.AbsoluteUri);
        var metaType = Schema.TryGetResource<ICimMetaClass>(typeUri);

        // Not registered in schema.
        if (metaType == null)
        {
            Logger?.ForContext<CimDatatypeLib>()
                .Debug("Schema entity {type} skipped: type not registered", typeUri);

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
            Logger?.ForContext<CimDatatypeLib>()
                .Warning("Type {name} does not implement IModelObject interface",
                    type.FullName);

            return;
        }

        if (!_RegisteredTypes.TryAdd(metaType, type)) _RegisteredTypes[metaType] = type;
    }

    public IModelObject? CreateInstance(IModelObjectFactory modelObjectFactory,
        IOIDDescriptor oid, ICimMetaClass metaClass)
    {
        if (Schema.CanCreateClass(metaClass) == false)
            throw new NotSupportedException(
                $"Class {metaClass.ShortName} cannot be created!");

        var isRegisteredType = RegisteredTypes
            .TryGetValue(metaClass, out var type);

        IModelObject? instance = null;
        if (isRegisteredType && type!.IsAssignableTo(modelObjectFactory.ProduceType))
            instance = Activator.CreateInstance(type, oid, metaClass) as IModelObject;

        instance ??= modelObjectFactory.Create(oid, metaClass);

        if (instance is DynamicModelObjectBase dynamicModelObject)
            dynamicModelObject.InternalTypeLib = this;

        return instance;
    }

    public T? CreateInstance<T>(IOIDDescriptor oid)
        where T : class, IModelObject
    {
        var metaClass = TypedToMetaClass<T>();
        var type = RegisteredTypes[metaClass];

        if (Schema.CanCreateClass(metaClass) == false)
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

        if (Schema.Individuals.Contains(metaIndividual)) return new EnumValueObject(metaIndividual);

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
            .FirstOrDefault(c => RegisteredTypes[c] == typeof(T));

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