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
    public Dictionary<Uri, ICimSchemaSerializable> Deserialize();
}

/// <summary>
/// Root interface for any schema entity.
/// </summary>
public interface ICimSchemaSerializable
{
    public Uri BaseUri { get; }
    public string ShortName { get; }
}

/// <summary>
/// Meta cim class info.
/// </summary>
public interface ICimMetaClass : ICimSchemaSerializable
{
    public bool SuperClass { get; }
    public ICimMetaClass? ParentClass { get; }
    public ICimMetaClass[] AllAncestors { get; }
    public bool IsEnum { get; }
    public bool IsCompound { get; }
}

/// <summary>
/// Meta cim property info.
/// </summary>
public interface ICimMetaProperty : ICimSchemaSerializable
{   
    public ICimMetaClass? OwnerClass { get;  }
    public CimMetaPropertyKind PropertyKind { get; }
    public ICimMetaProperty? InverseProperty { get; }
    public ICimSchemaSerializable? PropertyDatatype { get; }
}

/// <summary>
/// Provides system type information about cim entity.
/// </summary>
public interface ICimMetaDatatype : ICimSchemaSerializable
{
    public System.Type? SystemType { get; }
    public System.Type SimpleType { get; }
}

/// <summary>
/// Meta cim instance object info. Commonly used for enum members.
/// </summary>
public interface ICimMetaInstance : ICimSchemaSerializable
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
