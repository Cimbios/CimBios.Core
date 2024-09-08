using System.Xml.Linq;

namespace CimBios.Core.RdfXmlIOLib;

/// <summary>
/// Reader for rdf/xml formatted data.
/// Presents data in RDF-Triple format.
/// </summary>
public sealed class RdfXmlReader
{
    /// <summary>
    /// RDF document namespaces dictionary.
    /// </summary>
    public Dictionary<string, XNamespace> Namespaces { get => _Namespaces; }

    private Dictionary<string, XNamespace> _Namespaces { get; set; }
        = new Dictionary<string, XNamespace>();

    /// <summary>
    /// Root RDF node.
    /// </summary>
    private XElement? _RdfElement { get; set; }

    /// <summary>
    /// First element in RDF root node.
    /// </summary>
    private XElement? _FirstElement
    {
        get
        {
            return _RdfElement?.Elements().Count() > 0
                ? _RdfElement.Elements().First() : null;
        }
    }

    /// <summary>
    /// Stack of read waiting elements.
    /// </summary>
    private Stack<XElement> _ReadElementsStack { get; set; }
        = new Stack<XElement>();

    /// <summary>
    /// Default constructor.
    /// </summary>
    public RdfXmlReader() { }

    /// <summary>
    /// Constructor sets base namespace.
    /// </summary>
    /// <param name="baseNamespace">Base namespace for local identified objects.</param>
    public RdfXmlReader(XNamespace baseNamespace)
    {
        Namespaces.Add("base", baseNamespace);
    }

    /// <summary>
    /// Parse string rdf/xml content.
    /// </summary>
    /// <param name="content">String rdf/xml content.</param>
    public void Parse(string content)
    {
        XDocument xDoc = XDocument.Parse(content);
        ReadRdfRootNode(xDoc);
        Reset();
    }

    /// <summary>
    /// Load rdf/xml content from TextReader.
    /// </summary>
    public void Load(TextReader textReader)
    {
        XDocument xDoc = XDocument.Load(textReader);
        Load(xDoc);
    }

    /// <summary>
    /// Load rdf/xml content from XDocument.
    /// </summary>
    public void Load(XDocument xDoc)
    {
        ReadRdfRootNode(xDoc);
        Reset();
    }

    /// <summary>
    /// Close reader.
    /// </summary>
    public void Close()
    {
        _RdfElement = null;
        _Namespaces.Clear();
        _ReadElementsStack.Clear();
    }

    /// <summary>
    /// Read RDF content of next element.
    /// </summary>
    /// <returns>RDF node of last read element.</returns>
    public RdfNode? ReadNext()
    {
        if (_ReadElementsStack.Count() == 0)
        {
            return null;
        }

        XElement content = _ReadElementsStack.Pop();

        string subjectId = GetXElementIdentifier(content, out bool isAuto);
        Uri subject = MakeUri(subjectId);

        Uri typeIdentifier = new Uri(content.Name.Namespace.NamespaceName
                + content.Name.LocalName);

        var triples = new List<RdfTriple>(content.Elements().Count());
        foreach (var child in content.Elements())
        {
            Uri predicate = new Uri(child.Name.Namespace.NamespaceName
                + child.Name.LocalName);

            // Blank node
            if (child.HasElements == false)
            {
                XAttribute? resource = child
                    .Attribute(Namespaces["rdf"] + "resource");

                object? @object;
                // Blank node
                if (resource != null)
                {
                    @object = MakeUri(resource.Value);
                }
                // Literal node
                else
                {
                    @object = child.Value;
                }

                triples.Add(new RdfTriple(subject, predicate, @object));
            }
            // Element node
            else if (child.HasElements == true)
            {
                foreach (var el in child.Elements())
                {
                    _ReadElementsStack.Push(el);
                    var subObject = ReadNext();
                    if (subObject == null)
                    {
                        continue;
                    }

                    triples.Add(new RdfTriple(subject, predicate, subObject));
                }
            }
            else
            {
                throw new Exception("Cannot parse rdf node");
            }
        }

        var rdfNode = new RdfNode(subject, typeIdentifier, 
            triples.ToArray(), isAuto);

        return rdfNode;
    }

