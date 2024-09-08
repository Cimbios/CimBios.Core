using CimBios.Core.RdfXmlIOLib;

namespace CimBios.Core.CimModel.Schema.RdfSchema;

/// <summary>
/// Cim schema supports RDFS format.
/// </summary>
public class CimRdfSchema : ICimSchema
{
    public IReadOnlyDictionary<string, Uri> Namespaces 
    {get => _Namespaces; }
    public IEnumerable<ICimMetaClass> Classes 
    { get => _All.Values.OfType<ICimMetaClass>(); }
    public IEnumerable<ICimMetaProperty> Properties 
    { get => _All.Values.OfType<ICimMetaProperty>(); }
    public IEnumerable<ICimMetaInstance> Individuals 
    { get => _All.Values.OfType<ICimMetaInstance>(); }
    public IEnumerable<ICimMetaDatatype> Datatypes 
    { get => _All.Values.OfType<ICimMetaDatatype>(); }

    public CimRdfSchema()
    {
        _All = new Dictionary<Uri, ICimSchemaSerializable>(new RdfUriComparer());
        _Namespaces = new Dictionary<string, Uri>();
    }

    public void Load(TextReader textReader)
    {
        var serizalizer = new CimRdfSchemaSerializer();
        serizalizer.Load(textReader);

        _All = serizalizer.Deserialize();
        _Namespaces = serizalizer.Namespaces;
    }

    public IEnumerable<ICimMetaProperty> GetClassProperties(
        ICimMetaClass metaClass,
        bool inherit = false)
    {
        ICimMetaClass? nextClass = metaClass;

        do
        {
            foreach (var prop in Properties
                .Where(p => p.OwnerClass == nextClass))
            {
                yield return prop;
            }

            nextClass = nextClass?.ParentClass;
        }
        while (inherit == true && nextClass != null);
    }

    public T? TryGetDescription<T>(Uri uri) where T : ICimSchemaSerializable
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

    public void Join(ICimSchema schema, bool rewriteNamespaces = false)
    {
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

    private Dictionary<Uri, ICimSchemaSerializable> _All;

    private Dictionary<string, Uri> _Namespaces;
}

public class CimRdfSchemaFactory : ICimSchemaFactory
{
    public ICimSchema CreateSchema()
    {
        return new CimRdfSchema();
    }
}