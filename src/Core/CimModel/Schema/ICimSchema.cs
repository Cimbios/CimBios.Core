namespace CimBios.Core.CimModel.Schema;

/// <summary>
/// Cim schema interface. Defines usage structure.
/// </summary>
public interface ICimSchema
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
    /// All CimMetaProperty instances - RDF description instances 
    /// of RDF type Property.
    /// </summary>
    public IEnumerable<ICimMetaProperty> Properties { get; }
    /// <summary>
    /// All CimMetaIndividual instances - RDF description concrete instances.
    /// </summary>
    public IEnumerable<ICimMetaInstance> Individuals { get; }
    /// <summary>
    /// All CimMetaDatatype instances - RDF description instances 
    /// of RDF type Datatype.
    /// </summary>
    public IEnumerable<ICimMetaDatatype> Datatypes { get; }

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
    public T? TryGetDescription<T>(Uri uri) where T : ICimSchemaSerializable;

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
        bool inherit = false);

    /// <summary>
    /// Join this CIM schema with another one.
    /// </summary>
    /// <param name="schema">ICimSchema instance.</param>
    /// <param name="rewriteNamespaces">Rewrite namespaces URI.</param>
    public void Join(ICimSchema schema, bool rewriteNamespaces = false);
}
