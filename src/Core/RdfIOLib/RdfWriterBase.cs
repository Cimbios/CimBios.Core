using System.Text;
using System.Xml;

namespace CimBios.Core.RdfIOLib;

public abstract class RdfWriterBase : RdfNamespacesContainerBase
{
    protected RdfWriterBase() { }

    public RdfIRIModeKind RdfIRIMode { get; set; } = RdfIRIModeKind.About;

    /// <summary>
    /// Open rdf/xml content from TextWriter.
    /// </summary>
    public abstract void Open(TextWriter textWriter,
        bool excludeBase = true, Encoding? encoding = null);

    /// <summary>
    /// Open rdf/xml content from XmlWriter.
    /// </summary>
    public abstract void Open(XmlWriter xmlWriter,
        bool excludeBase = true);

    /// <summary>
    /// End of rdf document writing.
    /// </summary>
    public abstract void Close();

    /// <summary>
    /// Write RdfNode to XmlWriter stream
    /// </summary>
    /// <param name="rdfNode"></param>
    /// <param name="excludeBase"></param>
    /// <returns>Serialized model XDocument</returns>
    public abstract void Write(RdfNode rdfNode);

    /// <summary>
    /// Writes RdfNodes list to XmlWriter stream
    /// </summary>
    /// <param name="rdfNodes"></param>
    /// <param name="excludeBase"></param>
    /// <returns>Serialized model XDocument</returns>
    public abstract void WriteAll(IEnumerable<RdfNode> rdfNodes);
}

public enum RdfIRIModeKind
{
    About,
    ID
}
