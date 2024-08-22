using System.Reflection;
using System.Xml.Linq;
using CimBios.Core.RdfXmlIOLib;

namespace CimBios.Core.CimModel.Schema.RdfSchema;

public class CimRdfSchemaSerializer : ICimSchemaSerializer
{
    public Dictionary <string, XNamespace> Namespaces 
    { get => _Namespaces; }

    public void Load(TextReader reader)
    {
        _Namespaces.Clear();
        _ObjectsCache.Clear();

        _Reader.Load(reader);

        _Namespaces = _Reader.Namespaces;
    }

    public Dictionary<Uri, ICimSchemaSerialiable> Deserialize()
    {
        BuildInternalDatatypes();

        var descriptionTypedNodes = _Reader.ReadAll().ToArray();

        ReadObjects(descriptionTypedNodes);
        ResolveReferences(descriptionTypedNodes);

        return _ObjectsCache;
    }

    /// <summary>
    /// Serialization read of rdf description nodes.
    /// <param name="descriptionTypedNodes">RdfNode object.</param>
    /// </summary>
    private void ReadObjects(IEnumerable<RdfNode> descriptionTypedNodes)
    {
        var unknownTypeNodes = new List<RdfNode>();

        foreach (var node in descriptionTypedNodes)
        {
            if (_ObjectsCache.ContainsKey(node.Identifier))
            {
                continue;
            }

            var type = node.Triples.Where(t => RdfXmlReaderUtils
                    .RdfUriEquals(t.Predicate, CimRdfSchemaStrings.RdfType))
                .Single().Object as Uri;

            if (type == null)
            {
                continue;
            }

            if (_SerializeHelper.TryGetTypeInfo(type.AbsoluteUri, 
                    out var typeInfo)
                && typeInfo != null)
            {
                if (Activator.CreateInstance(typeInfo) 
                    is CimRdfDescriptionBase instance)
                {
                    instance.BaseUri = node.Identifier;
                    _ObjectsCache.Add(node.Identifier, instance);
                }
            }
            else
            {
                unknownTypeNodes.Add(node);
            }
        }

        CreateIndividuals(unknownTypeNodes);
    }

    /// <summary>
    /// Create schema individuals.
    /// <param name="nodes">List of description RDF nodes.</param>
    /// </summary>
    private void CreateIndividuals(IEnumerable<RdfNode> nodes)
    {
        foreach (var node in nodes)
        {
            var type = node.Triples.Where(t => RdfXmlReaderUtils
                .RdfUriEquals(t.Predicate, CimRdfSchemaStrings.RdfType))
            .Single().Object as Uri;

            if (type == null)
            {
                continue;
            }

            if ((_ObjectsCache.TryGetValue(type, out var entity)
                && entity is CimRdfsClass metaClass) == false)
            {
                continue;
            }

            _ObjectsCache.Add(node.Identifier, new CimRdfsIndividual()
            {
                BaseUri = node.Identifier,
                EquivalentClass = metaClass
            });
        }
    }

    /// <summary>
    /// Fill property reference instances.
    /// <param name="descriptionTypedNodes">List of description RDF nodes.</param>
    /// </summary>
    private void ResolveReferences(IEnumerable<RdfNode> descriptionTypedNodes)
    {
        foreach (var node in descriptionTypedNodes)
        {
            if (_ObjectsCache.TryGetValue(node.Identifier, 
                    out ICimSchemaSerialiable? metaDescription) == false
                || metaDescription is CimRdfDescriptionBase == false)
            {
                continue;
            }

            foreach (var triple in node.Triples)
            {
                var result = _SerializeHelper
                    .TryGetMemberInfo(triple.Predicate.AbsoluteUri, 
                        out var memberInfo);

                if (result == false || memberInfo == null)
                {
                    continue;
                }

                var attribute = memberInfo
                    .GetCustomAttribute<CimSchemaSerializableAttribute>(true);
                var value = triple.Object;

                if (attribute == null || value == null)
                {
                    continue;
                }

                if (attribute.FieldType == MetaFieldType.ByRef 
                    && value is Uri valueRefUri)
                {
                    if (_ObjectsCache.TryGetValue(valueRefUri, 
                        out var description))
                    {
                        _SerializeHelper.SetMetaMemberValue(metaDescription, 
                            memberInfo, description);
                    }
                }
                else if (attribute.FieldType == MetaFieldType.Value 
                    && value is string valueString)
                {
                    _SerializeHelper.SetMetaMemberValue(metaDescription, 
                        memberInfo, valueString);
                }
                else if (attribute.FieldType == MetaFieldType.Enum 
                    && value is Uri valueEnumUri)
                {
                    _SerializeHelper.TryGetMemberInfo(valueEnumUri.AbsoluteUri, 
                        out var field);
                    _SerializeHelper.TryGetTypeInfo(attribute.AbsoluteUri, 
                        out var enumClass);

                    if (field != null && enumClass != null)
                    {
                        var enumValue = Enum.Parse(enumClass, field.Name);
                        _SerializeHelper.SetMetaMemberValue(metaDescription, 
                            memberInfo, enumValue);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Build internal schema datatypes.
    /// </summary>
    private void BuildInternalDatatypes()
    {
        foreach (var typeUri in XmlDatatypesMapping.UriSystemTypes.Keys)
        {
            var datatype = XmlDatatypesMapping.UriSystemTypes[typeUri];

            var metaDatatype = new CimRdfsDatatype()
            {
                BaseUri = new Uri(typeUri),
                SystemType = datatype
            };

            _ObjectsCache.Add(metaDatatype.BaseUri, metaDatatype);
        }
    }

    private CimSchemaReflectionHelper _SerializeHelper
        = new CimSchemaReflectionHelper ();

    private RdfXmlReader _Reader = new RdfXmlReader();

    private Dictionary<Uri, ICimSchemaSerialiable> _ObjectsCache 
        = new Dictionary<Uri, ICimSchemaSerialiable>(new RdfUriComparer());

    private Dictionary <string, XNamespace> _Namespaces
        = new Dictionary<string, XNamespace> ();
}

/// <summary>
/// Pre-defined schemas URIs.
/// </summary>
public static class CimRdfSchemaStrings
{
    public static Uri RdfDescription = 
        new Uri("http://www.w3.org/1999/02/22-rdf-syntax-ns#Description");
    public static Uri RdfType = 
        new Uri("http://www.w3.org/1999/02/22-rdf-syntax-ns#type");
    public static Uri RdfsClass = 
        new Uri("http://www.w3.org/2000/01/rdf-schema#Class");
    public static Uri RdfProperty = 
        new Uri("http://www.w3.org/1999/02/22-rdf-syntax-ns#Property");
}
