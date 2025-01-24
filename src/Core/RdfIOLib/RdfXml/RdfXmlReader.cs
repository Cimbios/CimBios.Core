using System.Xml;
using System.Text;
using System.Collections.ObjectModel;

namespace CimBios.Core.RdfIOLib;

/// <summary>
/// Reader for rdf/xml formatted data.
/// Presents data in RDF-Triple format.
/// </summary>
public sealed class RdfXmlReader : RdfReaderBase
{
    private XmlReader _XmlReader
    {
        get
        {
            if (_xmlReader == null)
            {
                throw new InvalidOperationException("XmlReader has not been initialized!");
            }

            return _xmlReader;
        }
    }

    /// <summary>
    /// Default constructor.
    /// </summary>
    public RdfXmlReader() { }

    /// <summary>
    /// Constructor sets base namespace.
    /// </summary>
    /// <param name="baseNamespace">Base namespace for local identified objects.</param>
    public RdfXmlReader(Uri baseNamespace)
    {
        AddNamespace("base", baseNamespace);
    }

    public override void Parse(string content, 
        Encoding? encoding = null)
    {
        encoding ??= Encoding.Default;

        var stringBytes = encoding.GetBytes(content);
        var stringStream = new MemoryStream(stringBytes);
        var streamReader = new StreamReader(stringStream);
        Load(streamReader);
    }

    public override void Load(TextReader textReader)
    {
        var xmlReader = XmlReader.Create(textReader,
            new XmlReaderSettings()
            {
                IgnoreWhitespace = true,
                IgnoreComments = true,
            }
        );

        Load(xmlReader);
    }

    public override void Load(XmlReader xmlReader)
    {
        ClearNamespaces();
        _xmlReader = xmlReader;

        if (_XmlReader.ReadState != ReadState.Initial
            && _XmlReader.ReadState != ReadState.Interactive)
        {
            throw new Exception("XmlReader has not been initialized!");
        }

        if (ReadRdfRootNode() == false)
        {
            throw new Exception("Xml document does not contains rdf:RDF root node!");
        }
    }

    public override void Close()
    {
        ClearNamespaces();
        _xmlReader?.Close();
    }
    
    public override IEnumerable<RdfNode> ReadAll()
    {
        RdfNode? rdfNode;
        while ((rdfNode = ReadNext()) != null)
        {
            yield return rdfNode;
        }
    }

    public override RdfNode? ReadNext()
    {
        if (CanReadNext() == false)
        {
            return null;
        }

        if (SkipToNextElement() == false)
        {
            return null;
        }

        var subtreeReader = _XmlReader.ReadSubtree();
        subtreeReader.Read();

        var nodeInfo = ReadNodeHeader();
        var nodeIRI = NameToUri(nodeInfo.Identifier);
        var typeIRI = NameToUri(nodeInfo.TypeIdentifier);
        var rdfNode = new RdfNode(nodeIRI, typeIRI, nodeInfo.IsAuto);

        while (subtreeReader.Read())
        {
            if (subtreeReader.NodeType == XmlNodeType.EndElement 
                && subtreeReader.Depth == 0)
            {
                break;
            }

            if (SkipToNextElement(subtreeReader) == false)
            {
                break;
            }

            var predicateInfo = ReadNodeHeader();
            if (predicateInfo.AttributesMap.ContainsKey(rdf + "resource"))
            {
                rdfNode.NewTriple(
                    NameToUri(predicateInfo.TypeIdentifier), 
                    NameToUri(predicateInfo.Identifier));
            }
            else if (subtreeReader.Read() 
                && subtreeReader.NodeType == XmlNodeType.Text)
            {
                rdfNode.NewTriple(
                    NameToUri(predicateInfo.TypeIdentifier), 
                    subtreeReader.Value);
            }
            else if (subtreeReader.NodeType == XmlNodeType.Element)
            {
                do
                {
                    if (subtreeReader.NodeType == XmlNodeType.EndElement)
                    {
                        break;
                    }

                    var nextNode = ReadNext();
                    if (nextNode != null)
                    {
                        rdfNode.NewTriple(
                            NameToUri(predicateInfo.TypeIdentifier), 
                            nextNode);
                    }
                }
                while (subtreeReader.Read());
            }
        }

        return rdfNode;
    }

