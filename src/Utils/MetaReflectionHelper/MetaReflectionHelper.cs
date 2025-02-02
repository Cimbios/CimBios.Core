using System.Collections;
using System.Reflection;

namespace CimBios.Utils.MetaReflectionHelper;

/// <summary>
/// Collects serializable types, fields and provide read/write operations.
/// </summary>
public class MetaReflectionHelper
{
    /// <summary>
    /// Collected types dictionary within MetaTypeAttribute mark.
    /// </summary>
    public IReadOnlyDictionary<string, TypeInfo> DefinedTypes 
        => _Types.AsReadOnly();

    /// <summary>
    /// Collected members dictionary within MetaTypeAttribute mark.
    /// </summary>
    public IReadOnlyDictionary<string, MemberInfo> DefinedMembers 
        => _Members.AsReadOnly();

    public MetaReflectionHelper()
    {
        var assembly = Assembly.GetExecutingAssembly();
        CollectSerializableTypes(assembly);
    }

    public MetaReflectionHelper(Assembly assembly)
    {
        CollectSerializableTypes(assembly);
    }

    /// <summary>
    /// Get serializable type info by URI.
    /// </summary>
    /// <param name="uri">Identifier of type.</param>
    /// <param name="typeInfo">Out TypeInfo instance.</param>
    /// <returns>True if getting object succeed.</returns>
    public bool TryGetTypeInfo(string uri, out TypeInfo? typeInfo)
    {
        return _Types.TryGetValue(uri, out typeInfo);
    }

    /// <summary>
    /// Get serializable member info by URI.
    /// </summary>
    /// <param name="uri">Identifier of member.</param>
    /// <param name="memberInfo">Out MemberInfo instance.</param>
    /// <returns>True if getting object succeed.</returns>
    public bool TryGetMemberInfo(string uri, out MemberInfo? memberInfo)
    {
        return _Members.TryGetValue(uri, out memberInfo);
    }

    /// <summary>
    /// Collects serializable types.
    /// </summary>
    private void CollectSerializableTypes(Assembly assembly)
    {
        _Types.Clear();
        _Members.Clear();

        foreach (var type in assembly.GetTypes())
        {
            var typeInfo = type.GetTypeInfo();
            if (typeInfo.IsDefined(typeof(MetaTypeAttribute), 
                true) == false)
            {
                continue;
            }

            var attribute = typeInfo
                .GetCustomAttribute<MetaTypeAttribute>(false);
            if (attribute != null)
            {
                _Types.TryAdd(attribute.Identifier, type.GetTypeInfo());

                CollectSerializableMembers(type.GetTypeInfo());
            }
        }
    }

    /// <summary>
    /// Collects serializable types members.
    /// </summary>
    private void CollectSerializableMembers(TypeInfo typeInfo)
    {
        foreach (var property in typeInfo.GetMembers())
        {
            if (property.IsDefined(typeof(MetaTypeAttribute), 
                false) == false)
            {
                continue;
            }

            var attribute = property
                .GetCustomAttribute<MetaTypeAttribute>(false);
            if (attribute != null)
            {
                _Members.TryAdd(attribute.Identifier, property);
            }
        }
    }

    /// <summary>
    /// Set serizalizable member value.
    /// </summary>
    /// <param name="descriptionClass">RDF description based instance class.</param>
    /// <param name="member">Serializable member.</param>
    /// <param name="value">Value for set to member.</param>
    public void SetMetaMemberValue<T>(object descriptionClass,
        MemberInfo member, T value)
    {
        if (member is PropertyInfo propertyInfo)
        {
            if (typeof(IEnumerable).IsAssignableFrom(propertyInfo.PropertyType)
                && typeof(T) != typeof(string))
            {
                var propValue = propertyInfo.GetValue(descriptionClass);
                var addMethod = propertyInfo.PropertyType.GetMethod("Add");
                addMethod!.Invoke(propValue, [value]);
            }
            else
            {
                propertyInfo.SetValue(descriptionClass, value);
            }
        }
    }

    /// <summary>
    /// Collected serializable types.
    /// </summary>
    private Dictionary<string, TypeInfo> _Types { get; } = [];

    /// <summary>
    /// Collected serializable members.
    /// </summary>
    private Dictionary<string, MemberInfo> _Members { get; } = [];

}