using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection.PortableExecutable;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.Xml;
using System.ComponentModel.Design;

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
    /// Writes RdfNodes to XxmlDocument
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
            var headTuple = GetNamespace(rdfNode.TypeIdentifier);
            var serializedNode = new XElement(headTuple.Item1 + headTuple.Item2,
                                    new XAttribute(rdf + "about", $"{GetUuid(rdfNode.Identifier.ToString())}"));
            WriteTriples(ref serializedNode, rdfNode.Triples);

            header.Add(serializedNode);
        }
        return xDoc;
    }

    private Tuple<XNamespace, string> GetNamespace(Uri uri)
    {
        XNamespace selectedNamespace = Namespaces.FirstOrDefault(
            x => uri.ToString().Contains(x.Value.ToString())).Value.ToString();

        var remainder = uri.ToString().Substring(
            selectedNamespace.ToString().Length);
        if (remainder.StartsWith('#')) remainder = remainder.Substring(1);

        return new Tuple<XNamespace, string>(
            selectedNamespace, remainder);
    }

    private string GetUuid(string stringWithUid)
    {
        var regexPattern = new Regex(
            "(?im)[{(]?[0-9A-F]{8}[-]?(?:[0-9A-F]{4}[-]?){3}[0-9A-F]{12}[)}]?");
        var match = regexPattern.Matches(stringWithUid);
        if (match.Any())
        {
            return "#_" + match[0].Value;//Expecting a single GUID pattern in a string
        }
        return null;
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
            var dataTuple = GetNamespace(triple.Predicate);
            if (GetUuid(triple.Object.ToString()) != null)
            {
                serializedNode.Add(new XElement(dataTuple.Item1 + dataTuple.Item2,
                                    new XAttribute(rdf + "resource", $"{GetUuid(triple.Object.ToString())}")));
            }
            else if (triple.Object is RdfNode compound)
            {
                var headCompoundNode = GetNamespace(compound.TypeIdentifier);
                var compoundNode = new XElement(dataTuple.Item1 + dataTuple.Item2,
                                    new XElement(headCompoundNode.Item1 + headCompoundNode.Item2));
                XElement headCompound = (XElement)compoundNode.LastNode;
                WriteTriples(ref compoundNode, compound.Triples);
                headCompound.Add(compoundNode.LastNode);
                compoundNode.LastNode.Remove();
                serializedNode.Add(compoundNode);
            }
            else
            {
                if (Uri.IsWellFormedUriString(triple.Object.ToString(), UriKind.Absolute)) //For Enum
                {
                    var obj = GetNamespace((Uri)triple.Object);
                    serializedNode.Add(new XElement(dataTuple.Item1 + dataTuple.Item2,
                                        new XAttribute(rdf + "resource", $"{Namespaces.FirstOrDefault(x => x.Value.ToString() == obj.Item1.ToString()).Key}:{obj.Item2}")));
                }
                else
                {
                    serializedNode.Add(new XElement(dataTuple.Item1 + dataTuple.Item2, triple.Object.ToString()));
                }
            }
        }
    }

    /// <summary>
    /// Creates header-node with all on XML-RDF namespaces
    /// </summary>
    /// TODO:   reimplement namespaces to Namespaces property. | Done
    ///         return xelement besides to add to xdoc. | Done
    private XElement CreateRootRdfNode()
    {
        List<XAttribute> namespaces;
        if (Namespaces == null)
        {
            namespaces = new List<XAttribute>() // For Testing
            {
                new XAttribute(XNamespace.Xmlns + "rdf", "http://www.w3.org/1999/02/22-rdf-syntax-ns#"),
                new XAttribute(XNamespace.Xmlns + "cim", "http://iec.ch/TC57/2014/CIM-schema-cim16#"),
                new XAttribute(XNamespace.Xmlns + "cim17", "http://iec.ch/TC57/2014/CIM-schema-cim17#"),
                new XAttribute(XNamespace.Xmlns + "rf", "http://gost.ru/2019/schema-cim01#"),
                new XAttribute(XNamespace.Xmlns + "me", "http://monitel.com/2014/schema-cim16"),
                new XAttribute(XNamespace.Xmlns + "rh", "http://rushydro.ru/2015/schema-cim16#"),
                new XAttribute(XNamespace.Xmlns + "so", "http://so-ups.ru/2015/schema-cim16#"),
                new XAttribute(XNamespace.Xmlns + "md", "http://iec.ch/TC57/61970-552/ModelDescription/1#"),
            };
        }
        else
        {
            namespaces = new List<XAttribute>();
            foreach (var entry in Namespaces)
            {
                namespaces.Add(new XAttribute(XNamespace.Xmlns + entry.Key, entry.Value));
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
