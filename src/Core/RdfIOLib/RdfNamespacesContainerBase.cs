using System.Collections.ObjectModel;
using System.Xml.Linq;

namespace CimBios.Core.RdfIOLib;

/// <summary>
/// Provides XNamespace based wrapper for RDF document.
/// </summary>
public abstract class RdfNamespacesContainerBase
{
    /// <summary>
    /// RDF document namespaces dictionary.
    /// </summary>
    public IReadOnlyDictionary<string, Uri> Namespaces 
        => _Namespaces.AsReadOnly();

    private Dictionary<string, Uri> _Namespaces { get; set; }
        = new Dictionary<string, Uri>();

    protected RdfNamespacesContainerBase() { }

    /// <summary>
    /// Add namespace.
    /// </summary>
    /// <param name="prefix">Namespace alias prefix.</param>
    /// <param name="ns">Uri namespace representation</param>
    /// <returns>True if namespace has not been exists and added.</returns>
    public bool AddNamespace(string prefix, Uri ns)
    {
        if (_Namespaces.ContainsKey(prefix))
        {
            return false;
        } 

        _Namespaces.Add(prefix, ns);
        return true;
    }

    /// <summary>
    /// Remove namespace.
    /// </summary>
    /// <param name="prefix">Namespace alias prefix.</param>
    /// <returns>True if namespace found and removed.</returns>
    public bool RemoveNamespace(string prefix)
    {
        return _Namespaces.Remove(prefix);
    }

    /// <summary>
    /// Clear namespaces.
    /// </summary>
    public void ClearNamespaces()
    {
        _Namespaces.Clear();
    }

    /// <summary>
    /// Parse all root namespaces.
    /// </summary>
    /// <param name="rdfNode">rdf:RDF root node.</param>
    protected void ParseXmlns(XElement rdfNode)
    {
        XNamespace xlmns = "http://www.w3.org/2000/xmlns/";
        XNamespace ns = "http://www.w3.org/XML/1998/namespace";

        foreach (XAttribute attr in rdfNode.Attributes())
        {
            if (attr.Name.Namespace != xlmns
                && attr.Name != (ns + "base"))
            {
                continue;
            }

            if (Namespaces.ContainsKey(attr.Name.LocalName))
            {
                _Namespaces[attr.Name.LocalName] = new Uri(attr.Value);
            }
            else
            {
                _Namespaces.Add(attr.Name.LocalName, new Uri(attr.Value));
            }
        }
    }

    /// <summary>
    /// Make Uri from string identifier.
    /// </summary>
    /// <param name="identifier">String local identifier.</param>
    /// <param name="ns">Namespace of identifier.</param>
    protected Uri NameToUri(string identifier, string ns = "base")
    {
        var splittedPrefix = identifier.Split(':');
        if (splittedPrefix.Count() == 2
            && Namespaces.ContainsKey(splittedPrefix.First()))
        {
            return 
            new(Namespaces[splittedPrefix.First()] + splittedPrefix.Last());
        }

        if (Uri.IsWellFormedUriString(identifier, UriKind.Absolute))
        {
            return new Uri(identifier);
        }

        return new Uri(Namespaces[ns] + identifier);
    }

    /// <summary>
    /// Make string identifier from Uri.
    /// </summary>
    /// <param name="uri"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    protected XName UriToXName(Uri uri)
    {        
        XNamespace ns = uri.AbsoluteUri[..(uri.AbsoluteUri.IndexOf('#') + 1)];

        if (_Namespaces.Values.Contains(uri) == false)
        {
            throw new Exception("RdfXmlWriter.GetNameWithPrefix: no ns");
        }

        if (RdfUtils.TryGetEscapedIdentifier(uri, out var identifier))
        {
            XName result = ns + identifier;
            return result;
        }

        throw new Exception("RdfXmlWriter.GetNameWithPrefix: invalid rid");
    }

    public static XNamespace rdf = "http://www.w3.org/1999/02/22-rdf-syntax-ns#";
}