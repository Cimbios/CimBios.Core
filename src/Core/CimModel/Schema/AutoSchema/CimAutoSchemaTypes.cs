namespace CimBios.Core.CimModel.Schema.AutoSchema;

public class CimAutoResource : ICimMetaResource
{
    public required Uri BaseUri { get; set; }

    public required string ShortName { get; set; }

    public required string Description { get; set; }
}

/// <summary>
/// Schema auto class entity. Does not provide inheritance chain - only plain.
/// </summary>
public class CimAutoClass : CimAutoResource, ICimMetaClass
{
    public bool SuperClass => (ParentClass == null);

    public ICimMetaClass? ParentClass 
    { 
        get => _ParentClass;
        set
        {
            if (_ParentClass == value)
            {
                return;
            }

            if (_ParentClass != null)
            {
                _PlainAncestors.Remove(_ParentClass);
            }

            _ParentClass = value as CimAutoClass;

            if (_ParentClass != null)
            {
                _PlainAncestors.Add(_ParentClass);
            }
        }
    }

    public ICimMetaClass[] AllAncestors => [.. _PlainAncestors];

    public ICimMetaClass[] Extensions => [.. _PlainAncestors];

    public bool IsAbstract => false;

    public bool IsExtension { get; set; } = false;

    public bool IsEnum { get; set; }

    public bool IsCompound { get; set; }

    public bool IsDatatype => false;

    public bool AddExtension(ICimMetaClass metaClass)
    {
        if (_PlainAncestors.Contains(metaClass))
        {
            return false;
        }

        _PlainAncestors.Add(metaClass);

        (metaClass as CimAutoClass)!.IsExtension = true;

        return true;
    }

    public bool RemoveExtension(ICimMetaClass metaClass)
    {
        return _PlainAncestors.Remove(metaClass);
    }

    private readonly List<ICimMetaClass> _PlainAncestors = [];

    private CimAutoClass? _ParentClass;
}

public class CimAutoProperty : CimAutoResource, ICimMetaProperty
{
    public ICimMetaClass? OwnerClass { get; set; }

    public CimMetaPropertyKind PropertyKind { get; set; }

    public ICimMetaProperty? InverseProperty { get; set; }

    public ICimMetaClass? PropertyDatatype { get; set; }

    public bool IsExtension => false; 
}

public class CimAutoDatatype : CimAutoClass, ICimMetaDatatype
{
    public Type? SystemType { get; set; }

    public Type PrimitiveType => SystemType ?? typeof(string);
}

public class CimAutoIndividual : CimAutoResource, ICimMetaIndividual
{
    public ICimMetaClass? InstanceOf { get; set; }
}