    /// <summary>
    /// Resets current reading element to first.
    /// </summary>
    public void Reset()
    {
        if (_RdfElement == null)
        {
            throw new Exception("No rdf node");
        }

        _ReadElementsStack.Clear();
        _RdfElement.Elements().Reverse().ToList()
            .ForEach(el => _ReadElementsStack.Push(el));
    }

    /// <summary>
    /// Read all elements in document.
    /// </summary>
    /// <returns>Enumerable of RDF nodes</returns>
    public IEnumerable<RdfNode> ReadAll()
    {
        if (_RdfElement == null)
        {
            throw new Exception("No rdf node");
        }

        Reset();

        RdfNode? rdfNode;
        while ((rdfNode = ReadNext()) != null)
        {
            yield return rdfNode;
        }

        Reset();
    }

    /// <summary>
    /// Make Uri from string identifier.
    /// </summary>
    /// <param name="identifier">String local identifier.</param>
    /// <param name="ns">Namespace of identifier.</param>
    public Uri MakeUri(string identifier, string ns = "base")
    {
        var splittedPrefix = identifier.Split(':');
        if (splittedPrefix.Count() == 2
            && Namespaces.ContainsKey(splittedPrefix.First()))
        {
            return new Uri(Namespaces[splittedPrefix.First()]
                .NamespaceName + splittedPrefix.Last());
        }

        if (Uri.IsWellFormedUriString(identifier, UriKind.Absolute))
        {
            return new Uri(identifier);
        }

        return new Uri(Namespaces[ns].NamespaceName + identifier);
    }

    /// <summary>
    /// Get rdf:RDF root node.
    /// </summary>
    /// <param name="content">Linq Xml document.</param>
    private void ReadRdfRootNode(XDocument content)
    {
        XNamespace rdf = "http://www.w3.org/1999/02/22-rdf-syntax-ns#";

        XElement? rdfNode = content.Element(rdf + "RDF");
        if (rdfNode == null)
        {
            throw new Exception("No RDF Node");
        }

        _RdfElement = rdfNode;
        rdfNode.Elements().Reverse().ToList()
            .ForEach(el => _ReadElementsStack.Push(el));

        ParseXmlns(rdfNode);
    }

    /// <summary>
    /// Parse all root namespaces.
    /// </summary>
    /// <param name="rdfNode">rdf:RDF root node.</param>
    private void ParseXmlns(XElement rdfNode)
    {
        XNamespace xlmns = "http://www.w3.org/2000/xmlns/";

        foreach (XAttribute attr in rdfNode.Attributes())
        {
            if (attr.Name.Namespace != xlmns
                && attr.Name != "xml:base")
            {
                continue;
            }

            if (_Namespaces.ContainsKey(attr.Name.LocalName))
            {
                _Namespaces[attr.Name.LocalName] = attr.Value;
            }
            else
            {
                _Namespaces.Add(attr.Name.LocalName, attr.Value);
            }
        }
    }

    /// <summary>
    /// Get element identifier from rdf:about/id. Makes auto identifier in case of undentified element.
    /// </summary>
    /// <param name="element">Xml identified element.</param>
    /// <param name="isAuto">Set is auto identifier assigned.</param>
    private string GetXElementIdentifier(XElement element, out bool isAuto)
    {
        isAuto = false;
        XAttribute? about = element.Attribute(Namespaces["rdf"] + "about");
        if (about == null)
        {
            XAttribute? ID = element.Attribute(Namespaces["rdf"] + "id");

            if (ID == null)
            {
                isAuto = true;
                return $"#_auto{element.GetHashCode()}";
            }

            return ID.Value;
        }
        else
        {
            return about.Value;
        }
    }
}
