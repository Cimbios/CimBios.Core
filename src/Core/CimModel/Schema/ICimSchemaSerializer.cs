using System.Collections.ObjectModel;
using CimBios.Core.RdfIOLib;

namespace CimBios.Core.CimModel.Schema;

/// <summary>
/// Root interface cim schema serializer.
/// </summary>
public interface ICimSchemaSerializer
{
    /// <summary>
    /// Prefix to namespace URI mapping for schema.
    /// </summary>
    public ReadOnlyDictionary <string, Uri> Namespaces { get; }

    /// <summary>
    /// Load raw schema text data.
    /// </summary>
    public void Load(TextReader reader);

    /// <summary>
    /// Deserialize data to CIM schema.
    /// </summary>
    public Dictionary<Uri, ICimMetaResource> Deserialize();
}

public interface ICimSchemaSerializerFactory
{
    /// <summary>
    /// Factory method creates concrete serializer instance.
    /// </summary>
    /// <param name="rdfReader">Rdf reader provider.</param>
    /// <returns>Serializer instance.</returns>
    public ICimSchemaSerializer CreateSerializer();
}