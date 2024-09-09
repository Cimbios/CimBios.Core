using System.Xml.Linq;

namespace CimBios.Core.RdfXmlIOLib;

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
        Triples = triples;
        IsAuto = isAuto;
    }

    public Uri Identifier { get; set; }
    public Uri TypeIdentifier { get; set; }
    public RdfTriple[] Triples { get; set; }
    public bool IsAuto { get; set; } = false;
}

/// <summary>
/// RDF-Triple class.
/// </summary>
public class RdfTriple
{
    public RdfTriple(Uri subject, Uri predicate,
        object @object)
    {
        Subject = subject;
        Predicate = predicate;
        Object = @object;
    }

    public Uri Subject { get; set; }

    /// <summary>
    /// Uri type limited predicate.
    /// </summary>
    public Uri Predicate { get; set; }

    public object Object { get; set; }
}

/// <summary>
/// Helper class for rdf entities.
/// </summary>
public static class RdfXmlReaderUtils
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
            return RdfXmlReaderUtils.RdfUriEquals(lUri, rUri);
        }

        return false;
    }
}

public class RdfUriComparer : EqualityComparer<Uri>
{
    public override bool Equals(Uri? lUri, Uri? rUri)
    {
        return RdfXmlReaderUtils.RdfUriEquals(lUri, rUri);
    }

    public override int GetHashCode(Uri uri)
    {
        return uri.AbsoluteUri.GetHashCode();
    }
}

