using System.Xml.Linq;

namespace CimBios.RdfXml.IOLib
{
    /// <summary>
    /// Flat structured RDF node class.
    /// </summary>
    public class RdfNode
    {
        private HashSet<RdfNode> _Children;
        private RdfNode? _ParentNode;

        public RdfNode(Uri identifier, Uri typeIdentifier,
            RdfTriple[] triples, bool isAuto)
        {
            Identifier = identifier;
            TypeIdentifier = typeIdentifier;
            Triples = triples;
            IsAuto = isAuto;

            _Children = new HashSet<RdfNode>();
        }

        public Uri Identifier { get; set; }
        public Uri TypeIdentifier { get; set; }
        public RdfTriple[] Triples { get; set; }
        public bool IsAuto { get; set; } = false;
        public RdfNode? Parent
        {
            get => _ParentNode;
            set
            {
                if (_ParentNode == value)
                {
                    return;
                }

                _ParentNode = value;

                if (value == null)
                {
                    _ParentNode?.RemoveChild(this);
                }
                else
                {
                    value.AddChild(this);
                }
            }
        }
        public RdfNode[] Children { get => _Children.ToArray(); }

        public bool AddChild(RdfNode rdfNode)
        {
            rdfNode.Parent = this;
            return _Children.Add(rdfNode);
        }

        public bool RemoveChild(RdfNode rdfNode)
        {
            rdfNode.Parent = null;
            return _Children.Remove(rdfNode);
        }

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
        public static bool RdfUriEquals(Uri lUri, Uri rUri)
        {
            return lUri.AbsoluteUri == rUri.AbsoluteUri;
        }
    }

    public class RdfUriComparer : EqualityComparer<Uri>
    {
        public override bool Equals(Uri? lUri, Uri? rUri)
        {
            if (lUri != null && rUri != null)
            {
                return RdfXmlReaderUtils.RdfUriEquals(lUri, rUri);
            }

            if (lUri == null && rUri == null)
            {
                return true;
            }

            return false;
        }

        public override int GetHashCode(Uri uri)
        {
            return uri.AbsoluteUri.GetHashCode();
        }
    }
}
