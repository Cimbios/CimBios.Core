using System.Text;

namespace CimBios.Core.RdfIOLib;

public abstract class RdfReaderBase : RdfNamespacesContainerBase
{
    protected RdfReaderBase() { }

    /// <summary>
    /// Parse string rdf/xml content.
    /// </summary>
    /// <param name="content">String rdf/xml content.</param>
    public abstract void Parse(string content, Encoding? encoding = null);

    /// <summary>
    /// Load rdf/xml content from TextReader.
    /// </summary>
    public abstract void Load(TextReader textReader);

    /// <summary>
    /// Close reader.
    /// </summary>
    public abstract void Close();

    /// <summary>
    /// Read RDF content of next element.
    /// </summary>
    /// <returns>RDF node of last read element.</returns>
    public abstract RdfNode? ReadNext();

    // <summary>
    /// Read all elements in document.
    /// </summary>
    /// <returns>Enumerable of RDF nodes</returns>
    public abstract IEnumerable<RdfNode> ReadAll();
}
