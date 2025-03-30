namespace CimBios.Core.RdfIOLib;

/// <summary>
/// Helper class for rdf entities.
/// </summary>
public static class RdfUtils
{
    public static T? ExtractPredicateValue<T>(RdfNode node,
        Uri predicate) where T : RdfTripleObjectContainerBase
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
        
        var allowedNamespaces = new HashSet<string>()
        {
            "urn", "base"
        };

        if (allowedNamespaces.Contains(uri.Scheme))
        {
            identifier = uri.AbsoluteUri.Split(':').Last();
            return true;
        }
        else if (uri.Fragment != string.Empty)
        {
            identifier = uri.Fragment
                .Replace("#", "");

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

