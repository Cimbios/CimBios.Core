using CimBios.Core.RdfIOLib;

namespace CimBios.Core.CimModel.Schema.RdfSchema;

/// <summary>
/// Base class provides general RDF description node data.
/// </summary>
[
    CimSchemaSerializable
    ("http://www.w3.org/1999/02/22-rdf-syntax-ns#description")
]
public interface ICimRdfDescription : ICimMetaResource
{
    [CimSchemaSerializable(
        "http://www.w3.org/2000/01/rdf-schema#label",
        MetaFieldType.Value)]
    public string Label { get; set; }

    [CimSchemaSerializable(
        "http://www.w3.org/2000/01/rdf-schema#comment",
        MetaFieldType.Value)]
    public string Comment { get; set; }

    [CimSchemaSerializable(
        "http://iec.ch/TC57/1999/rdf-schema-extensions-19990926#dataType",
        MetaFieldType.ByRef)]
    public CimRdfsClass? Datatype { get; set; }

    [CimSchemaSerializable(
       "http://iec.ch/TC57/1999/rdf-schema-extensions-19990926#stereotype",
       MetaFieldType.Enum, isCollection: true)]
    public ICollection<UMLStereotype> Stereotypes { get; }
}

[CimSchemaSerializable("http://www.w3.org/2000/01/rdf-schema#Class")]
public class CimRdfsClass : CimMetaClassBase,
    ICimRdfDescription, ICimMetaClass, ICimMetaExtensible
{
    public string Label { get => ShortName; set => ShortName = value; }
    public string Comment { get => Description; set => Description = value; }
    public CimRdfsClass? Datatype { get; set; }
    public ICollection<UMLStereotype> Stereotypes { get => _Stereotypes; }

    public override bool IsAbstract => Stereotypes.Contains(UMLStereotype.CIMAbstract);
    public override bool IsExtension => Stereotypes.Contains(UMLStereotype.CIMExtension);
    public override bool IsEnum => Stereotypes.Contains(UMLStereotype.Enumeration);
    public override bool IsCompound => Stereotypes.Contains(UMLStereotype.Compound);
    public override bool IsDatatype => Stereotypes.Contains(UMLStereotype.CIMDatatype);

    [
        CimSchemaSerializable(
        "http://www.w3.org/2000/01/rdf-schema#subClassOf",
        MetaFieldType.ByRef, isCollection: true)
    ]
    public List<ICimMetaClass> SubClassOf 
    { 
        get => _Ancestors;
        set => _Ancestors = value;
    }

    public CimRdfsClass(Uri baseUri) : base(baseUri, 
        string.Empty, string.Empty) { }

    public CimRdfsClass(CimRdfsClass rdfClass) 
        : base(rdfClass.BaseUri, rdfClass.ShortName, rdfClass.Description)
    {
        _Ancestors = rdfClass._Ancestors;
    }

    public override bool AddExtension(ICimMetaClass extension)
    {
        if (CanAddExtension(extension))
        {
            _Ancestors.Add(extension);

            (extension as CimRdfsClass)?.Stereotypes
                .Add(UMLStereotype.CIMExtension);

            return true;
        }

        return false;
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

    private readonly List<UMLStereotype> _Stereotypes = [];
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

[CimSchemaSerializable("http://www.w3.org/1999/02/22-rdf-syntax-ns#Property")]
public class CimRdfsProperty : CimMetaPropertyBase, 
    ICimRdfDescription, ICimMetaProperty
{
    public string Label { get => ShortName; set => ShortName = value; }
    public string Comment { get => Description; set => Description = value; }
    public CimRdfsClass? Datatype { get; set; }
    public ICollection<UMLStereotype> Stereotypes { get => _Stereotypes; }

    public override CimMetaPropertyKind PropertyKind => GetMetaPropertyKind();
    public override ICimMetaClass? PropertyDatatype => GetDatatype();
    public override bool IsExtension => IsDomainExtension();
    public override bool IsValueRequired => ValueRequired();

    [
        CimSchemaSerializable(
         "http://www.w3.org/2000/01/rdf-schema#domain",
         MetaFieldType.ByRef)
    ]
    public ICimMetaClass? Domain
    { 
        get => OwnerClass; 
        set => OwnerClass = value;
    }

    [
        CimSchemaSerializable(
        "http://iec.ch/TC57/1999/rdf-schema-extensions-19990926#inverseRoleName",
        MetaFieldType.ByRef)
    ]
    public override ICimMetaProperty? InverseProperty { get; protected set; }

    [
        CimSchemaSerializable(
        "http://www.w3.org/2000/01/rdf-schema#range",
        MetaFieldType.ByRef)
    ]
    public ICimMetaClass? Range { get; set; }

    [
        CimSchemaSerializable(
        "http://iec.ch/TC57/1999/rdf-schema-extensions-19990926#multiplicity",
        MetaFieldType.Enum)
    ]
    public Multiplicity? Multiplicity { get; set; }

    public CimRdfsProperty(Uri baseUri) 
        : base(baseUri, string.Empty, string.Empty) { }

    private CimMetaPropertyKind GetMetaPropertyKind()
    {
        if (RdfUtils.RdfUriEquals(Range?.BaseUri, 
            CimRdfSchemaStrings.RdfStatement))
        {
            return CimMetaPropertyKind.Statements;
        }

        if (Stereotypes.Contains(UMLStereotype.Attribute))
        {
            return CimMetaPropertyKind.Attribute;
        }
        
        if (Multiplicity == RdfSchema.Multiplicity.OneToOne
            || Multiplicity == RdfSchema.Multiplicity.StrictlyOne)
        {
            return CimMetaPropertyKind.Assoc1To1;
        }

        if (Multiplicity == RdfSchema.Multiplicity.ZeroToN
            || Multiplicity == RdfSchema.Multiplicity.OneToN)
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

    private bool ValueRequired()
    {
        if (Multiplicity == null)
        {
            return false;
        }

        if (Multiplicity == RdfSchema.Multiplicity.OneToN
            || Multiplicity == RdfSchema.Multiplicity.StrictlyOne)
        {
            return true;
        }

        return false;
    }

    private readonly List<UMLStereotype> _Stereotypes = [];
}

public class CimRdfsIndividual(Uri baseUri) 
    :   CimMetaIndividualBase(baseUri, string.Empty, string.Empty), 
        ICimRdfDescription, ICimMetaIndividual
{
    public string Label { get => ShortName; set => ShortName = value; }
    public string Comment { get => Description; set => Description = value; }
    public CimRdfsClass? Datatype { get; set; }
    public ICollection<UMLStereotype> Stereotypes { get => _Stereotypes; }

    private readonly List<UMLStereotype> _Stereotypes = [];
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
    ZeroToN,
    [
        CimSchemaSerializable
        ("http://iec.ch/TC57/1999/rdf-schema-extensions-19990926#M:1..n")
    ]
    OneToN,
    [
        CimSchemaSerializable
        ("http://iec.ch/TC57/1999/rdf-schema-extensions-19990926#M:1")
    ]
    StrictlyOne,
}
