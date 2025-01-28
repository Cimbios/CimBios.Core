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
    public RdfTriple NewTriple(Uri predicate, 
        RdfTripleObjectContainerBase @object)
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
    public void RemoveTriple(Uri predicate, 
        RdfTripleObjectContainerBase @object)
    {
        _Triples.RemoveAll(m => RdfUtils.RdfUriEquals(m.Predicate, predicate) 
            && m.Object ==  @object);
    }

    private readonly List<RdfTriple> _Triples = [];
}

/// <summary>
/// RDF-Triple class.
/// </summary>
public class RdfTriple
{
    public RdfTriple(RdfNode subject, Uri predicate,
        RdfTripleObjectContainerBase @object)
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

    public RdfTripleObjectContainerBase Object { get; set; }
}

/// <summary>
/// Containered value object for RdfTriple instance.
/// </summary>
public abstract class RdfTripleObjectContainerBase
{
    public System.Type Type { get; private set; }
    public object RawObject { get; private set; }

    internal RdfTripleObjectContainerBase(object rawObject)
    {
        Type = rawObject.GetType();
        RawObject = rawObject;
    }
}

public sealed class RdfTripleObjectLiteralContainer 
    : RdfTripleObjectContainerBase
{
    public string LiteralObject
    {
        get
        {
            if (RawObject is not string literalObject)
            {
                throw new InvalidCastException("RdfTripleObjectLiteralContainer does not contain string object!");
            }

            return literalObject;
        }
    }

    public RdfTripleObjectLiteralContainer(string literalObject)
        : base(literalObject)
    {
    }
}

public sealed class RdfTripleObjectUriContainer 
    : RdfTripleObjectContainerBase
{
    public Uri UriObject
    {
        get
        {
            if (RawObject is not Uri uriObject)
            {
                throw new InvalidCastException("RdfTripleObjectUriContainer does not contain URI object!");
            }

            return uriObject;
        }
    }

    public RdfTripleObjectUriContainer(Uri uriObject)
        : base(uriObject)
    {
    }
}

public sealed class RdfTripleObjectStatementsContainer 
    : RdfTripleObjectContainerBase
{
    public ICollection<RdfNode> RdfNodesObject
    {
        get
        {
            if (RawObject is not ICollection<RdfNode> rdfNodes)
            {
                throw new InvalidCastException("RdfTripleObjectStatementsContainer does not contain ICollection<RdfNode> object!");
            }

            return rdfNodes;
        }
    }

    public RdfTripleObjectStatementsContainer(ICollection<RdfNode> rdfNodes)
        : base(rdfNodes)
    {
    }
}
