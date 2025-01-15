using CimBios.Core.RdfIOLib;

namespace CimBios.Core.CimModel.Schema.RdfSchema;

/// <summary>
/// Base class provides general RDF description node data.
/// </summary>
[
    CimSchemaSerializable
    ("http://www.w3.org/1999/02/22-rdf-syntax-ns#description")
]
public abstract class CimRdfDescriptionBase : ICimMetaResource
{
    public Uri BaseUri { get; }
    public string ShortName { get => Label; }
    public string Description { get => Comment; }

    [CimSchemaSerializable(
        "http://www.w3.org/2000/01/rdf-schema#label",
        MetaFieldType.Value)]
    public string Label { get; set; } = string.Empty;

    [CimSchemaSerializable(
        "http://www.w3.org/2000/01/rdf-schema#comment",
        MetaFieldType.Value)]
    public string Comment { get; set; } = string.Empty;

    [CimSchemaSerializable(
        "http://iec.ch/TC57/1999/rdf-schema-extensions-19990926#dataType",
        MetaFieldType.ByRef)]
    public CimRdfsClass? Datatype { get; set; }

    [CimSchemaSerializable(
       "http://iec.ch/TC57/1999/rdf-schema-extensions-19990926#stereotype",
       MetaFieldType.Enum, isCollection: true)]
    public List<object> Stereotypes
    { get => _Stereotypes; }

    protected CimRdfDescriptionBase(Uri baseUri) 
    { 
        BaseUri = baseUri;
    }

    protected CimRdfDescriptionBase(CimRdfDescriptionBase rdfDescription)
    {
        BaseUri = rdfDescription.BaseUri;
        Label = rdfDescription.Label;
        Comment = rdfDescription.Comment;
        Datatype = rdfDescription.Datatype;
        _Stereotypes = rdfDescription.Stereotypes;
    }

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

    private readonly List<object> _Stereotypes = [];
}

public class CimRdfsIndividual : CimRdfDescriptionBase, ICimMetaIndividual
{
    public ICimMetaClass? InstanceOf { get => EquivalentClass; }

    public CimRdfsClass? EquivalentClass { get; set; }

    public CimRdfsIndividual(Uri baseUri) : base(baseUri) { }
}

[CimSchemaSerializable("http://www.w3.org/2000/01/rdf-schema#Datatype")]
public class CimRdfsDatatype : CimRdfsClass, ICimMetaDatatype
{
    public System.Type? SystemType { get; set; }

    public System.Type PrimitiveType
    {
        get
        {
            var type = SystemType;
            var nextDatatype = Datatype as CimRdfsDatatype;

            while (type == null && nextDatatype != null)
            {
                type = nextDatatype.SystemType;
                nextDatatype = nextDatatype.Datatype as CimRdfsDatatype;
            }

            if (type == null)
            {
                return typeof(string);
            }

            return type;
        }
    }

    public CimRdfsDatatype(Uri baseUri) : base(baseUri)
    { 
        MakeStereotype();
    }

    public CimRdfsDatatype(CimRdfsClass rdfsClass) : base(rdfsClass) 
    { 
        MakeStereotype();
    }

    private void MakeStereotype()
    {
        if (Stereotypes.Contains(UMLStereotype.CIMDatatype) == false)
        {
            Stereotypes.Add(UMLStereotype.CIMDatatype);
        }
    }
}

