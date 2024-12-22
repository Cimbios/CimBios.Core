using System.Xml.Linq;

namespace CimBios.Core.RdfIOLib;

/// <summary>
/// Reader for rdf/xml formatted data.
/// Presents data in RDF-Triple format.
/// </summary>
public sealed class RdfXmlReader : RdfReaderBase
{
    /// <summary>
    /// Root RDF node.
    /// </summary>
    private XElement? _RdfElement { get; set; }

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
    public RdfXmlReader(Uri baseNamespace)
    {
        AddNamespace("base", baseNamespace);
    }

    public override void Parse(string content)
    {
        XDocument xDoc = XDocument.Parse(content);
        Load(xDoc);
    }

    public override void Load(TextReader textReader)
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

    public override void Close()
    {
        _RdfElement = null;
        ClearNamespaces();
        _ReadElementsStack.Clear();
    }

    public override RdfNode? ReadNext()
    {
        if (_ReadElementsStack.Count() == 0)
        {
            return null;
        }

        XElement content = _ReadElementsStack.Pop();

        string subjectId = GetXElementIdentifier(content, out bool isAuto);
        Uri subject = NameToUri(subjectId);

        Uri typeIdentifier = new Uri(content.Name.Namespace.NamespaceName
                + content.Name.LocalName);

        var rdfNode = new RdfNode(subject, typeIdentifier, isAuto);

        ///var triples = new List<RdfTriple>(content.Elements().Count());
        foreach (var child in content.Elements())
        {
            Uri predicate = new Uri(child.Name.Namespace.NamespaceName
                + child.Name.LocalName);

            // Blank node
            if (child.HasElements == false)
            {
                XAttribute? resource = child
                    .Attribute(rdf + "resource");

                object? @object;
                // Blank node
                if (resource != null)
                {
                    @object = NameToUri(resource.Value);
                }
                // Literal node
                else
                {
                    @object = child.Value;
                }

                rdfNode.NewTriple(predicate, @object);
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

                    rdfNode.NewTriple(predicate, subObject);
                }
            }
            else
            {
                throw new Exception("Cannot parse rdf node");
            }
        }

        return rdfNode;
    }

    public override IEnumerable<RdfNode> ReadAll()
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
    
    public override void Reset()
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
    /// Get rdf:RDF root node.
    /// </summary>
    /// <param name="content">Linq Xml document.</param>
    private void ReadRdfRootNode(XDocument content)
    {
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
    /// Get element identifier from rdf:about/id. Makes auto identifier in case of undentified element.
    /// </summary>
    /// <param name="element">Xml identified element.</param>
    /// <param name="isAuto">Set is auto identifier assigned.</param>
    private string GetXElementIdentifier(XElement element, out bool isAuto)
    {
        isAuto = false;
        XAttribute? about = element.Attribute(rdf + "about");
        if (about == null)
        {
            XAttribute? ID = element.Attribute(rdf + "ID");

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
