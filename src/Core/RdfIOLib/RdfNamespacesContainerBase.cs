namespace CimBios.Core.RdfIOLib;

/// <summary>
///     Provides XNamespace based wrapper for RDF document.
/// </summary>
public abstract class RdfNamespacesContainerBase
{
    protected const string Xmlns = "http://www.w3.org/2000/xmlns/";
    protected const string Xml = "http://www.w3.org/XML/1998/namespace";
    protected const string Rdf = "http://www.w3.org/1999/02/22-rdf-syntax-ns#";

    /// <summary>
    ///     RDF document namespaces dictionary.
    /// </summary>
    public IReadOnlyDictionary<string, Uri> Namespaces
        => _namespaces.AsReadOnly();

    private readonly Dictionary<string, Uri> _namespaces = [];

    protected RdfNamespacesContainerBase()
    {
        ClearNamespaces();
    }
    
    /// <summary>
    ///     Add namespace.
    /// </summary>
    /// <param name="prefix">Namespace alias prefix.</param>
    /// <param name="ns">Uri namespace representation</param>
    /// <returns>True if namespace has not been exists and added.</returns>
    public bool AddNamespace(string prefix, Uri ns)
    {
        return _namespaces.TryAdd(prefix, ns);
    }

    /// <summary>
    ///     Remove namespace.
    /// </summary>
    /// <param name="prefix">Namespace alias prefix.</param>
    /// <returns>True if namespace found and removed.</returns>
    public bool RemoveNamespace(string prefix)
    {
        return prefix != "rdf" && _namespaces.Remove(prefix);
    }

    /// <summary>
    ///     Clear namespaces.
    /// </summary>
    public void ClearNamespaces()
    {
        _namespaces.Clear();
        
        _namespaces.TryAdd("rdf", new Uri(Rdf));
    }

    /// <summary>
    ///     Make Uri from string identifier.
    /// </summary>
    /// <param name="identifier">String local identifier.</param>
    /// <param name="ns">Namespace of identifier.</param>
    protected Uri NameToUri(string identifier, string ns = "base")
    {
        var splittedPrefix = identifier.Split(':');
        if (splittedPrefix.Length == 2
            && Namespaces.ContainsKey(splittedPrefix.First()))
            return
                new Uri(Namespaces[splittedPrefix.First()] + splittedPrefix.Last());

        if (Uri.IsWellFormedUriString(identifier, UriKind.Absolute)) return new Uri(identifier);

        return new Uri(Namespaces[ns] + identifier);
    }

    /// <summary>
    ///     Make string identifier from Uri.
    /// </summary>
    /// <param name="uri"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    protected (string prefix, string name) UriToName(Uri uri)
    {
        var prefix = _namespaces.Where(ns => ns.Value == uri)
            .Select(p => p.Key).FirstOrDefault();

        if (prefix == null)
            throw new Exception(
                $"RdfXmlWriter.GetNameWithPrefix: no ns prefix for {uri.AbsolutePath}");

        if (RdfUtils.TryGetEscapedIdentifier(uri, out var identifier)) return (prefix, identifier);

        throw new Exception("RdfXmlWriter.GetNameWithPrefix: invalid rid");
    }
}