[CimSchemaSerializable("http://www.w3.org/2000/01/rdf-schema#Class")]
public class CimRdfsClass : CimRdfDescriptionBase, 
    ICimMetaClass, ICimMetaExtensible
{
    public bool SuperClass => (SubClassOf == null);
    public ICimMetaClass? ParentClass => GetParentClass();
    public IEnumerable<ICimMetaClass> AllAncestors => GetAllAncestors();
    public IEnumerable<ICimMetaClass> Extensions => _SubClassOf
        .OfType<ICimMetaClass>().Where(c => c.IsExtension);
    public IEnumerable<ICimMetaProperty> AllProperties => GetAllProperties();
    public IEnumerable<ICimMetaProperty> SelfProperties => _Properties;
    public bool IsAbstract => Stereotypes.Contains(UMLStereotype.CIMAbstract);
    public bool IsExtension => Stereotypes.Contains(UMLStereotype.CIMExtension);
    public bool IsEnum => Stereotypes.Contains(UMLStereotype.Enumeration);
    public bool IsCompound => Stereotypes.Contains(UMLStereotype.Compound);
    public bool IsDatatype => Stereotypes.Contains(UMLStereotype.CIMDatatype);

    [
        CimSchemaSerializable(
        "http://www.w3.org/2000/01/rdf-schema#subClassOf",
        MetaFieldType.ByRef, isCollection: true)
    ]
    public List<ICimMetaResource> SubClassOf => _SubClassOf;

    public CimRdfsClass(Uri baseUri) : base(baseUri) { }

    public CimRdfsClass(CimRdfsClass rdfClass) : base(rdfClass)
    {
        _SubClassOf = rdfClass._SubClassOf;
    }

    public bool HasProperty(ICimMetaProperty metaProperty, bool inherit = true)
    {
        return GetAllProperties().Contains(metaProperty)
            && (inherit == true || metaProperty.OwnerClass == this);
    }

    public void AddProperty(ICimMetaProperty metaProperty)
    {
        if (metaProperty.OwnerClass == this
            && metaProperty is CimRdfsProperty cimRdfsProperty
            && HasProperty(metaProperty, false) == false)
        {
            _Properties.Add(cimRdfsProperty);
        }
    }

    public void RemoveProperty(ICimMetaProperty metaProperty)
    {
        if (metaProperty is CimRdfsProperty cimRdfsProperty
            && HasProperty(metaProperty, false) == true)
        {
            _Properties.Remove(cimRdfsProperty);
        }        
    }

    public bool AddExtension(ICimMetaClass extension)
    {
        if (CanAddExtension(extension))
        {
            _SubClassOf.Add(extension);

            (extension as CimRdfsClass)?.Stereotypes
                .Add(UMLStereotype.CIMExtension);

            return true;
        }

        return false;
    }

    public bool RemoveExtension(ICimMetaClass extension)
    {
        return _SubClassOf.Remove(extension);
    }

    private bool CanAddExtension(ICimMetaClass metaClass)
    {
        if (metaClass.IsCompound || metaClass.IsDatatype)
        {
            return false;
        }

        if (metaClass.IsExtension)
        {
            return true;
        }

        if (RdfUtils.TryGetEscapedIdentifier(this.BaseUri, out var thisName)
            && RdfUtils.TryGetEscapedIdentifier(metaClass.BaseUri, 
                out var className)
            && thisName == className)
        {
            return true;
        }

        return false;
    }

    private ICimMetaClass? GetParentClass()
    {
        return SubClassOf.OfType<ICimMetaClass>()
            .FirstOrDefault(o => o.IsExtension == false 
                || (o.IsExtension && o.ParentClass != null));
    }

    private IEnumerable<ICimMetaClass> GetAllAncestors()
    {
        var parent = ParentClass;
        while (parent != null)
        {
            yield return parent;
            parent = parent.ParentClass;
        }
    }

    private HashSet<CimRdfsProperty> GetAllProperties()
    {
        HashSet<CimRdfsProperty> properties = [];

        ICimMetaClass? nextClass = this;
        while (nextClass != null)
        {
            foreach (var p in nextClass.SelfProperties
                .OfType<CimRdfsProperty>())
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
                    .OfType<CimRdfsProperty>())
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

    private readonly List<ICimMetaResource> _SubClassOf = [];
    private readonly HashSet<CimRdfsProperty> _Properties = [];
}

[CimSchemaSerializable("http://www.w3.org/1999/02/22-rdf-syntax-ns#Property")]
public class CimRdfsProperty : CimRdfDescriptionBase, ICimMetaProperty
{
    public ICimMetaClass? OwnerClass => Domain;
    public CimMetaPropertyKind PropertyKind => GetMetaPropertyKind();
    public ICimMetaProperty? InverseProperty => InverseOf;
    public ICimMetaClass? PropertyDatatype => GetDatatype();
    public bool IsExtension => IsDomainExtension();

    [
        CimSchemaSerializable(
         "http://www.w3.org/2000/01/rdf-schema#domain",
         MetaFieldType.ByRef)
    ]
    public CimRdfsClass? Domain
    { 
        get => _Domain; 
        set 
        {
            if (_Domain == value)
            {
                return;
            }

            if (value == null)
            {
                _Domain?.RemoveProperty(this); 
            }

            _Domain = value;
            _Domain?.AddProperty(this); 
        } 
    }

    [
        CimSchemaSerializable(
        "http://iec.ch/TC57/1999/rdf-schema-extensions-19990926#inverseRoleName",
        MetaFieldType.ByRef)
    ]
    public CimRdfsProperty? InverseOf { get; set; }

    [
        CimSchemaSerializable(
        "http://www.w3.org/2000/01/rdf-schema#range",
        MetaFieldType.ByRef)
    ]
    public CimRdfsClass? Range { get; set; }

    [
        CimSchemaSerializable(
        "http://iec.ch/TC57/1999/rdf-schema-extensions-19990926#multiplicity",
        MetaFieldType.Enum)
    ]
    public Multiplicity? Multiplicity { get; set; }

    public CimRdfsProperty(Uri baseUri) : base(baseUri) { }

    private CimMetaPropertyKind GetMetaPropertyKind()
    {
        if (Stereotypes.Contains(UMLStereotype.Attribute))
        {
            return CimMetaPropertyKind.Attribute;
        }
        
        if (Multiplicity == RdfSchema.Multiplicity.OneToOne
            || Multiplicity == RdfSchema.Multiplicity.StrictlyOne)
        {
            return CimMetaPropertyKind.Assoc1To1;
        }

        if (Multiplicity == RdfSchema.Multiplicity.OneToN)
        {
            return CimMetaPropertyKind.Assoc1ToM;
        }

        return CimMetaPropertyKind.NonStandard;
    }

    private ICimMetaClass? GetDatatype()
    {
        if (PropertyKind == CimMetaPropertyKind.NonStandard)
        {
            return null;
        }
        else if (PropertyKind == CimMetaPropertyKind.Attribute
            && Datatype != null)
        {
            return Datatype;
        }
        else
        {
            return Range;
        }
    }

    private bool IsDomainExtension()
    {
        if (Domain == null)
        {
            return false;
        }

        return Domain.IsExtension;
    }

    private CimRdfsClass? _Domain = null;
}

[
    CimSchemaSerializable
    ("http://iec.ch/TC57/1999/rdf-schema-extensions-19990926#stereotype")
]
public enum UMLStereotype
{
    [CimSchemaSerializable("http://langdale.com.au/2005/UML#attribute")]
    Attribute,
    [CimSchemaSerializable("http://langdale.com.au/2005/UML#aggregateOf")]
    AggregateOf,
    [CimSchemaSerializable("http://langdale.com.au/2005/UML#ofAggregate")]
    OfAggregate,
    [CimSchemaSerializable("http://langdale.com.au/2005/UML#enumeration")]
    Enumeration,
    [CimSchemaSerializable("http://langdale.com.au/2005/UML#compound")]
    Compound,
    [CimSchemaSerializable("http://langdale.com.au/2005/UML#cimextension")]
    CIMExtension,
    [CimSchemaSerializable("http://langdale.com.au/2005/UML#cimabstract")]
    CIMAbstract,
    [CimSchemaSerializable("http://langdale.com.au/2005/UML#cimdatatype")]
    CIMDatatype,
}

[
    CimSchemaSerializable
    ("http://iec.ch/TC57/1999/rdf-schema-extensions-19990926#multiplicity")
]
public enum Multiplicity
{
    [
        CimSchemaSerializable
        ("http://iec.ch/TC57/1999/rdf-schema-extensions-19990926#M:0..1")
    ]
    OneToOne,
    [
        CimSchemaSerializable
        ("http://iec.ch/TC57/1999/rdf-schema-extensions-19990926#M:0..n")
    ]
    OneToN,
    [
        CimSchemaSerializable
        ("http://iec.ch/TC57/1999/rdf-schema-extensions-19990926#M:1")
    ]
    StrictlyOne,
}
