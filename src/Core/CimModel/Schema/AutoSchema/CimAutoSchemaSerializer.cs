using System.Collections.ObjectModel;
using CimBios.Core.RdfIOLib;

namespace CimBios.Core.CimModel.Schema.AutoSchema;

/// <summary>
/// Use it only for debugging activities! The serializer does not guarantee 
/// the consistency and completeness of the data being read and written 
/// according to the generated schema.
/// </summary>
public class CimAutoSchemaSerializer(RdfReaderBase rdfReader) 
    : ICimSchemaSerializer
{
    public const string BaseSchemaUri = "http://cim.bios/schemas/auto";

    public ReadOnlyDictionary<string, Uri> Namespaces
        => _Namespaces.AsReadOnly();

    public void Load(TextReader reader)
    {
        _RdfReader.AddNamespace("base", new(BaseSchemaUri));
        _RdfReader.Load(reader);
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
        _ObjectsCache.TryGetValue(
            new(BaseSchemaUri + "#CimAutoClass"), 
            out var autoSuperClassResource);

        foreach (var property in node.Triples)
        {
            var classUri = MakeAncestorClassFromProperty(node.TypeIdentifier, 
                property);

            if (classUri == null)
            {
                continue;
            }

            CimAutoClass? propertyDatatype = null;
            CimMetaPropertyKind propertyKind = CimMetaPropertyKind.NonStandard;
            if (property.Object is RdfTripleObjectUriContainer uriContainer)
            {
                // local link.
                if (uriContainer.UriObject == Namespaces["base"])
                {
                    // The most generalized kind for assoc.
                    propertyKind = CimMetaPropertyKind.Assoc1ToM;
                    propertyDatatype = autoSuperClassResource as CimAutoClass;
                }
                else
                {
                    // individual a.k.a enum
                    propertyKind = CimMetaPropertyKind.Attribute;

                    var enumClass = CreateOrAugmentEnumClass(
                        uriContainer.UriObject);
                    propertyDatatype = enumClass;
                }
            }
            else if (property.Object is
                    RdfTripleObjectStatementsContainer statements
                 && statements.RdfNodesObject.Count == 1)
            {
                var subRdfNode = statements.RdfNodesObject.First();

                propertyKind = CimMetaPropertyKind.Attribute;
                var compoundCLass = AddClass(subRdfNode.TypeIdentifier, 
                    false, true);

                HandleProperties(subRdfNode);
                propertyDatatype = compoundCLass;
            }
            else if (property.Object is RdfTripleObjectLiteralContainer literal)
            {
                propertyDatatype = GetLiteralDatatype(literal.LiteralObject);
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

        _ObjectsCache.TryGetValue(
            new(BaseSchemaUri + "#CimAutoClass"), 
            out var autoSuperClassResource);

        var autoClass = new CimAutoClass()
        {
            BaseUri = typeIdentifier,
            ShortName = shortName,
            Description = string.Empty,
            ParentClass = autoSuperClassResource as CimAutoClass,
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
        CimMetaPropertyKind propertyKind, CimAutoClass? propertyDatatypeClass)
    {
        if (_ObjectsCache.TryGetValue(propertyUri, out var existingPropertyRes))
        {
            if (existingPropertyRes is CimAutoProperty existingProperty
                && propertyDatatypeClass is CimAutoDatatype propertyDatatype)
            {
                InvalidatePropertyDatatype(existingProperty, propertyDatatype);
            }

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
            PropertyDatatype = propertyDatatypeClass
        };

        _ObjectsCache.TryAdd(propertyUri, autoProperty);

        return true;
    }

    /// <summary>
    /// Check and rebind property datatype in case of literal data format change.
    /// </summary>
    /// <param name="property">Schema property entity.</param>
    /// <param name="propertyDatatype">New property datatype.</param>
    private void InvalidatePropertyDatatype(CimAutoProperty property,
        CimAutoDatatype propertyDatatype)
    {
        if (property.PropertyDatatype == null)
        {
            property.PropertyDatatype = propertyDatatype;
            return;
        }

        if (propertyDatatype == property.PropertyDatatype)
        {
            return;
        }

        var currentDatatypeOrder = LiteralValueTypeRecognizer
            .GetTypeSetOrder(property.PropertyDatatype!.BaseUri.AbsoluteUri);

        var newDatatypeOrder = LiteralValueTypeRecognizer
            .GetTypeSetOrder(propertyDatatype.BaseUri.AbsoluteUri);

        if (newDatatypeOrder > currentDatatypeOrder) 
        {
            property.PropertyDatatype = propertyDatatype;
        }       
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

        childClass.AddExtension(ancestorClass);

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
    /// Get literal datatype.
    /// </summary>
    /// <param name="literal">Literal string value.</param>
    /// <returns>Schema meta datatype entity.</returns>
    private CimAutoDatatype GetLiteralDatatype(string literal)
    {
        var typeUri = LiteralValueTypeRecognizer.Recognize(literal);

        if (_ObjectsCache.TryGetValue(typeUri, out var typeResource)
            && typeResource is CimAutoDatatype literalType)
        {
            return literalType;
        }
        else
        {
            throw new 
                Exception("Internal datatype has not beein initialized: " +
                    typeUri.AbsoluteUri);
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

        CreateAutoSuperClass();
    }

    private void CreateAutoSuperClass()
    {
        var autoClass = new CimAutoClass()
        {
            BaseUri = new(BaseSchemaUri + "#CimAutoClass"),
            ShortName = "CimAutoClass",
            Description = string.Empty,
            IsEnum = false,
            IsCompound = false
        };

        _ObjectsCache.TryAdd(autoClass.BaseUri, autoClass);
    }

    private readonly RdfReaderBase _RdfReader = rdfReader;

    private readonly Dictionary<Uri, ICimMetaResource> _ObjectsCache 
        = new(new RdfUriComparer());

    private Dictionary <string, Uri> _Namespaces = [];
}

public class CimAutoSchemaSerializerFactory(RdfReaderBase rdfReader)
    : ICimSchemaSerializerFactory
{
    public ICimSchemaSerializer CreateSerializer()
    {
        return new CimAutoSchemaSerializer(rdfReader);
    }
}
