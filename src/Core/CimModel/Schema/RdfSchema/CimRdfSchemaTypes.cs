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

    private readonly List<object> _Stereotypes =
        new List<object>();
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

    public System.Type SimpleType
    {
        get
        {
            // var type = SystemType;
            // var nextDatatype = Datatype;

            // while (type == null && nextDatatype != null)
            // {
            //     type = nextDatatype.SystemType;
            //     nextDatatype = nextDatatype.Datatype;
            // }

            // if (type == null)
            // {
            //     return typeof(string);
            // }

            // return type;
            return typeof(string);
        }
    }

    public CimRdfsDatatype(Uri baseUri) : base(baseUri) { }
}

[CimSchemaSerializable("http://www.w3.org/2000/01/rdf-schema#Class")]
public class CimRdfsClass : CimRdfDescriptionBase, ICimMetaClass
{
    public bool SuperClass => (SubClassOf == null);
    public ICimMetaClass? ParentClass => GetParentClass();
    public ICimMetaClass[] AllAncestors => GetAllAncestors().ToArray();
    public bool IsAbstract => Stereotypes.Contains(UMLStereotype.CIMAbstract);
    public bool IsExtension => Stereotypes.Contains(UMLStereotype.CIMExtension);
    public bool IsEnum => Stereotypes.Contains(UMLStereotype.Enumeration);
    public bool IsCompound => Stereotypes.Contains(UMLStereotype.Compound);

    [
        CimSchemaSerializable(
        "http://www.w3.org/2000/01/rdf-schema#subClassOf",
        MetaFieldType.ByRef, isCollection: true)
    ]
    public List<ICimMetaResource> SubClassOf => _SubClassOf;

    public CimRdfsClass(Uri baseUri) : base(baseUri) { }

    private ICimMetaClass? GetParentClass()
    {
        return SubClassOf.OfType<ICimMetaClass>()
            .FirstOrDefault(o => o.IsExtension == false);
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

    private readonly List<ICimMetaResource> _SubClassOf =
        new List<ICimMetaResource>();
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
    public CimRdfsClass? Domain { get; set; }

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