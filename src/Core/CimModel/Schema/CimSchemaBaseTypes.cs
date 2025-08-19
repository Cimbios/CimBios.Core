using CimBios.Core.RdfIOLib;

namespace CimBios.Core.CimModel.Schema;

/// <summary>
///     Base class for any schema entity.
/// </summary>
public abstract class CimMetaResourceBase : ICimMetaResource
{
    protected CimMetaResourceBase(Uri baseUri,
        string shortName, string description)
    {
        BaseUri = baseUri;
        ShortName = shortName;
        Description = description;
    }

    public Uri BaseUri { get; }

    public string ShortName { get; protected set; }

    public string Description { get; protected set; }

    public bool Equals(ICimMetaResource? other)
    {
        if (other == null) return false;

        return RdfUtils.RdfUriEquals(BaseUri, other.BaseUri);
    }

    public override int GetHashCode()
    {
        return BaseUri.AbsoluteUri.GetHashCode();
    }

    public override string ToString()
    {
        return ShortName;
    }

    public static bool operator ==(CimMetaResourceBase? left,
        CimMetaResourceBase? right)
    {
        if (left is not null) return left.Equals(right);

        if (right is not null) return right.Equals(left);

        return true;
    }

    public static bool operator !=(CimMetaResourceBase? left,
        CimMetaResourceBase? right)
    {
        return !(left == right);
    }

    public override bool Equals(object? obj)
    {
        if (obj is ICimMetaResource cimMetaResource) return Equals(cimMetaResource);

        return false;
    }
}

/// <summary>
///     Base meta cim class info.
/// </summary>
public abstract class CimMetaClassBase : CimMetaResourceBase,
    ICimMetaClass, ICimMetaExtensible
{
    protected HashSet<ICimMetaClass> _Ancestors = [];
    protected HashSet<ICimMetaIndividual> _Individuals = [];

    protected HashSet<ICimMetaProperty> _Properties = [];

    protected CimMetaClassBase(Uri baseUri,
        string shortName, string description)
        : base(baseUri, shortName, description)
    {
    }

    public virtual bool SuperClass => ParentClass == null;

    public virtual ICimMetaClass? ParentClass
    {
        get => GetParentClass();
        set
        {
            var parentClass = GetParentClass();
            if (parentClass != null) _Ancestors.Remove(parentClass);

            if (value != null && _Ancestors.Contains(value) == false) _Ancestors.Add(value);
        }
    }

    public virtual IEnumerable<ICimMetaClass> AllAncestors => GetAllAncestors();

    public virtual IEnumerable<ICimMetaClass> Extensions
        => _Ancestors
            .OfType<ICimMetaClass>()
            .Where(c => c.IsExtension && c.ParentClass == null);

    public virtual bool IsAbstract { get; protected set; } = false;

    public virtual bool IsExtension { get; protected set; }

    public virtual bool IsEnum { get; protected set; } = false;

    public virtual bool IsCompound { get; protected set; } = false;

    public virtual bool IsDatatype { get; protected set; } = false;

    public virtual IEnumerable<ICimMetaProperty> AllProperties
        => GetAllProperties();

    public virtual IEnumerable<ICimMetaProperty> SelfProperties
        => _Properties;

    public virtual IEnumerable<ICimMetaIndividual> AllIndividuals
        => GetAllIndividuals();

    public virtual IEnumerable<ICimMetaIndividual> SelfIndividuals
        => _Individuals;

    public virtual bool HasProperty(ICimMetaProperty metaProperty,
        bool inherit = true)
    {
        return GetAllProperties().Contains(metaProperty)
               && (inherit || Equals(metaProperty.OwnerClass));
    }

    // public bool IsDescendantOf(ICimMetaClass metaClass, bool orEquals = false)
    // {
    //     if (orEquals == true && this.Equals(metaClass))
    //     {
    //         return true;
    //     }


    // }

    public virtual bool AddExtension(ICimMetaClass metaClass)
    {
        if (_Ancestors.Contains(metaClass)) return false;

        _Ancestors.Add(metaClass);

        (metaClass as CimMetaClassBase)!.IsExtension = true;

        return true;
    }

    public virtual bool RemoveExtension(ICimMetaClass metaClass)
    {
        return _Ancestors.Remove(metaClass);
    }

    public virtual void AddProperty(ICimMetaProperty metaProperty)
    {
        if (Equals(metaProperty.OwnerClass)
            && HasProperty(metaProperty) == false)
            _Properties.Add(metaProperty);
    }

    public virtual void RemoveProperty(ICimMetaProperty metaProperty)
    {
        if (HasProperty(metaProperty, false)) _Properties.Remove(metaProperty);
    }

    public virtual void AddIndividual(ICimMetaIndividual metaIndividual)
    {
        if (Equals(metaIndividual.InstanceOf)
            && _Individuals.Contains(metaIndividual) == false)
            _Individuals.Add(metaIndividual);
    }

    public virtual void RemoveIndividual(ICimMetaIndividual metaIndividual)
    {
        if (_Individuals.Contains(metaIndividual)) _Individuals.Remove(metaIndividual);
    }

    protected virtual ICimMetaClass? GetParentClass()
    {
        return _Ancestors.OfType<ICimMetaClass>()
            .FirstOrDefault(o => o.IsExtension == false
                                 || BaseUri == o.BaseUri);
    }

    protected virtual IEnumerable<ICimMetaClass> GetAllAncestors()
    {
        var parent = ParentClass;
        while (parent != null)
        {
            yield return parent;
            parent = parent.ParentClass;
        }
    }

    protected virtual HashSet<ICimMetaProperty> GetAllProperties()
    {
        HashSet<ICimMetaProperty> properties = new(new CimMetaResourceComparer());

        ICimMetaClass? nextClass = this;
        while (nextClass != null)
        {
            foreach (var p in nextClass.SelfProperties
                         .OfType<ICimMetaProperty>())
            {
                if (properties.Contains(p)) continue;

                properties.Add(p);
            }

            foreach (var ext in nextClass.Extensions)
            foreach (var extp in ext.SelfProperties
                         .OfType<ICimMetaProperty>())
            {
                if (properties.Contains(extp)) continue;

                properties.Add(extp);
            }

            nextClass = nextClass.ParentClass;
        }

        return properties;
    }

    protected virtual HashSet<ICimMetaIndividual> GetAllIndividuals()
    {
        HashSet<ICimMetaIndividual> individuals = new(new CimMetaResourceComparer());

        ICimMetaClass? nextClass = this;
        while (nextClass != null)
        {
            foreach (var ind in nextClass.SelfIndividuals
                         .OfType<ICimMetaIndividual>())
            {
                if (individuals.Contains(ind)) continue;

                individuals.Add(ind);
            }

            foreach (var ext in nextClass.Extensions)
            foreach (var extind in ext.SelfIndividuals
                         .OfType<ICimMetaIndividual>())
            {
                if (individuals.Contains(extind)) continue;

                individuals.Add(extind);
            }

            nextClass = nextClass.ParentClass;
        }

        return individuals;
    }
}

