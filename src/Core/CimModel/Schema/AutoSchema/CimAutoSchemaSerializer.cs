using CimBios.Core.RdfIOLib;

namespace CimBios.Core.CimModel.Schema.AutoSchema;

/// <summary>
/// Use it only for debugging activities! The serializer does not guarantee 
/// the consistency and completeness of the data being read and written 
/// according to the generated schema.
/// </summary>
public class CimAutoSchemaSerializer : ICimSchemaSerializer
{
    public const string BaseSchemaUri = "http://cim.bios/schemas/auto";

    public Dictionary<string, Uri> Namespaces => _Namespaces;

    public CimAutoSchemaSerializer(RdfReaderBase rdfReader)
    {
        _RdfReader = rdfReader;
    }

    public Dictionary<Uri, ICimMetaResource> Deserialize()
    {
        _Namespaces.Clear();
        _ObjectsCache.Clear();

        var objectsModel = _RdfReader.ReadAll().ToArray();
        ForwardReaderNamespaces();
        BuildInternalDatatypes();

        CreateSchemaEntitiesFromModel(objectsModel);

        _RdfReader.Close();

        return _ObjectsCache;
    }

    public void Load(TextReader reader)
    {
        _RdfReader.AddNamespace("base", new(BaseSchemaUri));
        _RdfReader.Load(reader);
    }

    /// <summary>
    /// Rdf n-triples based schema convertation method.
    /// </summary>
    /// <param name="nodes"></param>
    private void CreateSchemaEntitiesFromModel(IEnumerable<RdfNode> nodes)
    {
        foreach (var node in nodes)
        {
            if (_ObjectsCache.ContainsKey(node.TypeIdentifier) == false)
            {
                AddClass(node.TypeIdentifier, false, false);
            }

            HandleProperties(node);
        }
    }

    /// <summary>
    /// Add meta properties for schema and fill necessary class links.
    /// </summary>
    /// <param name="node">Class instance Rdf node with property triples.</param>
    private void HandleProperties(RdfNode node)
    {
        foreach (var property in node.Triples)
        {
            var classUri = MakeAncestorClassFromProperty(node.TypeIdentifier, 
                property);

            if (classUri == null)
            {
                continue;
            }

            CimAutoClass? propertyDatatype = 
                _ObjectsCache[new("http://www.w3.org/2001/XMLSchema#string")] as CimAutoClass;

            CimMetaPropertyKind propertyKind = CimMetaPropertyKind.NonStandard;
            if (property.Object is Uri uriObject)
            {
                // local link.
                if (uriObject == Namespaces["base"])
                {
                    // The most generalized kind for assoc.
                    propertyKind = CimMetaPropertyKind.Assoc1ToM;
                }
                else
                {
                    // individual a.k.a enum
                    propertyKind = CimMetaPropertyKind.Attribute;

                    var enumClass = CreateOrAugmentEnumClass(uriObject);
                    propertyDatatype = enumClass;
                }
            }
            else if (property.Object is RdfNode subRdfNode)
            {
                propertyKind = CimMetaPropertyKind.Attribute;
                var compoundCLass = AddClass(subRdfNode.TypeIdentifier, 
                    false, true);

                HandleProperties(subRdfNode);
                propertyDatatype = compoundCLass;
            }
            else
            {
                propertyKind = CimMetaPropertyKind.Attribute;
            }

            AddProperty(property.Predicate, 
                _ObjectsCache[classUri] as CimAutoClass,
                propertyKind,
                propertyDatatype);
            
        }
    }

    /// <summary>
    /// Add new class to schema.
    /// </summary>
    /// <param name="typeIdentifier">Class uri.</param>
    /// <param name="isEnum">Is class enum.</param>
    /// <param name="IsCompound">Is class compound.</param>
    /// <returns>New class instance or null if class already exists.</returns>
    private CimAutoClass? AddClass(Uri typeIdentifier, 
        bool isEnum, bool IsCompound)
    {
        if (_ObjectsCache.ContainsKey(typeIdentifier))
        {
            return null;
        }

        if (RdfUtils.TryGetEscapedIdentifier(typeIdentifier, 
            out var shortName) == false)
        {
            shortName = typeIdentifier.AbsoluteUri;
        }

        var autoClass = new CimAutoClass()
        {
            BaseUri = typeIdentifier,
            ShortName = shortName,
            Description = string.Empty,
            IsEnum = isEnum,
            IsCompound = IsCompound
        };

        _ObjectsCache.TryAdd(autoClass.BaseUri, autoClass);

        return autoClass;
    }

