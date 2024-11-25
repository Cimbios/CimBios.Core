using System.Xml.Linq;

namespace CimBios.Core.RdfIOLib;

/// <summary>
/// Writer for rdf/xml formatted data.
/// Converts data from RDF-Triple format to XDocument.
/// </summary>
public class RdfXmlWriter : RdfWriterBase
{
    /// <summary>
    /// Default constructor, needs namespaces from Schema to function properly
    /// </summary>
    public RdfXmlWriter() { }

    public override XDocument Write(IEnumerable<RdfNode> rdfNodes,
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
    /// Makes string identifier with escaped syms.
    /// </summary>
    /// <param name="uri"></param>
    /// <returns></returns>
    private string NormalizeIdentifier(Uri uri)
    {
        if (Namespaces.TryGetValue("base", out var baseUri)
            && baseUri == uri)
        {
            return uri.AbsoluteUri[uri.AbsoluteUri.IndexOf('#')..];
        } 
        
        if (RdfUtils.TryGetEscapedIdentifier(uri, out var rid)
            && Namespaces.Values.Contains(uri))
        {
            var prefix = Namespaces.FirstOrDefault(ns => ns.Value == uri).Key;
            return $"{prefix}:{rid}";
        }

        return uri.AbsoluteUri;
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
}
