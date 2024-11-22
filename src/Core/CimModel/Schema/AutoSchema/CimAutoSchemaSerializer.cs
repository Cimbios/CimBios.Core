using CimBios.Core.RdfIOLib;

namespace CimBios.Core.CimModel.Schema.AutoSchema;

public class CimAutoSchemaSerializer : ICimSchemaSerializer
{
    public Dictionary<string, Uri> Namespaces => _Namespaces;

    public Dictionary<Uri, ICimMetaResource> Deserialize()
    {
        throw new NotImplementedException();
    }

    public void Load(TextReader reader)
    {
        _Reader.Load(reader);
    }

    /// <summary>
    /// Move namespaces from reader doc.
    /// </summary>
    private void ForwardReaderNamespaces()
    {
        foreach (var item in _Reader.Namespaces)
        {
            _Namespaces.Add(item.Key, item.Value);
        }
    }

    private RdfXmlReader _Reader = new RdfXmlReader();

    private Dictionary <string, Uri> _Namespaces
        = new Dictionary<string, Uri>();
}
