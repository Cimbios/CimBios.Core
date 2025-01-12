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

    /// <summary>
    /// Load rdf/xml content from XDocument.
    /// </summary>
    public void Load(XmlReader xmlReader)
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

        var nodesStack = new Stack<RdfNode>();
        var subtreeReader = _XmlReader.ReadSubtree();
        RdfNode? rdfNode = null;
        bool blockReading = false;

        do
        {
            if (blockReading == false 
                && subtreeReader.Read() == false)
            {
                break;
            }
            blockReading = false;

            if (subtreeReader.NodeType == XmlNodeType.EndElement
                && (subtreeReader.Depth & 1) == 0)
            {
                _XmlReader.ReadEndElement();
                rdfNode = nodesStack.Pop();

                if (nodesStack.Count != 0)
                {
                    nodesStack.Peek().NewTriple(
                        NameToUri(_XmlReader.Name), 
                        rdfNode);
                }

                continue;
            }

            if (subtreeReader.NodeType != XmlNodeType.Element)
            {
                continue;
            }

            int initialDepth = subtreeReader.Depth;

            var nodeInfo = ReadNodeHeader();
            // Depth parity is node-predicate arc marker.
            if ((initialDepth & 1) == 0)
            {
                var nodeIRI = NameToUri(nodeInfo.Identifier);
                var typeIRI = NameToUri(nodeInfo.TypeIdentifier);
                var nextNode = new RdfNode(nodeIRI, typeIRI, nodeInfo.IsAuto);
                
                nodesStack.Push(nextNode);
            }
            else
            {
                if (nodeInfo.AttributesMap.ContainsKey(rdf + "resource"))
                {
                    nodesStack.Peek().NewTriple(
                        NameToUri(nodeInfo.TypeIdentifier), 
                        NameToUri(nodeInfo.Identifier));
                }
                else if (subtreeReader.Read() 
                    && subtreeReader.NodeType == XmlNodeType.Text)
                {
                    nodesStack.Peek().NewTriple(
                        NameToUri(nodeInfo.TypeIdentifier), 
                        subtreeReader.Value);
                }
                else if (subtreeReader.NodeType == XmlNodeType.Element)
                {
                    blockReading = true;
                }
            }
        }
        while (nodesStack.Count != 0);

        return rdfNode;
    }

    /// <summary>
    /// Skip while Element not found or eof
    /// </summary>
    /// <returns>True if next element reached.</returns>
    private bool SkipToNextElement()
    {
        while (_XmlReader.NodeType != XmlNodeType.Element)
        {   
            if (_XmlReader.Read() == false)
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