    /// <summary>
    /// Add new property to schema.
    /// </summary>
    /// <param name="propertyUri">Property uri.</param>
    /// <param name="ownerClass">The owner class of adding property.</param>
    /// <returns>False if property already exists.</returns>
    public bool AddProperty(Uri propertyUri, CimAutoClass? ownerClass,
        CimMetaPropertyKind propertyKind, CimAutoClass? propertyDatatype)
    {
        if (_ObjectsCache.ContainsKey(propertyUri))
        {
            return false;
        }

        if (RdfUtils.TryGetEscapedIdentifier(propertyUri, 
            out var shortName) == false)
        {
            shortName = propertyUri.AbsoluteUri;
        }
        else
        {
            shortName = shortName[(shortName.IndexOf('.') + 1) ..];
        }

        var autoProperty = new CimAutoProperty()
        {
            BaseUri = propertyUri,
            ShortName = shortName,
            Description = string.Empty,
            OwnerClass = ownerClass,
            PropertyKind = propertyKind,
            PropertyDatatype = propertyDatatype
        };

        _ObjectsCache.TryAdd(propertyUri, autoProperty);

        return true;
    }

    /// <summary>
    /// Add ancestor inheritance link to class.
    /// </summary>
    /// <param name="classUri">Source class URI.</param>
    /// <param name="ancestorUri">Ancestor class URI.</param>
    /// <returns>True if link was been created.</returns>
    private bool AddAncestorToClass(Uri childClassUri, Uri ancestorUri)
    {
        if (_ObjectsCache[childClassUri] is not CimAutoClass childClass 
            || _ObjectsCache[ancestorUri] is not CimAutoClass ancestorClass)
        {
            return false;
        }

        childClass.AddAncestor(ancestorClass);

        return true;
    }

    /// <summary>
    /// Create new domain class of property (for abstract classes). Makes 
    /// generalization (extension )link beetween model owner instance
    /// class and ancestor.
    /// </summary>
    /// <param name="childClassUri">Child domain class.</param>
    /// <param name="property">Property triple.</param>
    /// <returns>Uri of </returns>
    private Uri? MakeAncestorClassFromProperty(Uri childClassUri, 
        RdfTriple property)
    {
        if (TryGetClassUriFromProperty(property.Predicate, 
            out var ancestorClassUri) == false)
        {
            return null;
        }

        if (_ObjectsCache.ContainsKey(ancestorClassUri) == false)
        {
            AddClass(ancestorClassUri, false, false);
        }

        if (RdfUtils.RdfUriEquals(ancestorClassUri, childClassUri) == false)
        {
            AddAncestorToClass(childClassUri, ancestorClassUri);
        }

        return ancestorClassUri;
    }

    /// <summary>
    /// Create new enum class or augment already exist with new enum value.
    /// </summary>
    /// <param name="enumValueUri"></param>
    /// <returns>Enum class or null.</returns>
    private CimAutoClass? CreateOrAugmentEnumClass(Uri enumValueUri)
    {
        if (TryGetClassUriFromProperty(enumValueUri, 
            out var enumUri) == false)
        {
            return null;
        }  

        CimAutoClass? enumClass; 
        if (_ObjectsCache.TryGetValue(enumUri, out var enumResource)
            && enumResource is CimAutoClass)
        {
            enumClass = enumResource as CimAutoClass;
        }
        else
        {
            enumClass = AddClass(enumUri, true, false);
        }

        if (RdfUtils.TryGetEscapedIdentifier(enumValueUri, 
            out var shortName) == false)
        {
            shortName = enumValueUri.AbsoluteUri;
        }
        else
        {
            shortName = shortName[(shortName.IndexOf('.') + 1) ..];
        }

        var enumValue = new CimAutoIndividual()
        {
            BaseUri = enumValueUri,
            ShortName = shortName,
            Description = string.Empty,
            InstanceOf = enumClass
        };

        _ObjectsCache.TryAdd(enumValueUri, enumValue);

        return enumClass;
    }

    /// <summary>
    /// Try extract class uri from property uri. Format: "na://{Class}.{Property}"
    /// </summary>
    /// <param name="propertyUri">Property URI.</param>
    /// <param name="classUri">Result out class URI.</param>
    /// <returns>True if extraction is successfully.</returns>
    private static bool TryGetClassUriFromProperty(Uri propertyUri, 
        out Uri classUri)
    {
        classUri = propertyUri;

        if (RdfUtils.TryGetEscapedIdentifier(propertyUri, 
            out var propertyId) == false)
        {
            return false;
        }

        var namespaceUri = propertyUri.AbsoluteUri
            .Replace(propertyId, "");

        var classId = propertyId[..propertyId.IndexOf('.')];

        classUri = new(namespaceUri + classId);

        return true;
    }

    /// <summary>
    /// Move namespaces from reader doc.
    /// </summary>
    private void ForwardReaderNamespaces()
    {
        foreach (var item in _RdfReader.Namespaces)
        {
            _Namespaces.Add(item.Key, item.Value);
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

            var uri = new Uri(typeUri);
            RdfUtils.TryGetEscapedIdentifier(uri, out var label);
            var metaDatatype = new CimAutoDatatype()
            {
                BaseUri = uri,
                SystemType = datatype,
                ShortName = label,
                Description = "Build-in xsd datatype."
            };

            _ObjectsCache.Add(metaDatatype.BaseUri, metaDatatype);
        }
    }

    private readonly RdfReaderBase _RdfReader;

    private readonly Dictionary<Uri, ICimMetaResource> _ObjectsCache 
        = new(new RdfUriComparer());

    private Dictionary <string, Uri> _Namespaces = [];
}
