using CimBios.Core.RdfXmlIOLib;
using CimBios.Utils.ClassTraits;

namespace CimBios.Core.CimModel.Schema.RdfSchema;

/// <summary>
/// Cim schema supports RDFS format.
/// </summary>
public class CimRdfSchema : ICimSchema
{
    public ILogView Log => _Log;

    public IReadOnlyDictionary<string, Uri> Namespaces 
    {get => _Namespaces; }
    public IEnumerable<ICimMetaClass> Classes 
    { get => _All.Values.OfType<ICimMetaClass>(); }
    public IEnumerable<ICimMetaProperty> Properties 
    { get => _All.Values.OfType<ICimMetaProperty>(); }
    public IEnumerable<ICimMetaIndividual> Individuals 
    { get => _All.Values.OfType<ICimMetaIndividual>(); }
    public IEnumerable<ICimMetaDatatype> Datatypes 
    { get => _All.Values.OfType<ICimMetaDatatype>(); }

    public CimRdfSchema()
    {
        _Log = new PlainLogView(this);

        _All = new Dictionary<Uri, ICimMetaResource>(new RdfUriComparer());
        _Namespaces = new Dictionary<string, Uri>();
    }

    public void Load(TextReader textReader)
    {
        var serizalizer = new CimRdfSchemaSerializer();
        serizalizer.Load(textReader);

        _All = serizalizer.Deserialize();
        _Namespaces = serizalizer.Namespaces;

        var details = string.Empty;
        if (_Namespaces.TryGetValue("base", out var baseUri))
        {
            details = baseUri.AbsoluteUri;
        }

        _Log.NewMessage(
            "Schema: Rdf schema has been loaded.",
            LogMessageSeverity.Info,
            details
        );
    }

    public IEnumerable<ICimMetaProperty> GetClassProperties(
        ICimMetaClass metaClass,
        bool inherit = false,
        bool extensions = true)
    {
        var result = new List<ICimMetaProperty>();

        ICimMetaClass? nextClass = metaClass;

        do
        {
            foreach (var prop in Properties
                .Where(p => 
                    RdfXmlReaderUtils.RdfUriEquals
                    (p.OwnerClass?.BaseUri, nextClass.BaseUri)))
            {
                result.Add(prop);
            }

            if (extensions == true)
            {         
                foreach (var extClass in nextClass.Extensions)
                {
                    result.AddRange(GetClassProperties(extClass, false, false));
                }
            }

            nextClass = nextClass?.ParentClass;
        }
        while (inherit == true && nextClass != null);

        return result;
    }

    public IEnumerable<ICimMetaIndividual> GetClassIndividuals(
        ICimMetaClass metaClass,
        bool inherit = false)
    {
        foreach (var individual in Individuals)
        {
            if (individual.InstanceOf == null)
            {
                continue;
            }

            if (RdfXmlReaderUtils.RdfUriEquals(
                individual.InstanceOf.BaseUri,
                metaClass.BaseUri))
            {
                yield return individual;
            }

            if (inherit == true)
            {
                if (individual.InstanceOf
                    .AllAncestors.Any(c => 
                        RdfXmlReaderUtils.RdfUriEquals(c.BaseUri, 
                        metaClass.BaseUri)))
                {
                    yield return individual;
                }
            }
        }
    }

    public T? TryGetDescription<T>(Uri uri) where T : ICimMetaResource
    {
        if (_All.TryGetValue(uri, out var metaDescription)
            && metaDescription is T meta)
        {
            return meta;
        }

        return default;
    }

    public bool HasUri(Uri uri)
    {
        return _All.ContainsKey(uri);
    }

    public bool CanCreateClass(ICimMetaClass metaClass)
    {
        if (metaClass.IsAbstract || metaClass.IsDatatype || metaClass.IsEnum)
        {
            return false;
        }

        if (metaClass.IsExtension)
        {
            var extendedBy = Classes.Where(
                c => c.Extensions.Contains(metaClass));
                
            if (extendedBy.Any(c => c.BaseUri != metaClass.BaseUri))
            {
                return false;
            }
        }

        return true;
    }

    public void Join(ICimSchema schema, bool rewriteNamespaces = false)
    {
        var details = string.Empty;
        if (schema.Namespaces.TryGetValue("base", out var baseUri))
        {
            details = baseUri.AbsoluteUri;
        }

        _Log.NewMessage(
            "Schema: Rdf schemas are joining.",
            LogMessageSeverity.Info,
            details
        );

        JoinNamespaces(schema.Namespaces, rewriteNamespaces);

        foreach (var metaClass in schema.Classes)
        {
            if (_All.ContainsKey(metaClass.BaseUri) == false)
            {
                _All.Add(metaClass.BaseUri, metaClass);
            }
        }

        foreach (var metaProperty in schema.Properties)
        {
            if (_All.ContainsKey(metaProperty.BaseUri) == false)
            {
                _All.Add(metaProperty.BaseUri, metaProperty);
            }
        }

        foreach (var metaDatatype in schema.Datatypes)
        {
            if (_All.ContainsKey(metaDatatype.BaseUri) == false)
            {
                _All.Add(metaDatatype.BaseUri, metaDatatype);
            }
        }       

        foreach (var metaInstance in schema.Individuals)
        {
            if (_All.ContainsKey(metaInstance.BaseUri) == false)
            {
                _All.Add(metaInstance.BaseUri, metaInstance);
            }
        }   
    }

    public string GetUriNamespacePrefix(Uri uri)
    {
        foreach (var ns in Namespaces)
        {
            if (ns.Value.AbsolutePath == uri.AbsolutePath)
            {
                return ns.Key;
            }
        }

        return "_";
    }

    private void JoinNamespaces(IReadOnlyDictionary<string, Uri> namespaces, 
        bool rewriteNamespaces)
    {
        foreach (var ns in namespaces)
        {
            if (_Namespaces.ContainsKey(ns.Key)
                && rewriteNamespaces == true)
            {
                _Namespaces[ns.Key] = ns.Value;
            }
            else
            {
                _Namespaces.TryAdd(ns.Key, ns.Value);
            }
        }
    }

    private Dictionary<Uri, ICimMetaResource> _All;

    private Dictionary<string, Uri> _Namespaces;

    private PlainLogView _Log;
}

public class CimRdfSchemaFactory : ICimSchemaFactory
{
    public ICimSchema CreateSchema()
    {
        return new CimRdfSchema();
    }
}