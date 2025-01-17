using System.Xml;

namespace CimBios.Core.RdfIOLib;

public abstract class RdfWriterBase : RdfNamespacesContainerBase
{
    protected RdfWriterBase() { }

    /// <summary>
    /// Open rdf/xml content from TextWriter.
    /// </summary>
    public abstract void Open(TextWriter textWriter,
        bool excludeBase = true);

    /// <summary>
    /// Open rdf/xml content from XmlWriter.
    /// </summary>
    public abstract void Open(XmlWriter xmlWriter,
        bool excludeBase = true);

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

    /// <summary>
    /// Re-format literal values for specific types.
    /// </summary>
    /// <param name="value">Object literal value.</param>
    /// <returns>Re-formatted value.</returns>
    protected static object FormatLiteralValue(object value)
    {
        if (value is DateTime dateTimeValue)
        {
            return dateTimeValue.ToUniversalTime();
        }

        return value;
    }
}
