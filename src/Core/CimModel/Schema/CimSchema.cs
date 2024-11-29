using CimBios.Core.RdfIOLib;
using CimBios.Utils.ClassTraits;

namespace CimBios.Core.CimModel.Schema;

public class CimSchema : ICimSchema
{
    public ILogView Log => _Log;

    public IReadOnlyDictionary<string, Uri> Namespaces 
        => _Namespaces;
    public IEnumerable<ICimMetaClass> Classes 
        => _All.Values.OfType<ICimMetaClass>();
    public IEnumerable<ICimMetaProperty> Properties 
        => _All.Values.OfType<ICimMetaProperty>();
    public IEnumerable<ICimMetaIndividual> Individuals 
        => _All.Values.OfType<ICimMetaIndividual>(); 
    public IEnumerable<ICimMetaDatatype> Datatypes 
        => _All.Values.OfType<ICimMetaDatatype>();

    public ICimSchemaSerializer? Serializer { get; set; }

    public bool TieSameNameEnums { get; set; } = true;

    public CimSchema()
    {   
        _Log = new PlainLogView(this);

        _All = new Dictionary<Uri, ICimMetaResource>(new RdfUriComparer());
        _Namespaces = [];
    }

    public CimSchema(ICimSchemaSerializer serializer)
        : this()
    {
        Serializer = serializer;
    }

    public void Load(TextReader textReader)
    {
        if (Serializer == null)
        {
            _Log.NewMessage("Schema serializer has not been initialized", 
                LogMessageSeverity.Error);

            return;
        }

        Serializer.Load(textReader);

        _All = Serializer.Deserialize();
        _Namespaces = Serializer.Namespaces.ToDictionary();

        if (TieSameNameEnums)
        {
            TieEnumExtensions();
        }

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
                    RdfUtils.RdfUriEquals
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
        bool extensions = true)
    {
        var result = new List<ICimMetaIndividual>();

        foreach (var individual in Individuals)
        {
            if (individual.InstanceOf == null)
            {
                continue;
            }

            if (RdfUtils.RdfUriEquals(
                individual.InstanceOf.BaseUri,
                metaClass.BaseUri))
            {
                result.Add(individual);
            }
        }

        if (extensions == true)
        {
            foreach (var ext in metaClass.Extensions)
            {
                result.AddRange(GetClassIndividuals(ext, false));
            }
        }

        return result;
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

    /// <summary>
    /// Tie the same name enum instances through extension link.
    /// </summary>
    private void TieEnumExtensions()
    {
        var enumProperties = _All.Values.OfType<ICimMetaProperty>()
            .Where(o => o.PropertyDatatype?.IsEnum == true);

        var enumsMap = new Dictionary<string, ICimMetaClass>();
        foreach (var property in enumProperties)
        {
            if (property.PropertyDatatype is not ICimMetaClass enumClass)
            {
                continue;
            }

            if (RdfUtils.TryGetEscapedIdentifier(enumClass.BaseUri,
                out var enumName) == false)
            {
                continue;
            }

            enumsMap.TryAdd(enumName, enumClass);
        }

        var enums = _All.Values.OfType<ICimMetaClass>()
            .Where(o => o.IsEnum == true);

        foreach (var enumClass in enums)
        {
            if (RdfUtils.TryGetEscapedIdentifier(enumClass.BaseUri,
                out var enumName) == false)
            {
                continue;
            }

            if (enumsMap.TryGetValue(enumName, out var baseEnum)
                && baseEnum != enumClass)
            {
                baseEnum.AddExtension(enumClass);
            }
        }
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
