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
                _Types.TryAdd(attribute.Identifier.ToLower(), type.GetTypeInfo());

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
                _Members.TryAdd(attribute.Identifier.ToLower(), property);
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
