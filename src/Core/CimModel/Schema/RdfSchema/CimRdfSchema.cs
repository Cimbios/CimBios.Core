using CimBios.Core.RdfXmlIOLib;

namespace CimBios.Core.CimModel.Schema.RdfSchema;

/// <summary>
/// Cim schema supports RDFS format.
/// </summary>
public class CimRdfSchema : ICimSchema
{
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
    }

    public void Load(TextReader textReader)
    {
        var serizalizer = new CimRdfSchemaSerializer();
        serizalizer.Load(textReader);

        _All = serizalizer.Deserialize();
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

    private Dictionary<Uri, ICimSchemaSerializable> _All;
}
