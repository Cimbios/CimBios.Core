namespace CimBios.Core.CimModel.Schema.AutoSchema;

/// <summary>
/// Schema auto class entity. Does not provide inheritance chain - only plain.
/// </summary>
public class CimAutoClass(Uri baseUri, string shortName, string description) 
    :   CimMetaClassBase(baseUri, shortName, description), 
        ICimMetaClass, ICimMetaExtensible
{
    public void SetIsEnum(bool isEnum) => IsEnum = isEnum;
    public void SetIsCompound(bool isCompound) => IsCompound = isCompound;
    public void SetIsAbstract(bool isAbstract) => IsAbstract = isAbstract;
}

public class CimAutoProperty(Uri baseUri, string shortName, string description) 
    :   CimMetaPropertyBase(baseUri, shortName, description), 
        ICimMetaProperty
{
    public void SetPropertyKind(CimMetaPropertyKind propertyKind) 
        => PropertyKind = propertyKind;

    public void SetPropertyDatatype(ICimMetaClass? propertyDatatype) 
        => PropertyDatatype = propertyDatatype;    
}

public class CimAutoDatatype(Uri baseUri, string shortName, string description)  
    :   CimAutoClass(baseUri, shortName, description), 
        ICimMetaDatatype
{
    public Type? SystemType { get; set; }
    public Type PrimitiveType => SystemType ?? typeof(string);
}

public class CimAutoIndividual(Uri baseUri, string shortName, string description) 
    :   CimMetaIndividualBase(baseUri, shortName, description), 
        ICimMetaIndividual
{
}