    /// <summary>
    /// Skip while Element not found or eof
    /// </summary>
    /// <returns>True if next element reached.</returns>
    private bool SkipToNextElement()
    {
        return SkipToNextElement(_XmlReader);
    }

    /// <summary>
    /// Skip while Element not found or eof with special reader.
    /// </summary>
    /// <param name="xmlReader">Xml reader</param>
    /// <returns>True if next element reached.</returns>
    private bool SkipToNextElement(XmlReader xmlReader)
    {
        while (xmlReader.NodeType != XmlNodeType.Element)
        {   
            if (xmlReader.Read() == false)
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Get element identifier from rdf:about/id. 
    /// Makes auto identifier in case of unidetified element.
    /// </summary>
    /// <returns>dcd</returns>
    private RdfXmlNodeInfo ReadNodeHeader()
    {
        var info = new RdfXmlNodeInfo()
        {
            TypeIdentifier = _XmlReader.NamespaceURI + _XmlReader.LocalName,
            IsEmpty = _XmlReader.IsEmptyElement,
            IsAuto = false
        };

        var attributes = ReadNodeAttributes();
        info.AttributesMap = attributes.AsReadOnly();

        if (attributes.TryGetValue(rdf + "about", out var aboutName))
        {
            info.Identifier = aboutName;
        }
        else if (attributes.TryGetValue(rdf + "ID", out var IDName))
        {
            info.Identifier = IDName;
        }
        else if (attributes.TryGetValue(rdf + "resource", out var resourceName))
        {
            info.Identifier = resourceName;
        }
        else
        {
            info.IsAuto = true;
            info.Identifier = $"#_auto{_XmlReader.GetHashCode()}{info.GetHashCode()}";
        }

        return info;
    }

    /// <summary>
    /// Get reading ability status.
    /// </summary>
    /// <returns>True if reading is available.</returns>
    private bool CanReadNext()
    {
        return _xmlReader != null
            && _xmlReader.ReadState != ReadState.Closed
            && _xmlReader.ReadState != ReadState.Error
            && _xmlReader.ReadState != ReadState.EndOfFile;
    }

    /// <summary>
    /// Read attributes from element node.
    /// </summary>
    /// <returns>Pairs (attr uri, value) set.</returns>
    private Dictionary<string, string> ReadNodeAttributes()
    {
        var result = new Dictionary<string, string>();

        if (_XmlReader.HasAttributes == false)
        {
            return result;
        }

        while (_XmlReader.MoveToNextAttribute())
        {
            var uri = _XmlReader.NamespaceURI + _XmlReader.LocalName;
            result.Add(uri, _XmlReader.Value);
        }

        return result;
    }

    /// <summary>
    /// Get rdf:RDF root node.
    /// </summary>
    private bool ReadRdfRootNode()
    {
        if (_XmlReader.ReadToFollowing("RDF", rdf)
            && _XmlReader.NodeType == XmlNodeType.Element)
        {
            ParseXmlns();
            return true;
        }

        return false;
    }

    /// <summary>
    /// Parse all root namespaces.
    /// </summary>
    private void ParseXmlns()
    {
        while (_XmlReader.MoveToNextAttribute())
        {
            if (_XmlReader.NamespaceURI != xmlns
                && _XmlReader.Name != "xml:base")
            {
                continue;
            }

            AddNamespace(_XmlReader.LocalName, new Uri(_XmlReader.Value));
        }
    }

    private XmlReader? _xmlReader = null;

    private struct RdfXmlNodeInfo
    {
        public string Identifier { get; set; }
        public bool IsAuto { get; set; }
        public string TypeIdentifier { get; set; }
        public bool IsEmpty { get; set; }
        public ReadOnlyDictionary<string, string> AttributesMap { get; set; }
    }
}
