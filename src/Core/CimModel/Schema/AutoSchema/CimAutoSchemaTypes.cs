using CimBios.Core.RdfIOLib;

namespace CimBios.Core.CimModel.Schema.AutoSchema;

public class CimAutoResource : ICimMetaResource
{
    public required Uri BaseUri { get; set; }

    public required string ShortName { get; set; }

    public required string Description { get; set; }

    public override string ToString()
    {
        return ShortName;
    }

    public bool Equals(ICimMetaResource? other)
    {
        if (other == null)
        {
            return false;
        }

        return RdfUtils.RdfUriEquals(BaseUri, other.BaseUri);
    }

    public override int GetHashCode()
    {
        return BaseUri.AbsoluteUri.GetHashCode();
    }
}

/// <summary>
/// Schema auto class entity. Does not provide inheritance chain - only plain.
/// </summary>
public class CimAutoClass : CimAutoResource, 
    ICimMetaClass, ICimMetaExtensible
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

    public IEnumerable<ICimMetaClass> AllAncestors => _PlainAncestors;

    public IEnumerable<ICimMetaClass> Extensions => _PlainAncestors;

    public bool IsAbstract => false;

    public bool IsExtension { get; set; } = false;

    public bool IsEnum { get; set; }

    public bool IsCompound { get; set; }

    public bool IsDatatype => false;

    public IEnumerable<ICimMetaProperty> AllProperties => GetAllProperties();

    public IEnumerable<ICimMetaProperty> SelfProperties => _Properties;

    public IEnumerable<ICimMetaIndividual> AllIndividuals => GetAllIndividuals();

    public IEnumerable<ICimMetaIndividual> SelfIndividuals => _Individuals;

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

    public bool HasProperty(ICimMetaProperty metaProperty, bool inherit = true)
    {
        return GetAllProperties().Contains(metaProperty)
            && (inherit == true || metaProperty.OwnerClass == this);
    }

    public void AddProperty(ICimMetaProperty metaProperty)
    {
        if (this.Equals(metaProperty.OwnerClass)
            && HasProperty(metaProperty, false) == false)
        {
            _Properties.Add(metaProperty);
        }
    }

    public void RemoveProperty(ICimMetaProperty metaProperty)
    {
        if (metaProperty is CimAutoProperty cimAutoProperty
            && HasProperty(metaProperty, false) == true)
        {
            _Properties.Remove(cimAutoProperty);
        }        
    }

    public void AddIndividual(ICimMetaIndividual metaIndividual)
    {
        if (this.Equals(metaIndividual.InstanceOf)
            && _Individuals.Contains(metaIndividual) == false)
        {
            _Individuals.Add(metaIndividual);
        }
    }

    public void RemoveIndividual(ICimMetaIndividual metaIndividual)
    {
        if (metaIndividual is CimAutoIndividual cimAutoIndividual
            && _Individuals.Contains(metaIndividual) == true)
        {
            _Individuals.Remove(cimAutoIndividual);
        }        
    }

    private HashSet<CimAutoProperty> GetAllProperties()
    {
        HashSet<CimAutoProperty> properties = [];

        ICimMetaClass? nextClass = this;
        while (nextClass != null)
        {
            foreach (var p in nextClass.SelfProperties
                .OfType<CimAutoProperty>())
            {
                if (properties.Contains(p) == true)
                {
                    continue;
                }

                properties.Add(p);
            }

            foreach (var ext in nextClass.Extensions)
            {
                foreach (var extp in ext.SelfProperties
                    .OfType<CimAutoProperty>())
                {
                    if (properties.Contains(extp) == true)
                    {
                        continue;
                    }

                    properties.Add(extp);
                }              
            }

            nextClass = nextClass.ParentClass;
        }

        return properties;
    }

    private HashSet<CimAutoIndividual> GetAllIndividuals()
    {
        HashSet<CimAutoIndividual> individuals = [];

        ICimMetaClass? nextClass = this;
        while (nextClass != null)
        {
            foreach (var ind in nextClass.SelfIndividuals
                .OfType<CimAutoIndividual>())
            {
                if (individuals.Contains(ind) == true)
                {
                    continue;
                }

                individuals.Add(ind);
            }

            foreach (var ext in nextClass.Extensions)
            {
                foreach (var extind in ext.SelfIndividuals
                    .OfType<CimAutoIndividual>())
                {
                    if (individuals.Contains(extind) == true)
                    {
                        continue;
                    }

                    individuals.Add(extind);
                }              
            }

            nextClass = nextClass.ParentClass;
        }

        return individuals;
    }

    private readonly List<ICimMetaClass> _PlainAncestors = [];

    private CimAutoClass? _ParentClass;

    private readonly HashSet<ICimMetaProperty> _Properties = [];
    private readonly HashSet<ICimMetaIndividual> _Individuals = [];
}

public class CimAutoProperty : CimAutoResource, ICimMetaProperty
{
    public ICimMetaClass? OwnerClass 
    { 
        get => _OwnerClass; 
        set
        {
            if (_OwnerClass == value)
            {
                return;
            }

            if (value == null)
            {
                (_OwnerClass as ICimMetaExtensible)?.RemoveProperty(this); 
            }

            _OwnerClass = value as CimAutoClass;
            (_OwnerClass as ICimMetaExtensible)?.AddProperty(this); 
        }
    }

    public CimMetaPropertyKind PropertyKind { get; set; }

    public ICimMetaProperty? InverseProperty { get; set; }

    public ICimMetaClass? PropertyDatatype { get; set; }

    public bool IsExtension => false; 
    public bool IsValueRequired => false;

    private ICimMetaClass? _OwnerClass = null;
}

public class CimAutoDatatype : CimAutoClass, ICimMetaDatatype
{
    public Type? SystemType { get; set; }

    public Type PrimitiveType => SystemType ?? typeof(string);
}

public class CimAutoIndividual : CimAutoResource, ICimMetaIndividual
{
    public ICimMetaClass? InstanceOf 
    {
        get => _InstanceOf; 
        set
        {
            if (_InstanceOf == value)
            {
                return;
            }

            if (value == null)
            {
                _InstanceOf?.RemoveIndividual(this); 
            }

            _InstanceOf = value as CimAutoClass;
            _InstanceOf?.AddIndividual(this); 
        }
    }

    private CimAutoClass? _InstanceOf = null;
}
