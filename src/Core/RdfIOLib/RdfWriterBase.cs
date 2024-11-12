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
}
