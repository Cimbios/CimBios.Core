using CimBios.Core.CimModel.Schema.AutoSchema;
using CimBios.Core.CimModel.Schema.RdfSchema;
using CimBios.Core.RdfIOLib;
using CimBios.Utils.ClassTraits.CanLog;

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

    public ICimMetaClass ResourceSuperClass => _ResourceSuperClass;

    private ICimSchemaSerializer? _Serializer { get; set; }

    public CimSchema()
    {   
        _Log = new PlainLogView(this);

        _All = [];
        _Namespaces = [];

        var resourceSuperClass = new CimAutoClass(
            CimRdfSchemaStrings.RdfsResource,
            "Resource", "Root rdfs:Resource meta instance.");
        resourceSuperClass.SetIsAbstract(true);
        _ResourceSuperClass = resourceSuperClass;   
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
            _Log.Error("Schema serializer has not been initialized", this);

            return;
        }

        _Serializer.Load(textReader);

        _All = _Serializer.Deserialize();
        _All.Add(_ResourceSuperClass.BaseUri, _ResourceSuperClass);

        _All.Add(CimRdfSchemaStrings.RdfDescription, 
            new CimAutoClass(
                CimRdfSchemaStrings.RdfDescription, 
                "Description", 
                "rdf:Description meta instance.")
            );

        _Namespaces = _Serializer.Namespaces.ToDictionary();

        if (TieSameNameEnums)
        {
            TieEnumExtensions();
        }

        CreateSuperDescriptionClass();

        var details = string.Empty;
        if (_Namespaces.TryGetValue("base", out var baseUri))
        {
            details = baseUri.AbsoluteUri;
        }

        _Log.Info($"Schema has been loaded. Base = {details}", this);
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
        foreach (var metaClass in Classes)
        {
            foreach (var metaProperty in metaClass.SelfProperties)
            {
                if (TryGetResource<ICimMetaProperty>(
                    metaProperty.BaseUri) != null)
                {
                    continue;
                }

                if (metaClass is ICimMetaExtensible extClass)
                {
                    extClass.RemoveProperty(metaProperty);
                }
            }
        }
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

    private void CreateSuperDescriptionClass()
    {
        var extensionsCache = Extensions;
        foreach (var quasiSuper in Classes.Where(c => 
            c != ResourceSuperClass
            && c.SuperClass 
            && !c.IsEnum 
            && c is not ICimMetaDatatype
            && !extensionsCache.Contains(c)))
        {
            quasiSuper.ParentClass = ResourceSuperClass;
        }
    }

    private Dictionary<Uri, ICimMetaResource> _All;

    private Dictionary<string, Uri> _Namespaces;

    public ICimMetaClass _ResourceSuperClass;

    private readonly PlainLogView _Log;
}
