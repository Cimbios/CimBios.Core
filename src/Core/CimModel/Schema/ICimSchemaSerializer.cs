using System.Xml.Linq;

namespace CimBios.Core.CimModel.Schema;

/// <summary>
/// Root interface cim schema serializer.
/// </summary>
public interface ICimSchemaSerializer
{
    public Dictionary <string, XNamespace> Namespaces { get; }
    public void Load(TextReader reader);
    public Dictionary<Uri, ICimSchemaSerialiable> Deserialize();
}

/// <summary>
/// Root interface for any schema entity.
/// </summary>
public interface ICimSchemaSerialiable
{
    public Uri? BaseUri { get; set; }
    public string ShortName { get; set; }
}

/// <summary>
/// Meta cim class info.
/// </summary>
public interface ICimMetaClass : ICimSchemaSerialiable
{
    public bool SuperClass { get; }
    public ICimMetaClass? ParentClass { get;  }
}

/// <summary>
/// Meta cim property info.
/// </summary>
public interface ICimMetaProperty : ICimSchemaSerialiable
{   
    public ICimMetaClass? OwnerClass { get;  }
    public CimMetaPropertyKind PropertyKind { get; }
    public ICimMetaProperty? InverseProperty { get; }
}

/// <summary>
/// Provides system type information about cim entity.
/// </summary>
public interface ICimMetaDatatype : ICimSchemaSerialiable
{
    public System.Type? SystemType { get; }
    public System.Type? SimpleType { get; }
}

/// <summary>
/// Meta cim instance object info. Commonly used for enum members.
/// </summary>
public interface ICimMetaInstance : ICimSchemaSerialiable
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
    Assoc1ToN
}
