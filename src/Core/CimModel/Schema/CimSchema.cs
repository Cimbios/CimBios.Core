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
    public IEnumerable<ICimMetaClass> Extensions 
        => GetExtensions();
    public IEnumerable<ICimMetaProperty> Properties 
        => _All.Values.OfType<ICimMetaProperty>();
    public IEnumerable<ICimMetaIndividual> Individuals 
        => _All.Values.OfType<ICimMetaIndividual>(); 
    public IEnumerable<ICimMetaDatatype> Datatypes 
        => _All.Values.OfType<ICimMetaDatatype>();

    public bool TieSameNameEnums { get; set; } = true;

    private ICimSchemaSerializer? _Serializer { get; set; }

    public CimSchema()
    {   
        _Log = new PlainLogView(this);

        _All = [];
        _Namespaces = [];
    }

    public CimSchema(ICimSchemaSerializerFactory serializerFactory)
        : this()
    {
        _Serializer = serializerFactory.CreateSerializer();
    }

    public void Load(TextReader textReader)
    {
        if (_Serializer == null)
        {
            _Log.NewMessage("Schema serializer has not been initialized", 
                LogMessageSeverity.Error);

            return;
        }

        _Serializer.Load(textReader);

        _All = _Serializer.Deserialize();
        _Namespaces = _Serializer.Namespaces.ToDictionary();

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

    public void Load(TextReader textReader, 
        ICimSchemaSerializerFactory serializerFactory)
    {
        _Serializer = serializerFactory.CreateSerializer();
        Load(textReader);
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
            foreach (var prop in Properties.Where(p => 
                nextClass == p.OwnerClass))
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

            if (metaClass.Equals(individual.InstanceOf))
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

    public T? TryGetResource<T>(Uri uri) where T : ICimMetaResource
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
        JoinClasses(schema.Classes); 

        if (TieSameNameEnums)
        {
            TieEnumExtensions();
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

    public void InvalidateAuto()
    {
        
    }

    private HashSet<ICimMetaClass> GetExtensions()
    {
        var extensions = new HashSet<ICimMetaClass>();
        foreach (var metaClass in Classes)
        {
            foreach (var extension in metaClass.Extensions)
            {
                if (extensions.Contains(extension))
                {
                    continue;
                }
                
                extensions.Add(extension);
            }
        }

        return extensions;
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
                && baseEnum != enumClass
                && baseEnum is ICimMetaExtensible extensibleEnum)
            {
                extensibleEnum.AddExtension(enumClass);
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="classes"></param>
    private void JoinClasses(IEnumerable<ICimMetaClass> classes)
    {
        var addClassDelegate = (ICimMetaClass metaClass) => 
        {
            if (_All.ContainsKey(metaClass.BaseUri) == false)
            {
                if (metaClass.ParentClass != null)
                {
                    var parentLeftClass = TryGetResource<ICimMetaClass>
                        (metaClass.ParentClass.BaseUri);
                        
                    if (parentLeftClass != null)
                    {
                        metaClass.ParentClass = parentLeftClass;
                    }
                }
                
                _All.Add(metaClass.BaseUri, metaClass);
            }

            JoinProperties(metaClass.SelfProperties);
            JoinIndividuals(metaClass.SelfIndividuals);
        };

        foreach (var metaClass in classes)
        {
            var thisMetaClass = TryGetResource<ICimMetaClass>(metaClass.BaseUri);
            addClassDelegate(metaClass);

            if (thisMetaClass != null)
            {
                if (thisMetaClass.ParentClass == null 
                    && metaClass.ParentClass != null)
                {
                    addClassDelegate(metaClass.ParentClass);
                    thisMetaClass.ParentClass = metaClass.ParentClass;                    
                }

                foreach (var ext in metaClass.Extensions)
                {
                    if (thisMetaClass.Extensions.Contains(ext) == false
                        && thisMetaClass is ICimMetaExtensible extClass)
                    {
                        addClassDelegate(ext);
                        if (ext != null)
                        {
                            extClass.AddExtension(ext);
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="properties"></param>
    private void JoinProperties(IEnumerable<ICimMetaProperty> properties)
    {
        foreach (var metaProperty in properties.ToArray())
        {
            if (_All.ContainsKey(metaProperty.BaseUri) == false)
            {
                _All.Add(metaProperty.BaseUri, metaProperty);

                if (metaProperty.OwnerClass != null)
                {
                    var ownerLeftClass = TryGetResource<ICimMetaClass>
                        (metaProperty.OwnerClass.BaseUri); 

                    if (ownerLeftClass is ICimMetaExtensible extClass)
                    {
                        extClass.RemoveProperty(metaProperty);
                        extClass.AddProperty(metaProperty);
                    }
                }   
            }
        }  
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="properties"></param>
    private void JoinIndividuals(IEnumerable<ICimMetaIndividual> individuals)
    {
        foreach (var metaIndividual in individuals.ToArray())
        {
            if (_All.ContainsKey(metaIndividual.BaseUri) == false)
            {
                _All.Add(metaIndividual.BaseUri, metaIndividual);

                if (metaIndividual.InstanceOf != null)
                {
                    var ownerLeftClass = TryGetResource<ICimMetaClass>
                        (metaIndividual.InstanceOf.BaseUri); 

                    if (ownerLeftClass is ICimMetaExtensible extClass)
                    {
                        extClass.AddIndividual(metaIndividual);
                    }
                }
            }
        }  
    }

    private Dictionary<Uri, ICimMetaResource> _All;

    private Dictionary<string, Uri> _Namespaces;

    private readonly PlainLogView _Log;
}
