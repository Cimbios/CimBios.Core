using CimBios.Core.RdfXmlIOLib;
using System.Reflection;

namespace CimBios.Core.CimModel.Schema;

/// <summary>
/// Pre-defined schemas URIs.
/// </summary>
public static class CimSchemaStrings
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

/// <summary>
/// Pre-defined mapping of XML types to system types.
/// </summary>
internal static class XmlDatatypesMapping
{
    internal static readonly Dictionary<string, System.Type> UriSystemTypes
        = new Dictionary<string, System.Type>()
        {
            { "http://www.w3.org/2001/XMLSchema#string", typeof(string) },
            { "http://www.w3.org/2001/XMLSchema#boolean", typeof(bool) },
            { "http://www.w3.org/2001/XMLSchema#decimal", typeof(decimal) },
            { "http://www.w3.org/2001/XMLSchema#float", typeof(float) },
            { "http://www.w3.org/2001/XMLSchema#double", typeof(double) },
            { "http://www.w3.org/2001/XMLSchema#dateTime", typeof(DateTime) },
            { "http://www.w3.org/2001/XMLSchema#time", typeof(TimeOnly) },
            { "http://www.w3.org/2001/XMLSchema#date", typeof(DateOnly) },
            { "http://www.w3.org/2001/XMLSchema#anyURI", typeof(Uri) },
        };
}

/// <summary>
/// Cim schema interface. Defines usage structure.
/// </summary>
public interface ICimSchema
{
    /// <summary>
    /// All CimMetaClass instances - RDF description instances 
    /// of RDF type Class.
    /// </summary>
    public IEnumerable<CimMetaClass> Classes { get; }
    /// <summary>
    /// All CimMetaProperty instances - RDF description instances 
    /// of RDF type Property.
    /// </summary>
    public IEnumerable<CimMetaProperty> Properties { get; }
    /// <summary>
    /// All CimMetaIndividual instances - RDF description concrete instances.
    /// </summary>
    public IEnumerable<CimMetaIndividual> Individuals { get; }
    /// <summary>
    /// All CimMetaDatatype instances - RDF description instances 
    /// of RDF type Datatype.
    /// </summary>
    public IEnumerable<CimMetaDatatype> Datatypes { get; }

    /// <summary>
    /// Load RDFS schema content via text reader.
    /// </summary>
    public void Load(TextReader textReader);
    //public void Save(TextWriter textWriter);

    /// <summary>
    /// Get concrete serialized meta description instance.
    /// </summary>
    /// <param name="uri">Identifier of instance.</param>
    /// <returns>CimRdfDescriptionBase inherits instance.</returns>
    public T? TryGetDescription<T>(Uri uri) where T : CimRdfDescriptionBase;

    /// <summary>
    /// Get list of class properties.
    /// </summary>
    /// <param name="metaClass">Meta class instance.</param>
    /// <param name="inherit">Is needed to collect ancestors properties.</param>
    /// <returns>Enumerable of CimMetaProperty.</returns>
    public IEnumerable<CimMetaProperty> GetClassProperties(
        CimMetaClass metaClass,
        bool inherit = false);
}

/// <summary>
/// Cim schema supports RDFS format.
/// </summary>
public class CimSchema : ICimSchema
{
    public IEnumerable<CimMetaClass> Classes 
    { get => _All.Values.OfType<CimMetaClass>(); }
    public IEnumerable<CimMetaProperty> Properties 
    { get => _All.Values.OfType<CimMetaProperty>(); }
    public IEnumerable<CimMetaIndividual> Individuals 
    { get => _All.Values.OfType<CimMetaIndividual>(); }
    public IEnumerable<CimMetaDatatype> Datatypes 
    { get => _All.Values.OfType<CimMetaDatatype>(); }

    private CimSchemaReflectionHelper _SerializeHelper { get; }
    private RdfXmlReader _Reader { get; set; } = new RdfXmlReader();

    public CimSchema()
    {
        _All = new Dictionary<Uri, CimRdfDescriptionBase>(new RdfUriComparer());

        _SerializeHelper = new CimSchemaReflectionHelper();
    }

    public void Load(TextReader textReader)
    {
        _All.Clear();

        _Reader.Load(textReader);

        BuildInternalDatatypes();

        var descriptionTypedNodes = _Reader.ReadAll()
            .Where(n => RdfXmlReaderUtils.RdfUriEquals(n.TypeIdentifier,
                CimSchemaStrings.RdfDescription))
            .Where(n => n.Triples
                .Any(t => RdfXmlReaderUtils
                    .RdfUriEquals(t.Predicate, CimSchemaStrings.RdfType)
            )).ToArray();

        ReadObjects(descriptionTypedNodes);
        ResolveReferences(descriptionTypedNodes);
    }

    public IEnumerable<CimMetaProperty> GetClassProperties(
        CimMetaClass metaClass,
        bool inherit = false)
    {
        CimMetaClass? nextClass = metaClass;

        do
        {
            foreach (var prop in Properties.Where(p => p.Domain == nextClass))
            {
                yield return prop;
            }

            nextClass = nextClass?.SubClassOf;
        }
        while (inherit == true && nextClass != null);
    }

    /// <summary>
    /// Build internal schema datatypes.
    /// </summary>
    private void BuildInternalDatatypes()
    {
        foreach (var typeUri in XmlDatatypesMapping.UriSystemTypes.Keys)
        {
            var datatype = XmlDatatypesMapping.UriSystemTypes[typeUri];

            var metaDatatype = new CimMetaDatatype()
            {
                Uri = new Uri(typeUri),
                SystemType = datatype
            };

            _All.Add(metaDatatype.Uri, metaDatatype);
        }
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
            if (_All.ContainsKey(node.Identifier))
            {
                continue;
            }

            var type = node.Triples.Where(t => RdfXmlReaderUtils
                    .RdfUriEquals(t.Predicate, CimSchemaStrings.RdfType))
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
                    instance.Uri = node.Identifier;
                    _All.Add(node.Identifier, instance);
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
                .RdfUriEquals(t.Predicate, CimSchemaStrings.RdfType))
            .Single().Object as Uri;

            if (type == null)
            {
                continue;
            }

            var metaClass = TryGetDescription<CimMetaClass>(type);
            if (metaClass == null)
            {
                continue;
            }

            _All.Add(node.Identifier, new CimMetaIndividual()
            {
                Uri = node.Identifier,
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
            if (_All.TryGetValue(node.Identifier, 
                    out CimRdfDescriptionBase? metaDescription) == false
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
                    if (_All.TryGetValue(valueRefUri, out var description))
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

    public T? TryGetDescription<T>(Uri uri) where T : CimRdfDescriptionBase
    {
        if (_All.TryGetValue(uri, out var metaDescription)
            && metaDescription is T meta)
        {
            return meta;
        }

        return null;
    }

    private readonly Dictionary<Uri, CimRdfDescriptionBase> _All;
}
