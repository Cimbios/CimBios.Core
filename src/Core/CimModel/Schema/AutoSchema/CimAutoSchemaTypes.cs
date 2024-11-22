namespace CimBios.Core.CimModel.Schema.AutoSchema;

public class CimAutoResource : ICimMetaResource
{
    public required Uri BaseUri { get; set; }

    public required string ShortName { get; set; }

    public required string Description { get; set; }
}

public class CimAutoClass : CimAutoResource, ICimMetaClass
{
    public bool SuperClass => (ParentClass == null);

    public ICimMetaClass? ParentClass => null;

    public ICimMetaClass[] AllAncestors => _DummyAncestors;

    public ICimMetaClass[] Extensions => _DummyAncestors;

    public bool IsAbstract => false;

    public bool IsExtension => false;

    public bool IsEnum => false;

    public bool IsCompound { get; set; }

    public bool IsDatatype => false;

    private ICimMetaClass[] _DummyAncestors = [];
}

public class CimAutoProperty : ICimMetaProperty
{
    public ICimMetaClass? OwnerClass => throw new NotImplementedException();

    public CimMetaPropertyKind PropertyKind => throw new NotImplementedException();

    public ICimMetaProperty? InverseProperty => throw new NotImplementedException();

    public ICimMetaClass? PropertyDatatype => throw new NotImplementedException();

    public bool IsExtension => throw new NotImplementedException();

    public Uri BaseUri => throw new NotImplementedException();

    public string ShortName => throw new NotImplementedException();

    public string Description => throw new NotImplementedException();

}
