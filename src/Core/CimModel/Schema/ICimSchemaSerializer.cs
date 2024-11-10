namespace CimBios.Core.CimModel.Schema;

/// <summary>
/// Root interface cim schema serializer.
/// </summary>
public interface ICimSchemaSerializer
{
    /// <summary>
    /// Prefix to namespace URI mapping for schema.
    /// </summary>
    public Dictionary <string, Uri> Namespaces { get; }

    /// <summary>
    /// Load raw schema text data.
    /// </summary>
    public void Load(TextReader reader);

    /// <summary>
    /// Deserialize data to CIM schema.
    /// </summary>
    public Dictionary<Uri, ICimMetaResource> Deserialize();
}