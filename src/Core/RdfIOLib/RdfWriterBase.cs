using System.Xml.Linq;

namespace CimBios.Core.RdfIOLib;

public abstract class RdfWriterBase : RdfNamespacesContainerBase
{
    protected RdfWriterBase() { }

    /// <summary>
    /// Writes RdfNodes to XxmlDocument
    /// </summary>
    /// <param name="rdfNodes"></param>
    /// <param name="excludeBase"></param>
    /// <returns>Serialized model XDocument</returns>
    public abstract XDocument Write(IEnumerable<RdfNode> rdfNodes, 
        bool excludeBase = true);

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
