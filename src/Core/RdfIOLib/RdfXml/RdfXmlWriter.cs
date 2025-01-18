using System.Xml;

namespace CimBios.Core.RdfIOLib;

/// <summary>
/// Writer for rdf/xml formatted data.
/// Converts data from RDF-Triple format to XDocument.
/// </summary>
public class RdfXmlWriter : RdfWriterBase
{
    private XmlWriter _XmlWriter
    {
        get
        {
            if (_xmlWriter == null)
            {
                throw new InvalidOperationException("XmlWriter has not been initialized!");
            }

            return _xmlWriter;
        }
    }

    /// <summary>
    /// Default constructor, needs namespaces from Schema to function properly
    /// </summary>
    public RdfXmlWriter() { }

    public override void Open(TextWriter textWriter,
        bool excludeBase = true)
    {
        var xmlWriter = XmlWriter.Create(textWriter,
            new XmlWriterSettings()
            {
                Indent = true,
                CloseOutput = true,
            }
        );

        Open(xmlWriter);
    }

    public override void Open(XmlWriter xmlWriter,
        bool excludeBase = true)
    {
        _xmlWriter = xmlWriter;

        if (_xmlWriter.WriteState == WriteState.Closed
            || _xmlWriter.WriteState == WriteState.Error)
        {
            throw new Exception("XmlWriter has not been initialized!");
        }

        if (WriteRdfRootNode() == false)
        {
            throw new Exception("Failed to write rdf:RDF root node!");
        }
    }

    public override void Close()
    {
        if (_XmlWriter.WriteState == WriteState.Closed
            || _XmlWriter.WriteState == WriteState.Error)
        {
            return;
        }

        _XmlWriter.WriteEndElement();
        _XmlWriter.WriteEndDocument();
        _XmlWriter.Close();
    }

    public override void Write(RdfNode rdfNode)
    {
        var nodeName = UriToName(rdfNode.TypeIdentifier);
        _XmlWriter.WriteStartElement(nodeName.prefix, nodeName.name, 
            Namespaces[nodeName.prefix].AbsoluteUri);

        var iri = NormalizeIdentifier(rdfNode.Identifier);

        if (rdfNode.IsAuto == false)
        {
            _XmlWriter.WriteAttributeString(
                "rdf", 
                RdfIRIMode == RdfIRIModeKind.About ? "about" : "ID", 
                rdf, iri);
        }

        foreach (var triple in rdfNode.Triples)
        {
            var (prefix, name) = UriToName(triple.Predicate);
            _XmlWriter.WriteStartElement(prefix, 
                name, Namespaces[prefix].AbsoluteUri);
            
            if (triple.Object is Uri uriObject)
            {
                _XmlWriter.WriteAttributeString(
                    "rdf", "resource", rdf, NormalizeIdentifier(uriObject));
            }
            else if (triple.Object is RdfNode compoundNode)
            {
                Write(compoundNode);
            }
            else
            {
                var formatedValue = FormatLiteralValue(triple.Object);
                _XmlWriter.WriteString(formatedValue.ToString());
            }

            _XmlWriter.WriteEndElement();
        }
        
        _XmlWriter.WriteEndElement();
    }

    public override void WriteAll(IEnumerable<RdfNode> rdfNodes)
    {
        foreach (var rdfNode in rdfNodes)
        {
            Write(rdfNode);
        }

        Close();
    }

    /// <summary>
    /// Creates header-node with all on XML-RDF namespaces
    /// </summary>
    private bool WriteRdfRootNode()
    {
        if (CanWriteNext() == false)
        {
            return false;
        }

        _XmlWriter.WriteStartDocument();
        _XmlWriter.WriteStartElement("rdf", "RDF", rdf);

        foreach (var ns in Namespaces)
        {
            _XmlWriter.WriteAttributeString("xmlns", ns.Key, 
                xmlns, ns.Value.AbsoluteUri);
        }

        return true;
    }

    /// <summary>
    /// Get writing ability status.
    /// </summary>
    /// <returns>True if writing is available.</returns>
    private bool CanWriteNext()
    {
        return _xmlWriter != null
            && _xmlWriter.WriteState != WriteState.Closed
            && _xmlWriter.WriteState != WriteState.Error;
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

    private XmlWriter? _xmlWriter = null;
}