/// <summary>
///     Base meta cim property info.
/// </summary>
public abstract class CimMetaPropertyBase : CimMetaResourceBase,
    ICimMetaProperty
{
    private ICimMetaClass? _OwnerClass;

    protected CimMetaPropertyBase(Uri baseUri,
        string shortName, string description)
        : base(baseUri, shortName, description)
    {
    }

    public virtual ICimMetaClass? OwnerClass
    {
        get => _OwnerClass;
        set
        {
            if (_OwnerClass == value) return;

            if (value == null) (_OwnerClass as ICimMetaExtensible)?.RemoveProperty(this);

            _OwnerClass = value;
            (_OwnerClass as ICimMetaExtensible)?.AddProperty(this);
        }
    }

    public virtual CimMetaPropertyKind PropertyKind { get; protected set; }

    public virtual ICimMetaProperty? InverseProperty { get; protected set; }

    public virtual ICimMetaClass? PropertyDatatype { get; protected set; }

    public virtual bool IsExtension => false;
    public virtual bool IsValueRequired => false;
}

/// <summary>
///     Base meta cim instance class.
/// </summary>
public abstract class CimMetaIndividualBase : CimMetaResourceBase,
    ICimMetaIndividual
{
    private ICimMetaClass? _InstanceOf;

    protected CimMetaIndividualBase(Uri baseUri,
        string shortName, string description)
        : base(baseUri, shortName, description)
    {
    }

    public virtual ICimMetaClass? InstanceOf
    {
        get => _InstanceOf;
        set
        {
            if (_InstanceOf == value) return;

            if (value == null) (_InstanceOf as ICimMetaExtensible)?.RemoveIndividual(this);

            _InstanceOf = value;
            (_InstanceOf as ICimMetaExtensible)?.AddIndividual(this);
        }
    }
}

/// <summary>
/// 
/// </summary>
public class CimMetaResourceComparer : EqualityComparer<ICimMetaResource>
{
    public override bool Equals(ICimMetaResource? lResource,
        ICimMetaResource? rResource)
    {
        return RdfUtils.RdfUriEquals(lResource?.BaseUri, rResource?.BaseUri);
    }

    public override int GetHashCode(ICimMetaResource resource)
    {
        return resource.GetHashCode();
    }
}