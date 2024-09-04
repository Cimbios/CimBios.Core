using System.Xml.Linq;

namespace CimBios.Core.RdfXmlIOLib;
public class RdfXmlWriter
{
    private XDocument _xDoc;
    public RdfXmlWriter() { }
    public XDocument Write(IEnumerable<RdfNode> rdfNodes)
    {
        _xDoc = new XDocument();
        WriteFirstLine();
        foreach (RdfNode rdfNode in rdfNodes)
        {
            var serializedNode = new XElement(rdfNode.TypeIdentifier.ToString(), new XAttribute("rdf:about", $"#_{rdfNode.Identifier}"));
            WriteTriples(ref serializedNode, rdfNode.Triples);

            _xDoc.Add(serializedNode);
        }
        return _xDoc;
    }

    private void WriteTriples(ref XElement serializedNode, IEnumerable<RdfTriple> triples)
    {
        foreach (var triple in triples)
        {
            if (Guid.TryParse(triple.Object.ToString(), out Guid guidResult))
            {
                serializedNode.Add(new XElement(triple.Predicate.ToString(), new XAttribute("rdf:resource", $"#_{guidResult}")));
            }
            else if (triple.Object is RdfNode)
            {
                var compound = (RdfNode)triple.Object;
                var compoundNode = new XElement(triple.Predicate.ToString(),
                                       new XElement(compound.TypeIdentifier.ToString()));
                WriteTriples(ref compoundNode, compound.Triples); // Рекурсия для компаундов
                serializedNode.Add(compoundNode);
            }
            serializedNode.Add(new XElement(triple.Predicate.ToString(), triple.Object.ToString()));
        }
    }

    private void WriteFirstLine()
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
