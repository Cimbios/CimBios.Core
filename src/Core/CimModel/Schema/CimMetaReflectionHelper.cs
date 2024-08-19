using System.Reflection;

namespace CimBios.Core.CimModel.Schema;

/// <summary>
/// Collects serializable types, fields and provide read/write operations.
/// </summary>
internal class CimSchemaReflectionHelper
{
    public CimSchemaReflectionHelper()
    {
        CollectSerializableTypes();
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
    private void CollectSerializableTypes()
    {
        _Types.Clear();
        _Members.Clear();

        foreach (var type in Assembly.GetExecutingAssembly().GetTypes())
        {
            var typeInfo = type.GetTypeInfo();
            if (typeInfo.IsDefined(typeof(CimSchemaSerializableAttribute), 
                true) == false)
            {
                continue;
            }

            var attribute = typeInfo
                .GetCustomAttribute<CimSchemaSerializableAttribute>(false);
            if (attribute != null)
            {
                _Types.TryAdd(attribute.AbsoluteUri, type.GetTypeInfo());

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
            if (property.IsDefined(typeof(CimSchemaSerializableAttribute), 
                false) == false)
            {
                continue;
            }

            var attribute = property
                .GetCustomAttribute<CimSchemaSerializableAttribute>(false);
            if (attribute != null)
            {
                _Members.TryAdd(attribute.AbsoluteUri, property);
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
            if (propertyInfo.GetValue(descriptionClass) 
                is ICollection<T> collection)
            {
                collection.Add(value);
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
    private Dictionary<string, TypeInfo> _Types { get; }
        = new Dictionary<string, TypeInfo>();

    /// <summary>
    /// Collected serializable members.
    /// </summary>
    private Dictionary<string, MemberInfo> _Members { get; }
        = new Dictionary<string, MemberInfo>();

}

/// <summary>
/// Member serialization types.
/// </summary>
internal enum MetaFieldType
{
    /// <summary>
    /// XML string node value.
    /// </summary>
    Value,
    /// <summary>
    /// Enumeration according schema individuals. 
    /// </summary>
    Enum,
    /// <summary>
    /// Schema RDF description based instance by URI reference.
    /// </summary>
    ByRef,
    /// <summary>
    /// Schema RDF datatype.
    /// </summary>
    Datatype,
}

