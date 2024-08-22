using CimBios.Core.CimModel.Schema;

namespace CimBios.Core.CimModel.Schema.RdfSchema;

public class CimRdfsIndividual : CimRdfDescriptionBase, ICimMetaInstance
{
    public ICimMetaClass? InstanceOf { get => EquivalentClass; }

    public CimRdfsClass? EquivalentClass { get; set; }
}

[CimSchemaSerializable("http://www.w3.org/2000/01/rdf-schema#Datatype")]
public class CimRdfsDatatype : CimRdfDescriptionBase, ICimMetaDatatype
{
    public System.Type? SystemType { get; set; }

    public System.Type? SimpleType
    {
        get
        {
            var type = SystemType;
            var nextDatatype = Datatype;

            while (type == null && nextDatatype != null)
            {
                type = nextDatatype.SystemType;
                nextDatatype = nextDatatype.Datatype;
            }

            return type;
        }
    }
}

[CimSchemaSerializable("http://www.w3.org/2000/01/rdf-schema#Class")]
public class CimRdfsClass : CimRdfDescriptionBase, ICimMetaClass
{
    public bool SuperClass { get => SubClassOf == null; }
    public ICimMetaClass? ParentClass { get => SubClassOf; }

    [
        CimSchemaSerializable(
        "http://www.w3.org/2000/01/rdf-schema#subClassOf",
        MetaFieldType.ByRef)
    ]
    public CimRdfsClass? SubClassOf { get; set; }
}

[CimSchemaSerializable("http://www.w3.org/1999/02/22-rdf-syntax-ns#Property")]
public class CimRdfsProperty : CimRdfDescriptionBase, ICimMetaProperty
{
    public ICimMetaClass? OwnerClass { get => Domain; }
    public CimMetaPropertyKind PropertyKind { get => GetMetaPropertyKind(); }
    public ICimMetaProperty? InverseProperty { get => InverseOf;  }

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

    private CimMetaPropertyKind GetMetaPropertyKind()
    {
        if (Stereotypes.Contains(UMLStereotype.Attribute))
        {
            return CimMetaPropertyKind.Attribute;
        }
        
        if (Multiplicity == RdfSchema.Multiplicity.OneToOne)
        {
            return CimMetaPropertyKind.Assoc1To1;
        }

        if (Multiplicity == RdfSchema.Multiplicity.OneToN)
        {
            return CimMetaPropertyKind.Assoc1To1;
        }

        return CimMetaPropertyKind.NonStandard;
    }
}

[
    CimSchemaSerializable
    ("http://iec.ch/TC57/1999/rdf-schema-extensions-19990926#stereotype")
]
public enum UMLStereotype
{
    [CimSchemaSerializable("http://iec.ch/TC57/NonStandard/UML#attribute")]
    Attribute,
    [CimSchemaSerializable("http://iec.ch/TC57/NonStandard/UML#aggregateOf")]
    AggregateOf,
    [CimSchemaSerializable("http://iec.ch/TC57/NonStandard/UML#ofAggregate")]
    OfAggregate,
    [CimSchemaSerializable("http://iec.ch/TC57/NonStandard/UML#enumeration")]
    Enumeration,
    [CimSchemaSerializable("http://iec.ch/TC57/NonStandard/UML#compound")]
    Compound
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
    OneToN
}