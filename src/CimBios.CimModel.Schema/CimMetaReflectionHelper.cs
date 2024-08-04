using System.Reflection;

namespace CimBios.CimModel.Schema
{
    internal class CimSchemaReflectionHelper
    {
        public CimSchemaReflectionHelper()
        {
            CollectSerializableTypes();
        }

        public bool TryGetTypeInfo(string uri, out TypeInfo? typeInfo)
        {
            return _Types.TryGetValue(uri, out typeInfo);
        }

        public bool TryGetMemberInfo(string uri, out MemberInfo? memberInfo)
        {
            return _Members.TryGetValue(uri, out memberInfo);
        }

        private void CollectSerializableTypes()
        {
            _Types.Clear();
            _Members.Clear();

            foreach (var type in Assembly.GetExecutingAssembly().GetTypes())
            {
                var typeInfo = type.GetTypeInfo();
                if (typeInfo.IsDefined(typeof(CimSchemaSerializableAttribute), true) == false)
                {
                    continue;
                }

                var attribute = typeInfo.GetCustomAttribute<CimSchemaSerializableAttribute>(false);
                if (attribute != null)
                {
                    _Types.TryAdd(attribute.AbsoluteUri, type.GetTypeInfo());

                    CollectSerializableMembers(type.GetTypeInfo());
                }
            }
        }

        private void CollectSerializableMembers(TypeInfo typeInfo)
        {
            foreach (var property in typeInfo.GetMembers())
            {
                if (property.IsDefined(typeof(CimSchemaSerializableAttribute), false) == false)
                {
                    continue;
                }

                var attribute = property.GetCustomAttribute<CimSchemaSerializableAttribute>(false);
                if (attribute != null)
                {
                    _Members.TryAdd(attribute.AbsoluteUri, property);
                }
            }
        }

        public void SetMetaMemberValue<T>(object descriptionClass,
            MemberInfo member, Attribute attribute, T value)
        {
            if (member is PropertyInfo propertyInfo)
            {
                if (propertyInfo.GetValue(descriptionClass) is ICollection<T> collection)
                {
                    collection.Add(value);
                }
                else
                {
                    propertyInfo.SetValue(descriptionClass, value);
                }
            }
        }

        private Dictionary<string, TypeInfo> _Types { get; }
            = new Dictionary<string, TypeInfo>();
        private Dictionary<string, MemberInfo> _Members { get; }
            = new Dictionary<string, MemberInfo>();

    }

    internal enum MetaFieldType
    {
        Value,
        Enum,
        ByRef,
        Datatype,
    }
}
