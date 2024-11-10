namespace CimBios.Core.CimModel.Schema;

/// <summary>
/// Root interface cim schema serializer.
/// </summary>
public interface ICimSchemaSerializer
{
    /// <summary>
    /// Prefix to namespace URI mapping for schema.
    /// </summary>
    public Dictionary <string, Uri> Namespaces { get; }

    /// <summary>
    /// Load raw schema text data.
    /// </summary>
    public void Load(TextReader reader);

    /// <summary>
    /// Deserialize data to CIM schema.
    /// </summary>
    public Dictionary<Uri, ICimMetaResource> Deserialize();
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
    public System.Type SimpleType { get; }
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
