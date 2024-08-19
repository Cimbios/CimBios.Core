using System;

namespace CimBios.Core.CimModel.Schema;

/// <summary>
/// Custom attribute provides necessary serialization data. 
/// </summary>
internal class CimSchemaSerializableAttribute : Attribute
{
    public string AbsoluteUri { get; }
    public MetaFieldType FieldType { get; }
    public bool IsCollection { get; }

    public CimSchemaSerializableAttribute(string uri)
    {
        AbsoluteUri = uri;
    }

    public CimSchemaSerializableAttribute(string uri,
        MetaFieldType fieldType,
        bool isCollection = false) : this(uri)
    {
        FieldType = fieldType;
        IsCollection = isCollection;
    }
}

/// <summary>
/// Base class provides general RDF description node data.
/// </summary>
[
    CimSchemaSerializable
    ("http://www.w3.org/1999/02/22-rdf-syntax-ns#description")
]
public abstract class CimRdfDescriptionBase
{
    public Uri? Uri { get; set; }

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
    public CimMetaDatatype? Datatype { get; set; }

    [CimSchemaSerializable(
       "http://iec.ch/TC57/1999/rdf-schema-extensions-19990926#stereotype",
       MetaFieldType.Enum, isCollection: true)]
    public List<object> Stereotypes
    { get => _Stereotypes; }

    protected CimRdfDescriptionBase() { }

    private readonly List<object> _Stereotypes =
        new List<object>();
}

public class CimMetaIndividual : CimRdfDescriptionBase
{
    public CimMetaClass? EquivalentClass { get; set; }
}

[CimSchemaSerializable("http://www.w3.org/2000/01/rdf-schema#Datatype")]
public class CimMetaDatatype : CimRdfDescriptionBase
{
    public System.Type? SystemType { get; set; }

    public System.Type? EndSystemType
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
public class CimMetaClass : CimRdfDescriptionBase
{
    public bool SuperClass { get => SubClassOf == null; }

    [
        CimSchemaSerializable(
        "http://www.w3.org/2000/01/rdf-schema#subClassOf",
        MetaFieldType.ByRef)
    ]
    public CimMetaClass? SubClassOf { get; set; }
}

[CimSchemaSerializable("http://www.w3.org/1999/02/22-rdf-syntax-ns#Property")]
public class CimMetaProperty : CimRdfDescriptionBase
{
    [
        CimSchemaSerializable(
         "http://www.w3.org/2000/01/rdf-schema#domain",
         MetaFieldType.ByRef)
    ]
    public CimMetaClass? Domain { get; set; }

    [
        CimSchemaSerializable(
        "http://iec.ch/TC57/1999/rdf-schema-extensions-19990926#inverseRoleName",
        MetaFieldType.ByRef)
    ]
    public CimMetaProperty? InverseOf { get; set; }

    [
        CimSchemaSerializable(
        "http://www.w3.org/2000/01/rdf-schema#range",
        MetaFieldType.ByRef)
    ]
    public CimMetaClass? Range { get; set; }

    [
        CimSchemaSerializable(
        "http://iec.ch/TC57/1999/rdf-schema-extensions-19990926#multiplicity",
        MetaFieldType.Enum)
    ]
    public Multiplicity? Multiplicity { get; set; }
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