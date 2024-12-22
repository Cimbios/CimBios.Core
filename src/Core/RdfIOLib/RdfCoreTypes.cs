namespace CimBios.Core.RdfIOLib;

/// <summary>
/// Flat structured RDF node class.
/// </summary>
public class RdfNode
{
    public RdfNode(Uri identifier, Uri typeIdentifier,
        RdfTriple[] triples, bool isAuto)
    {
        Identifier = identifier;
        TypeIdentifier = typeIdentifier;
        _Triples.AddRange(triples);
        IsAuto = isAuto;
    }

    public RdfNode(Uri identifier, Uri typeIdentifier, bool isAuto)
    {
        Identifier = identifier;
        TypeIdentifier = typeIdentifier;
        IsAuto = isAuto;
    }

    /// <summary>
    /// Rdf resource identifier.
    /// </summary>
    public Uri Identifier { get; }

    /// <summary>
    /// Rdf resource node identifier.
    /// </summary>
    public Uri TypeIdentifier { get; }

    /// <summary>
    /// Rdf node's triples collection.
    /// </summary>
    public RdfTriple[] Triples => [.. _Triples];

    /// <summary>
    /// Is auto (not identified rdf node) flag.
    /// </summary>
    public bool IsAuto { get; } = false;

    /// <summary>
    /// Create and add RdfTriple triple with RdfNode subject
    /// </summary>
    /// <param name="predicate">Triple predicate URI.</param>
    /// <param name="object">Triple generic object.</param>
    /// <returns>Created rdf node.</returns>
    public RdfTriple NewTriple(Uri predicate, object @object)
    {
        var triple = new RdfTriple(this, predicate, @object);
        _Triples.Add(triple);

        return triple;
    }

    /// <summary>
    /// Remove all triples with predicate URI.
    /// </summary>
    /// <param name="predicate">Triple predicate URI.</param>
    public void RemoveTriple(Uri predicate)
    {
        _Triples.RemoveAll(m => RdfUtils.RdfUriEquals(m.Predicate, predicate));
    }

    /// <summary>
    /// Remove all triples with predicate URI and matched object.
    /// </summary>
    /// <param name="predicate">Triple predicate URI.</param>
    /// <param name="object">Triple generic object.</param>
    public void RemoveTriple(Uri predicate, object @object)
    {
        _Triples.RemoveAll(m => RdfUtils.RdfUriEquals(m.Predicate, predicate) 
            && m.Object ==  @object);
    }

    private List<RdfTriple> _Triples = [];
}

/// <summary>
/// RDF-Triple class.
/// </summary>
public class RdfTriple
{
    public RdfTriple(RdfNode subject, Uri predicate,
        object @object)
    {
        Subject = subject;
        Predicate = predicate;
        Object = @object;
    }

    public RdfNode Subject { get; set; }

    /// <summary>
    /// Uri type limited predicate.
    /// </summary>
    public Uri Predicate { get; set; }

    public object Object { get; set; }
}

/// <summary>
/// Helper class for rdf entities.
/// </summary>
public static class RdfUtils
{
    public static T? ExtractPredicateValue<T>(RdfNode node,
        Uri predicate) where T : class
    {
        var triples = node.Triples
            .Where(t => RdfUriEquals(t.Predicate, predicate));
        if (triples.Count() > 0 && triples.First().Object is T value)
        {
            return value;
        }
        else
        {
            return null;
        }
    }

    /// <summary>
    /// Equality respects URI fragments comparision.
    /// </summary>
    public static bool RdfUriEquals(Uri? lUri, Uri? rUri)
    {
        if (lUri == null && rUri == null)
        {
            return true;
        }

         if (lUri != null && rUri != null)
        {
            return lUri.AbsoluteUri == rUri.AbsoluteUri;
        }

        return false;
    }

    /// <summary>
    /// Get short string form of URI.
    /// </summary>
    /// <param name="uri">Resource identifier.</param>
    /// <param name="identifier">Escaped identifier. Empty if conversion fails.</param>
    /// <returns>True if identifier</returns>
    public static bool TryGetEscapedIdentifier(Uri uri, out string identifier)
    {
        identifier = string.Empty;

        if (uri.Fragment != string.Empty)
        {
            identifier = uri.Fragment
                .Replace("#", "")
                .Replace("_", "");

            return true;
        }
        else if (uri.LocalPath != string.Empty)
        {
            identifier = uri.LocalPath.Replace("/", "");
            return true;
        }

        return false;
    }
}

public class RdfUriComparer : EqualityComparer<Uri>
{
    public override bool Equals(Uri? lUri, Uri? rUri)
    {
        return RdfUtils.RdfUriEquals(lUri, rUri);
    }

    public override int GetHashCode(Uri uri)
    {
        return uri.AbsoluteUri.GetHashCode();
    }
}

