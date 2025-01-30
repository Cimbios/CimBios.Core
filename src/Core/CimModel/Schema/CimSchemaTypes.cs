using CimBios.Utils.MetaReflectionHelper;

namespace CimBios.Core.CimModel.Schema;

/// <summary>
/// Pre-defined mapping of XML types to system types.
/// </summary>
internal static class XmlDatatypesMapping
{
    internal static readonly Dictionary<string, System.Type> UriSystemTypes
        = new Dictionary<string, System.Type>()
        {
            { IntegerUri, typeof(int) },
            { StringUri, typeof(string) },
            { BooleanUri, typeof(bool) },
            { DecimalUri, typeof(decimal) },
            { FloatUri, typeof(float) },
            { DoubleUri, typeof(double) },
            { DateTimeUri, typeof(DateTime) },
            { TimeUri, typeof(DateTime) },
            { DateUri, typeof(DateTime) },
            { AnyURIUri, typeof(Uri) },
        };

    internal const string IntegerUri = "http://www.w3.org/2001/XMLSchema#integer";
    internal const string StringUri = "http://www.w3.org/2001/XMLSchema#string";
    internal const string BooleanUri = "http://www.w3.org/2001/XMLSchema#boolean";
    internal const string DecimalUri = "http://www.w3.org/2001/XMLSchema#decimal";
    internal const string FloatUri = "http://www.w3.org/2001/XMLSchema#float";
    internal const string DoubleUri = "http://www.w3.org/2001/XMLSchema#double";
    internal const string DateTimeUri = "http://www.w3.org/2001/XMLSchema#dateTime";
    internal const string TimeUri = "http://www.w3.org/2001/XMLSchema#time";
    internal const string DateUri = "http://www.w3.org/2001/XMLSchema#date";
    internal const string AnyURIUri = "http://www.w3.org/2001/XMLSchema#anyURI";
}

/// <summary>
/// Custom attribute provides necessary serialization data. 
/// </summary>
internal class CimSchemaSerializableAttribute : MetaTypeAttribute
{
    public MetaFieldType FieldType { get; }
    public bool IsCollection { get; }

    public CimSchemaSerializableAttribute(string uri) 
        : base(uri) { }

    public CimSchemaSerializableAttribute(string uri,
        MetaFieldType fieldType,
        bool isCollection = false) : this(uri)
    {
        FieldType = fieldType;
        IsCollection = isCollection;
    }
}

/// <summary>
/// Member serialization types.
/// </summary>
internal enum MetaFieldType
{
    /// <summary>
    /// XML string node value.
    /// </summary>
    Value,
    /// <summary>
    /// Enumeration according schema individuals. 
    /// </summary>
    Enum,
    /// <summary>
    /// Schema RDF description based instance by URI reference.
    /// </summary>
    ByRef,
    /// <summary>
    /// Schema RDF datatype.
    /// </summary>
    Datatype,
}

/// <summary>
/// Root interface for any schema entity.
/// </summary>
public interface ICimMetaResource : IEquatable<ICimMetaResource>
{
    public Uri BaseUri { get; }
    public string ShortName { get; }
    public string Description { get; }
    public int GetHashCode();
}

/// <summary>
/// Meta cim class info.
/// </summary>
public interface ICimMetaClass : ICimMetaResource
{
    public bool SuperClass { get; }
    public ICimMetaClass? ParentClass { get; set; }
    public IEnumerable<ICimMetaClass> AllAncestors { get; }
    public IEnumerable<ICimMetaClass> Extensions { get; }
    public IEnumerable<ICimMetaProperty> AllProperties { get; }
    public IEnumerable<ICimMetaProperty> SelfProperties { get; }
    public IEnumerable<ICimMetaIndividual> AllIndividuals { get; }
    public IEnumerable<ICimMetaIndividual> SelfIndividuals { get; }
    public bool IsAbstract { get; }
    public bool IsExtension { get; }
    public bool IsEnum { get; }
    public bool IsCompound { get; }
    public bool IsDatatype { get; }

    public bool HasProperty(ICimMetaProperty metaProperty, bool inherit = true);
}

/// <summary>
/// Provides schema extensibility for meta resource.
/// </summary>
internal interface ICimMetaExtensible
{
    /// <summary>
    /// Add extension class to meta resource.
    /// </summary>
    /// <param name="extension">Meta extension class</param>
    /// <returns>True if added.</returns>
    public bool AddExtension(ICimMetaClass extension);

    /// <summary>
    /// Remove extension class from meta resource.
    /// </summary>
    /// <param name="extension"Meta extension class></param>
    /// <returns>True if removed.</returns>
    public bool RemoveExtension(ICimMetaClass extension);  

    /// <summary>
    /// 
    /// </summary>
    /// <param name="metaProperty"></param>
    public void AddProperty(ICimMetaProperty metaProperty);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="metaProperty"></param>
    public void RemoveProperty(ICimMetaProperty metaProperty);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="metaIndividual"></param>
    public void AddIndividual(ICimMetaIndividual metaIndividual);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="metaIndividual"></param>
    public void RemoveIndividual(ICimMetaIndividual metaIndividual);
}

/// <summary>
/// Meta cim property info.
/// </summary>
public interface ICimMetaProperty : ICimMetaResource
{   
    public ICimMetaClass? OwnerClass { get; }
    public CimMetaPropertyKind PropertyKind { get; }
    public ICimMetaProperty? InverseProperty { get; }
    public ICimMetaClass? PropertyDatatype { get; }
    public bool IsExtension { get; }
    public bool IsValueRequired { get; }
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
