using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection.PortableExecutable;
using System.Xml.Linq;

namespace CimBios.Core.RdfXmlIOLib;

// TODO: docs everywhere

/// <summary>
/// Writer for rdf/xml formatted data.
/// Converts data from RDF-Triple format to XDocument.
/// </summary>
public class RdfXmlWriter
{
    // TODO:    Code style fields order: props (public, private), constructor, 
    //          methods (public, private), fields
    //          + lines 80 chars length limit!
    /// <summary>
    /// Default constructor, needs namespaces from Schema to function properly
    /// </summary>
    /// <param name="schemaNamespaces"></param>
    public RdfXmlWriter(Dictionary<string, Uri> schemaNamespaces)
    {
        Namespaces = new ReadOnlyDictionary<string, Uri>(schemaNamespaces);
    }

    /// <summary>
    /// Writes RdfNodes to XDocument
    /// </summary>
    /// <param name="rdfNodes"></param>
    /// <returns></returns>
    public XDocument Write(IEnumerable<RdfNode> rdfNodes)
    {
        var xDoc = new XDocument();

        // TODO: add root xelement to xdoc here | Done
        var header = CreateRootRdfNode();
        xDoc.Add(header);

        foreach (RdfNode rdfNode in rdfNodes)
        {
            // TODO: exlude '#_' prefix | Done
            var serializedNode = new XElement(ShortForm(rdfNode.TypeIdentifier),
                                 new XAttribute(rdf + "about", $"#_{rdfNode.Identifier}"));
            WriteTriples(ref serializedNode, rdfNode.Triples);

            xDoc.Add(serializedNode);
        }
        return xDoc;
    }

    /// <summary>
    /// Shortens Namespace part of the string
    /// </summary>
    /// <param name="absoluteUri"></param>
    /// <returns></returns>
    private XName ShortForm(Uri absoluteUri)
    {
        var splitUri = absoluteUri.ToString().Split('#');
        XNamespace result = absoluteUri.AbsoluteUri.ToString();
        return result + absoluteUri.Fragment.Trim('#');
        /*return Namespaces.FirstOrDefault(x => x.Value.AbsolutePath == splitUri[0]).Key cim:IdentifiedObject
            + ":" + splitUri[1];*/
    }

    /// <summary>
    /// Converts RdfTriples into XElements for the node that was given as ref object
    /// </summary>
    /// <param name="serializedNode"></param>
    /// <param name="triples"></param>
    /// TODO:   Exlude System.Guid parsing | Done now checks if IsWellFormedUriString
    ///         Add rdf:id support | Questions
    ///         Exlude '#_' prefix (its serializer resp-ty) | Done
    private void WriteTriples(ref XElement serializedNode, IEnumerable<RdfTriple> triples)
    {
        foreach (var triple in triples)
        {
            if (Uri.IsWellFormedUriString(triple.Object.ToString(), UriKind.RelativeOrAbsolute))
            {
                serializedNode.Add(new XElement(ShortForm(triple.Predicate),
                                   new XAttribute(rdf + "resource", $"#_{triple.Object}")));
            }
            else if (triple.Object is RdfNode compound)
            {
                var compoundNode = new XElement(ShortForm(triple.Predicate),
                                       new XElement(ShortForm(compound.TypeIdentifier)));
                WriteTriples(ref compoundNode, compound.Triples);
                serializedNode.Add(compoundNode);
            }
            serializedNode.Add(new XElement(ShortForm(triple.Predicate), triple.Object.ToString()));
        }
    }

    /// <summary>
    /// Creates header-node with all on XML-RDF namespaces
    /// </summary>
    /// TODO:   reimplement namespaces to Namespaces property. | Done
    ///         return xelement besides to add to xdoc. | Done
    private XElement CreateRootRdfNode()
    {
        XAttribute[] namespaces;
        if (Namespaces == null)
        {
            namespaces = new XAttribute[8] // For Testing
            {
                new XAttribute("xmlns:rdf", "http://www.w3.org/1999/02/22-rdf-syntax-ns#"),
                new XAttribute("xmlns:cim", "http://iec.ch/TC57/2014/CIM-schema-cim16#"),
                new XAttribute("xmlns:cim17", "http://iec.ch/TC57/2014/CIM-schema-cim17#"),
                new XAttribute("xmlns:rf", "http://gost.ru/2019/schema-cim01#"),
                new XAttribute("xmlns:me", "http://monitel.com/2014/schema-cim16"),
                new XAttribute("xmlns:rh", "http://rushydro.ru/2015/schema-cim16#"),
                new XAttribute("xmlns:so", "http://so-ups.ru/2015/schema-cim16#"),
                new XAttribute("xmlns:md", "http://iec.ch/TC57/61970-552/ModelDescription/1#"),
            };
        }
        else
        {
            namespaces = new XAttribute[Namespaces.Count];
            foreach (var entry in Namespaces)
            {
                namespaces.Append(new XAttribute(entry.Key, entry.Value));
            }
        }

        return new XElement(rdf + "RDF", namespaces);
        //return new XElement("rdf:RDF", namespaces);
    }

    /// <summary>
    /// RDF document namespaces dictionary.
    /// </summary>
    /// TODO: Fill it from ICimSchema.Namespaces via serializer. | Done
    public IReadOnlyDictionary<string, Uri> Namespaces
    {
        get => _namespaces;
        set => _namespaces = (ReadOnlyDictionary<string, Uri>)value;
    }

    private ReadOnlyDictionary<string, Uri> _namespaces { get; set; }
    XNamespace rdf = "http://www.w3.org/1999/02/22-rdf-syntax-ns#";
}
