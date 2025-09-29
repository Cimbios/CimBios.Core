using System.Collections.ObjectModel;
using System.Text;
using System.Xml;

namespace CimBios.Core.RdfIOLib;

/// <summary>
///     Reader for rdf/xml formatted data.
///     Presents data in RDF-Triple format.
/// </summary>
public sealed class RdfXmlReader : RdfReaderBase
{
    private XmlReader? _xmlReader;

    /// <summary>
    ///     Default constructor.
    /// </summary>
    public RdfXmlReader()
    {
    }

    /// <summary>
    ///     Constructor sets base namespace.
    /// </summary>
    /// <param name="baseNamespace">Base namespace for local identified objects.</param>
    public RdfXmlReader(Uri baseNamespace)
    {
        AddNamespace("base", baseNamespace);
    }

    private XmlReader XmlReader
    {
        get
        {
            if (_xmlReader == null) throw new InvalidOperationException("XmlReader has not been initialized!");

            return _xmlReader;
        }
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
            new XmlReaderSettings
            {
                IgnoreWhitespace = true,
                IgnoreComments = true
            }
        );

        Load(xmlReader);
    }

    public override void Load(XmlReader xmlReader)
    {
        ClearNamespaces();
        _xmlReader = xmlReader;

        if (XmlReader.ReadState != ReadState.Initial
            && XmlReader.ReadState != ReadState.Interactive)
            throw new Exception("XmlReader has not been initialized!");

        if (ReadRdfRootNode() == false) throw new Exception("Xml document does not contains rdf:RDF root node!");
    }

    public override void Close()
    {
        ClearNamespaces();
        _xmlReader?.Close();
    }
    
    private const string RdfDescription = Rdf + "Description";

    public override IEnumerable<RdfNode> ReadAll()
    {
        while (ReadNext() is { } rdfNode)
        {
            if (XmlReader.IsEmptyElement) XmlReader.Read();
            
            yield return rdfNode;
        }
    }

    public override RdfNode? ReadNext()
    {
        if (CanReadNext() == false) return null;

        if (SkipToNextElement() == false) return null;

        var subtreeReader = XmlReader.ReadSubtree();
        subtreeReader.Read();

        var nodeInfo = ReadNodeHeader();
        var nodeIRI = NameToUri(nodeInfo.Identifier);
        var typeIRI = NameToUri(nodeInfo.TypeIdentifier);
        var rdfNode = new RdfNode(nodeIRI, typeIRI, nodeInfo.IsAuto);

        while (subtreeReader.Read())
        {
            if (subtreeReader is { NodeType: XmlNodeType.EndElement, Depth: 0 })
                break;

            if (SkipToNextElement(subtreeReader) == false) break;

            var predicateInfo = ReadNodeHeader();
            if (predicateInfo.AttributesMap.ContainsKey(Rdf + "resource"))
            {
                var resourceIRI = NameToUri(predicateInfo.Identifier);
                var predicateTypeIRI = NameToUri(predicateInfo.TypeIdentifier);

                if (predicateTypeIRI.AbsoluteUri == Rdf + "type")
                    rdfNode.TypeIdentifier = resourceIRI;
                else
                    rdfNode.NewTriple(predicateTypeIRI,
                        new RdfTripleObjectUriContainer(resourceIRI));
            }
            else if (subtreeReader.Read()
                     && subtreeReader.NodeType == XmlNodeType.Text)
            {
                rdfNode.NewTriple(
                    NameToUri(predicateInfo.TypeIdentifier),
                    new RdfTripleObjectLiteralContainer(subtreeReader.Value));
            }
            else if (subtreeReader.NodeType == XmlNodeType.Element)
            {
                var rdfNodesCollection = new List<RdfNode>();
                do
                {
                    if (subtreeReader.NodeType == XmlNodeType.EndElement) break;

                    var nextNode = ReadNext();
                    if (nextNode != null) rdfNodesCollection.Add(nextNode);
                } while (subtreeReader.Read());

                rdfNode.NewTriple(
                    NameToUri(predicateInfo.TypeIdentifier),
                    new RdfTripleObjectStatementsContainer(rdfNodesCollection));
            }
        }

        return rdfNode;
    }

    /// <summary>
    ///     Skip while Element not found or eof
    /// </summary>
    /// <returns>True if next element reached.</returns>
    private bool SkipToNextElement()
    {
        return SkipToNextElement(XmlReader);
    }

    /// <summary>
    ///     Skip while Element not found or eof with special reader.
    /// </summary>
    /// <param name="xmlReader">Xml reader</param>
    /// <returns>True if next element reached.</returns>
    private bool SkipToNextElement(XmlReader xmlReader)
    {
        while (xmlReader.NodeType != XmlNodeType.Element)
            if (xmlReader.Read() == false)
                return false;

        return true;
    }

    /// <summary>
    ///     Get element identifier from rdf:about/id.
    ///     Makes auto identifier in case of unidentified element.
    /// </summary>
    /// <returns>dcd</returns>
    private RdfXmlNodeInfo ReadNodeHeader()
    {
        var sharperNamespace = XmlReader.NamespaceURI;
        if (XmlReader.NamespaceURI.LastOrDefault() != '#') sharperNamespace += '#';

        var info = new RdfXmlNodeInfo
        {
            TypeIdentifier = sharperNamespace + XmlReader.LocalName,
            IsEmpty = XmlReader.IsEmptyElement,
            IsAuto = false
        };

        var attributes = ReadNodeAttributes();
        info.AttributesMap = attributes.AsReadOnly();

        if (attributes.TryGetValue(Rdf + "about", out var aboutName))
        {
            info.Identifier = aboutName;
        }
        else if (attributes.TryGetValue(Rdf + "ID", out var IDName))
        {
            info.Identifier = IDName;
        }
        else if (attributes.TryGetValue(Rdf + "resource", out var resourceName))
        {
            info.Identifier = resourceName;
        }
        else
        {
            info.IsAuto = true;
            info.Identifier = $"#_auto{XmlReader.GetHashCode()}{info.GetHashCode()}";
        }

        return info;
    }

    /// <summary>
    ///     Get reading ability status.
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
    ///     Read attributes from element node.
    /// </summary>
    /// <returns>Pairs (attr uri, value) set.</returns>
    private Dictionary<string, string> ReadNodeAttributes()
    {
        var result = new Dictionary<string, string>();

        if (XmlReader.HasAttributes == false) return result;

        while (XmlReader.MoveToNextAttribute())
        {
            var uri = XmlReader.NamespaceURI + XmlReader.LocalName;
            result.Add(uri, XmlReader.Value);
        }

        return result;
    }

    /// <summary>
    ///     Get rdf:RDF root node.
    /// </summary>
    private bool ReadRdfRootNode()
    {
        if (XmlReader.ReadToFollowing("RDF", Rdf)
            && XmlReader.NodeType == XmlNodeType.Element)
        {
            ParseXmlns();
            return true;
        }

        return false;
    }

    /// <summary>
    ///     Parse all root namespaces.
    /// </summary>
    private void ParseXmlns()
    {
        while (XmlReader.MoveToNextAttribute())
        {
            if (XmlReader.NamespaceURI != Xmlns
                && XmlReader.Name != "xml:base")
                continue;

            AddNamespace(XmlReader.LocalName, new Uri(XmlReader.Value));
        }
    }

    private record struct RdfXmlNodeInfo
    {
        public string Identifier { get; set; }
        public bool IsAuto { get; set; }
        public string TypeIdentifier { get; init; }
        public bool IsEmpty { get; set; }
        public ReadOnlyDictionary<string, string> AttributesMap { get; set; }
    }
}
