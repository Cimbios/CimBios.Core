using System.Xml.Linq;

namespace CimBios.Core.RdfXmlIOLib;

/// <summary>
/// Writer for rdf/xml formatted data.
/// Converts data from RDF-Triple format to XDocument.
/// </summary>
public class RdfXmlWriter
{
    /// <summary>
    /// RDF document namespaces dictionary.
    /// </summary>
    public Dictionary<string, Uri> Namespaces { get => _Namespaces; }

    private Dictionary<string, Uri> _Namespaces { get; set; }
        = new Dictionary<string, Uri>();

    /// <summary>
    /// Default constructor, needs namespaces from Schema to function properly
    /// </summary>
    public RdfXmlWriter() { }

    /// <summary>
    /// Writes RdfNodes to XxmlDocument
    /// </summary>
    /// <param name="rdfNodes"></param>
    /// <param name="excludeBase"></param>
    /// <returns>Serialized model XDocument</returns>
    public XDocument Write(IEnumerable<RdfNode> rdfNodes,
        bool excludeBase = true)
    {
        var xDoc = new XDocument();

        var header = CreateRootRdfNode();
        xDoc.Add(header);

        foreach (RdfNode rdfNode in rdfNodes)
        {
            var resouceIdentifier = NormalizeIdentifier(rdfNode.Identifier);

            var nodeIdentifier = UriToXName(rdfNode.TypeIdentifier);
            var serializedNode = new XElement(nodeIdentifier,
                                 new XAttribute(rdf + "about", resouceIdentifier));

            WriteTriples(ref serializedNode, rdfNode.Triples);

            header.Add(serializedNode);
        }

        if (excludeBase == true)
        {
            header.Attribute(XNamespace.Xmlns + "base")?.Remove();
        }
        
        return xDoc;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="uri"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    private XName UriToXName(Uri uri)
    {        
        XNamespace ns = uri.AbsoluteUri[..(uri.AbsoluteUri.IndexOf('#') + 1)];

        if (Namespaces.Values.Contains(uri) == false)
        {
            throw new Exception("RdfXmlWriter.GetNameWithPrefix: no ns");
        }

        if (RdfUtils.TryGetEscapedIdentifier(uri, out var identifier))
        {
            XName result = ns + identifier;
            return result;
        }

        throw new Exception("RdfXmlWriter.GetNameWithPrefix: invalid rid");
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="uri"></param>
    /// <returns></returns>
    private string NormalizeIdentifier(Uri uri)
    {
        string result = uri.AbsoluteUri;

        if (Namespaces.TryGetValue("base", out var baseUri)
            && baseUri == uri)
        {
            result = uri.AbsoluteUri[uri.AbsoluteUri.IndexOf('#')..];
        } 
        else if (RdfUtils.TryGetEscapedIdentifier(uri, out var rid))
        {
            if (Namespaces.ContainsValue(uri))
            {
                var prefix = Namespaces.FirstOrDefault(ns => ns.Value == uri).Key;
                result = $"{prefix}:{rid}";
            }
            else
            {
                result = rid;
            }
        }

        return result;
    }

    /// <summary>
    /// Converts RdfTriples into XElements for the node that was given as ref object
    /// </summary>
    /// <param name="serializedNode"></param>
    /// <param name="triples"></param>
    private void WriteTriples(ref XElement serializedNode, 
        IEnumerable<RdfTriple> triples)
    {
        foreach (var triple in triples)
        {
            var xPredicate = UriToXName(triple.Predicate);

            // resource reference - blank
            if (triple.Object is Uri refIdentifier)
            {
                serializedNode.Add(
                    new XElement(
                        xPredicate,
                        new XAttribute(
                            rdf + "resource", 
                            NormalizeIdentifier(refIdentifier)))
                );
            }
            // compound prop - anonymous
            else if (triple.Object is RdfNode compound)
            {
                var xType = UriToXName(compound.TypeIdentifier);

                var headCompound = new XElement(xType);
                var compoundNode = new XElement(xPredicate, headCompound);
                WriteTriples(ref compoundNode, compound.Triples);

                headCompound.Add(compoundNode.LastNode);
                compoundNode.LastNode?.Remove();

                serializedNode.Add(compoundNode);
            }
            // variable prop - literal
            else
            {
                serializedNode.Add(new XElement(xPredicate, triple.Object));
            }
        }
    }

    /// <summary>
    /// Creates header-node with all on XML-RDF namespaces
    /// </summary>
    private XElement CreateRootRdfNode()
    {
        var xNamespaces = Namespaces.Select(
            n =>  new XAttribute(XNamespace.Xmlns + n.Key, n.Value));

        return new XElement(rdf + "RDF", xNamespaces);
    }

    private static XNamespace rdf = "http://www.w3.org/1999/02/22-rdf-syntax-ns#";
}
