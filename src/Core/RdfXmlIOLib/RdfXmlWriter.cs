using System.Xml.Linq;

namespace CimBios.Core.RdfXmlIOLib;

// TODO: docs everywhere

/// <summary>
/// 
/// </summary>
public class RdfXmlWriter
{
    // TODO:    Code style fields order: props (public, private), constructor, 
    //          methods (public, private), fields
    //          + lines 80 chars length limit!
    
    // TODO: no property? needs only in Write method.
    private XDocument _xDoc;
    public RdfXmlWriter() { }

    /// <summary>
    /// RDF document namespaces dictionary.
    /// </summary>
    /// TODO: Fill it from ICimSchema.Namespaces via serializer. 
    public Dictionary<string, XNamespace> Namespaces { get => _Namespaces; }

    private Dictionary<string, XNamespace> _Namespaces { get; set; }
        = new Dictionary<string, XNamespace>();

    /// <summary>
    /// 
    /// </summary>
    /// <param name="rdfNodes"></param>
    /// <returns></returns>
    public XDocument Write(IEnumerable<RdfNode> rdfNodes)
    {
        _xDoc = new XDocument();

        // TODO: add root xelement to xdoc here
        CreateRootRdfNode();
        
        foreach (RdfNode rdfNode in rdfNodes)
        {
            // TODO: exlude '#_' prefix
            var serializedNode = new XElement(rdfNode.TypeIdentifier.ToString(), new XAttribute("rdf:about", $"#_{rdfNode.Identifier}"));
            WriteTriples(ref serializedNode, rdfNode.Triples);

            _xDoc.Add(serializedNode);
        }

        return _xDoc;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="serializedNode"></param>
    /// <param name="triples"></param>
    /// TODO:   Exlude System.Guid parsing
    ///         Add rdf:id support
    ///         Exlude '#_' prefix (its serializer resp-ty)
    private void WriteTriples(ref XElement serializedNode, IEnumerable<RdfTriple> triples)
    {
        foreach (var triple in triples)
        {
            if (Guid.TryParse(triple.Object.ToString(), out Guid guidResult))
            {
                serializedNode.Add(new XElement(triple.Predicate.ToString(), new XAttribute("rdf:resource", $"#_{guidResult}")));
            }
            else if (triple.Object is RdfNode compound)
            {
                var compoundNode = new XElement(triple.Predicate.ToString(),
                                       new XElement(compound.TypeIdentifier.ToString()));
                WriteTriples(ref compoundNode, compound.Triples); // Рекурсия для компаундов
                serializedNode.Add(compoundNode);
            }
            serializedNode.Add(new XElement(triple.Predicate.ToString(), triple.Object.ToString()));
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// TODO:   reimplement namespaces to Namespaces property.
    ///         return xelement besides to add to xdoc.
    private void CreateRootRdfNode()
    {
        XAttribute[] namespaces = 
            {   new XAttribute("xmlns:rdf", "http://www.w3.org/1999/02/22-rdf-syntax-ns#"),
                new XAttribute("xmlns:cim", "http://iec.ch/TC57/2014/CIM-schema-cim16#"),
                new XAttribute("xmlns:cim17", "http://iec.ch/TC57/2014/CIM-schema-cim17#"),
                new XAttribute("xmlns:rf", "http://gost.ru/2019/schema-cim01#"),
                new XAttribute("xmlns:me", "http://monitel.com/2014/schema-cim16"),
                new XAttribute("xmlns:rh", "http://rushydro.ru/2015/schema-cim16#"),
                new XAttribute("xmlns:so", "http://so-ups.ru/2015/schema-cim16#"),                
                new XAttribute("xmlns:md", "http://iec.ch/TC57/61970-552/ModelDescription/1#"),
        };
        var header = new XElement("rdf:RDF", namespaces);
        _xDoc.Add(header);
    }

}
