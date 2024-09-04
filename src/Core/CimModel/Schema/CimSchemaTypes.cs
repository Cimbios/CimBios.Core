namespace CimBios.Core.CimModel.Schema;

/// <summary>
/// Pre-defined mapping of XML types to system types.
/// </summary>
internal static class XmlDatatypesMapping
{
    internal static readonly Dictionary<string, System.Type> UriSystemTypes
        = new Dictionary<string, System.Type>()
        {
            { "http://www.w3.org/2001/XMLSchema#string", typeof(string) },
            { "http://www.w3.org/2001/XMLSchema#boolean", typeof(bool) },
            { "http://www.w3.org/2001/XMLSchema#decimal", typeof(decimal) },
            { "http://www.w3.org/2001/XMLSchema#float", typeof(float) },
            { "http://www.w3.org/2001/XMLSchema#double", typeof(double) },
            { "http://www.w3.org/2001/XMLSchema#dateTime", typeof(DateTime) },
            { "http://www.w3.org/2001/XMLSchema#time", typeof(DateTime) },
            { "http://www.w3.org/2001/XMLSchema#date", typeof(DateTime) },
            { "http://www.w3.org/2001/XMLSchema#anyURI", typeof(Uri) },
        };
}

/// <summary>
/// Custom attribute provides necessary serialization data. 
/// </summary>
internal class CimSchemaSerializableAttribute : Attribute
{
    public string AbsoluteUri { get; }
    public MetaFieldType FieldType { get; }
    public bool IsCollection { get; }

    public CimSchemaSerializableAttribute(string uri)
    {
        AbsoluteUri = uri;
    }

    public CimSchemaSerializableAttribute(string uri,
        MetaFieldType fieldType,
        bool isCollection = false) : this(uri)
    {
        FieldType = fieldType;
        IsCollection = isCollection;
    }
}
