namespace CimBios.Core.CimModel.Schema;

/// <summary>
/// Pre-defined mapping of XML types to system types.
/// </summary>
internal static class XmlDatatypesMapping
{
    internal static readonly Dictionary<string, System.Type> UriSystemTypes
        = new Dictionary<string, System.Type>()
        {
            { "http://www.w3.org/2001/XMLSchema#integer", typeof(int) },
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

/// <summary>
/// Root interface for any schema entity.
/// </summary>
public interface ICimMetaResource
{
    public Uri BaseUri { get; }
    public string ShortName { get; }
    public string Description { get; }
}

/// <summary>
/// Meta cim class info.
/// </summary>
public interface ICimMetaClass : ICimMetaResource
{
    public bool SuperClass { get; }
    public ICimMetaClass? ParentClass { get; }
    public ICimMetaClass[] AllAncestors { get; }
    public ICimMetaClass[] Extensions { get; }
    public bool IsAbstract { get; }
    public bool IsExtension { get; }
    public bool IsEnum { get; }
    public bool IsCompound { get; }
    public bool IsDatatype { get; }
}

/// <summary>
/// Meta cim property info.
/// </summary>
public interface ICimMetaProperty : ICimMetaResource
{   
    public ICimMetaClass? OwnerClass { get;  }
    public CimMetaPropertyKind PropertyKind { get; }
    public ICimMetaProperty? InverseProperty { get; }
    public ICimMetaClass? PropertyDatatype { get; }
    public bool IsExtension { get; }
}

/// <summary>
/// Provides system type information about cim entity.
/// </summary>
public interface ICimMetaDatatype : ICimMetaClass
{
    public System.Type? SystemType { get; }
    public System.Type PrimitiveType { get; }
}

/// <summary>
/// Meta cim instance object info. Commonly used for enum members.
/// </summary>
public interface ICimMetaIndividual : ICimMetaResource
{
    public ICimMetaClass? InstanceOf { get; }
}

/// <summary>
/// Cim property kind.
/// </summary>
public enum CimMetaPropertyKind
{
    NonStandard,
    Attribute,
    Assoc1To1,
    Assoc1ToM
}
