using System.Xml.Serialization;
using CimBios.Utils.ClassTraits;

namespace CimBios.Core.CimModel.Schema;

/// <summary>
/// Cim schema interface. Defines usage structure.
/// </summary>
public interface ICimSchema : ICanLog
{
    /// <summary>
    /// Prefix to namespace URI mapping for schema.
    /// </summary>
    public IReadOnlyDictionary<string, Uri> Namespaces { get; }
    
    /// <summary>
    /// All CimMetaClass instances - RDF description instances 
    /// of RDF type Class.
    /// </summary>
    public IEnumerable<ICimMetaClass> Classes { get; }

    /// <summary>
    /// All CimMetaClass instances - RDF description instances 
    /// of RDF type Class.
    /// </summary>
    public IEnumerable<ICimMetaClass> Extensions { get; }

    /// <summary>
    /// All CimMetaProperty instances - RDF description instances 
    /// of RDF type Property.
    /// </summary>
    public IEnumerable<ICimMetaProperty> Properties { get; }

    /// <summary>
    /// All CimMetaIndividual instances - RDF description concrete instances.
    /// </summary>
    public IEnumerable<ICimMetaIndividual> Individuals { get; }

    /// <summary>
    /// All CimMetaDatatype instances - RDF description instances 
    /// of RDF type Datatype.
    /// </summary>
    public IEnumerable<ICimMetaDatatype> Datatypes { get; }

    /// <summary>
    /// Tie all same name enums in one via extension link.
    /// </summary>
    public bool TieSameNameEnums { get; set; }

    /// <summary>
    /// Resource super class for all objective classes.
    /// </summary>
    public ICimMetaClass ResourceSuperClass { get; }

    /// <summary>
    /// Load RDFS schema content via text reader.
    /// </summary>
    /// <param name="textReader">Text reader provider.</param>
    public void Load(TextReader textReader);
    //public void Save(TextWriter textWriter);

    /// <summary>
    /// Load RDFS schema content via text reader with redefined serializer.
    /// </summary>
    /// <param name="textReader">Text reader provider.</param>
    /// <param name="serializerFactory">Serializer factory.</param>
    public void Load(TextReader textReader, 
        ICimSchemaSerializerFactory serializerFactory);

    /// <summary>
    /// Get concrete serialized meta description instance.
    /// </summary>
    /// <param name="uri">Identifier of instance.</param>
    /// <returns>CimRdfDescriptionBase inherits instance.</returns>
    public T? TryGetResource<T>(Uri uri) where T : ICimMetaResource;

    /// <summary>
    /// Check schema uri instance exists.
    /// </summary>
    /// <param name="uri">Identifier of instance.</param>
    /// <returns>Has schema instance with uri status.</returns>
    public bool HasUri(Uri uri);

    /// <summary>
    /// Get list of class properties.
    /// </summary>
    /// <param name="metaClass">Meta class instance.</param>
    /// <param name="inherit">Is needed to collect ancestors properties.</param>
    /// <returns>Enumerable of CimMetaProperty.</returns>
    public IEnumerable<ICimMetaProperty> GetClassProperties(
        ICimMetaClass metaClass,
        bool inherit = false,
        bool extensions = true);

    /// <summary>
    /// Get list of class individuals.
    /// </summary>
    /// <param name="metaClass">Meta class instance.</param>
    /// <param name="inherit">Is needed to collect inheritors individuals.</param>
    /// <returns></returns>
    public IEnumerable<ICimMetaIndividual> GetClassIndividuals(
        ICimMetaClass metaClass,
        bool extensions = true);

    /// <summary>
    /// Can meta class instance be created within schema.
    /// </summary>
    /// <param name="metaClass">Meta class object.</param>
    /// <returns>Can create meta class instance bool.</returns>
    public bool CanCreateClass(ICimMetaClass metaClass);

    /// <summary>
    /// Join this CIM schema with another one.
    /// NOTE: Joining schema objects grabs as refs 
    /// and still can affect on this schema!
    /// </summary>
    /// <param name="schema">ICimSchema instance.</param>
    /// <param name="rewriteNamespaces">Rewrite namespaces URI.</param>
    public void Join(ICimSchema schema, bool rewriteNamespaces = false);

    /// <summary>
    /// Get string prefix of uri namespace.
    /// </summary>
    /// <param name="uri">Object uri.</param>
    /// <returns>String prefix or '_' if namespace does not exists.</returns>
    public string GetUriNamespacePrefix(Uri uri);

    /// <summary>
    /// Clear no reference (auto created) auto meta types from schema.
    /// </summary>
    public void InvalidateAuto();
}

/// <summary>
/// Factory method interface for abstract schema activation.
/// </summary>
public interface ICimSchemaFactory
{
    /// <summary>
    /// Create ICimSchema instance.
    /// </summary>
    /// <returns>ICimSchema instance.</returns>
    public ICimSchema CreateSchema();
}
