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

    private Dictionary<string, Uri> _Namespaces { get; set; } = [];

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
    protected (string prefix, string name) UriToName(Uri uri)
    {   
        var prefix = _Namespaces.Where(ns => ns.Value == uri)
            .Select(p => p.Key).FirstOrDefault();
            
        if (prefix == null)
        {
            throw new Exception("RdfXmlWriter.GetNameWithPrefix: no ns");
        }

        if (RdfUtils.TryGetEscapedIdentifier(uri, out var identifier))
        {
            return (prefix, identifier);
        }

        throw new Exception("RdfXmlWriter.GetNameWithPrefix: invalid rid");
    }

    protected const string xmlns = "http://www.w3.org/2000/xmlns/";
    protected const string xml = "http://www.w3.org/XML/1998/namespace";
    protected const string rdf = "http://www.w3.org/1999/02/22-rdf-syntax-ns#";
}
