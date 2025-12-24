/*
*    CimBios.Core - Common Information Model (IEC61970) I/O Library
*    Copyright (C) 2025 Yuri A. Kovalenko a.k.a belizahrt <belizahrt@gmail.com>
*
*    This program is free software: you can redistribute it and/or modify
*    it under the terms of the GNU General Public License as published by
*    the Free Software Foundation, either version 3 of the License, or
*    (at your option) any later version.
*
*    This program is distributed in the hope that it will be useful,
*    but WITHOUT ANY WARRANTY; without even the implied warranty of
*    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
*    GNU General Public License for more details.
*
*    You should have received a copy of the GNU General Public License
*    along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/

using CimBios.Core.RdfIOLib;

namespace CimBios.Core.CimModel.Schema.RdfSchema;

/// <summary>
///     Base class provides general RDF description node data.
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
        "http://iec.ch/TC57/1999/rdf-schema-extensions-19990926#belongsToCategory",
        MetaFieldType.ByRef)]
    public CimRdfsPackage? BelongsToCategory { get; set; }

    [CimSchemaSerializable(
        "http://iec.ch/TC57/1999/rdf-schema-extensions-19990926#stereotype",
        MetaFieldType.Enum, true)]
    public ICollection<UMLStereotype> Stereotypes { get; }
}

[CimSchemaSerializable("http://iec.ch/TC57/1999/rdf-schema-extensions-19990926#ClassCategory")]
public class CimRdfsPackage : CimMetaPackageBase, ICimRdfDescription, ICimMetaPackage
{
    public CimRdfsPackage(Uri baseUri): base(baseUri,
        string.Empty, string.Empty)
    {
    }

    public string Label
    {
        get => ShortName;
        set => ShortName = value;
    }

    public string Comment
    {
        get => Description;
        set => Description = value;
    }
    
    public CimRdfsClass? Datatype { get; set; }
    public CimRdfsPackage? BelongsToCategory { get; set; }
    public ICollection<UMLStereotype> Stereotypes => _stereotypes;
    
    private readonly List<UMLStereotype> _stereotypes = [];
}

[CimSchemaSerializable("http://www.w3.org/2000/01/rdf-schema#Class")]
public class CimRdfsClass : CimMetaClassBase,
    ICimRdfDescription, ICimMetaClass, ICimMetaExtensible
{
    private readonly List<UMLStereotype> _stereotypes = [];

    public CimRdfsClass(Uri baseUri) : base(baseUri,
        string.Empty, string.Empty)
    {
    }

    public CimRdfsClass(CimRdfsClass rdfClass)
        : base(rdfClass.BaseUri, rdfClass.ShortName, rdfClass.Description)
    {
        _Ancestors = rdfClass._Ancestors;
    }

    [
        CimSchemaSerializable(
            "http://www.w3.org/2000/01/rdf-schema#subClassOf",
            MetaFieldType.ByRef, true)
    ]
    public HashSet<ICimMetaClass> SubClassOf
    {
        get => _Ancestors;
        set => _Ancestors = value;
    }

    public override bool IsAbstract => Stereotypes.Contains(UMLStereotype.CIMAbstract);
    public override bool IsEnum => Stereotypes.Contains(UMLStereotype.Enumeration);
    public override bool IsCompound => Stereotypes.Contains(UMLStereotype.Compound);
    public override bool IsDatatype => Stereotypes.Contains(UMLStereotype.CIMDatatype);

    public string Label
    {
        get => ShortName;
        set => ShortName = value;
    }

    public string Comment
    {
        get => Description;
        set => Description = value;
    }

    public CimRdfsClass? Datatype { get; set; }
    
    public CimRdfsPackage? BelongsToCategory 
    { get => Package as CimRdfsPackage; set => Package = value; }
    
    public ICollection<UMLStereotype> Stereotypes => _stereotypes;
}

[CimSchemaSerializable("http://www.w3.org/2000/01/rdf-schema#Datatype")]
public class CimRdfsDatatype : CimRdfsClass, ICimMetaDatatype
{
    public CimRdfsDatatype(Uri baseUri) : base(baseUri)
    {
        MakeStereotype();
    }

    public CimRdfsDatatype(CimRdfsClass rdfsClass) : base(rdfsClass)
    {
        MakeStereotype();
    }

    public Type? SystemType { get; set; }

    public Type PrimitiveType
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

            if (type == null) return typeof(string);

            return type;
        }
    }

    private void MakeStereotype()
    {
        if (Stereotypes.Contains(UMLStereotype.CIMDatatype) == false) Stereotypes.Add(UMLStereotype.CIMDatatype);
    }
}

[CimSchemaSerializable("http://www.w3.org/1999/02/22-rdf-syntax-ns#Property")]
public class CimRdfsProperty : CimMetaPropertyBase,
    ICimRdfDescription, ICimMetaProperty
{
    private readonly List<UMLStereotype> _stereotypes = [];

    public CimRdfsProperty(Uri baseUri)
        : base(baseUri, string.Empty, string.Empty)
    {
    }

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

    public override CimMetaPropertyKind PropertyKind => GetMetaPropertyKind();
    public override ICimMetaClass? PropertyDatatype => GetDatatype();
    public override bool IsValueRequired => ValueRequired();

    [
        CimSchemaSerializable(
            "http://iec.ch/TC57/1999/rdf-schema-extensions-19990926#inverseRoleName",
            MetaFieldType.ByRef)
    ]
    public override ICimMetaProperty? InverseProperty { get; protected set; }

    public string Label
    {
        get => ShortName;
        set => ShortName = value;
    }

    public string Comment
    {
        get => Description;
        set => Description = value;
    }

    public CimRdfsClass? Datatype { get; set; }
    
    public CimRdfsPackage? BelongsToCategory 
    { get => Package as CimRdfsPackage; set => Package = value; }
    
    public ICollection<UMLStereotype> Stereotypes => _stereotypes;

    private CimMetaPropertyKind GetMetaPropertyKind()
    {
        if (RdfUtils.RdfUriEquals(Range?.BaseUri,
                CimRdfSchemaStrings.RdfStatement))
            return CimMetaPropertyKind.Statements;

        if (Stereotypes.Contains(UMLStereotype.Attribute)) return CimMetaPropertyKind.Attribute;

        if (Multiplicity == RdfSchema.Multiplicity.OneToOne
            || Multiplicity == RdfSchema.Multiplicity.StrictlyOne)
            return CimMetaPropertyKind.Assoc1To1;

        if (Multiplicity == RdfSchema.Multiplicity.ZeroToN
            || Multiplicity == RdfSchema.Multiplicity.OneToN)
            return CimMetaPropertyKind.Assoc1ToM;

        return CimMetaPropertyKind.NonStandard;
    }

    private ICimMetaClass? GetDatatype()
    {
        if (PropertyKind == CimMetaPropertyKind.NonStandard) return null;

        if (PropertyKind == CimMetaPropertyKind.Attribute
            && Datatype != null)
            return Datatype;

        return Range;
    }

    private bool ValueRequired()
    {
        if (Multiplicity == null) return false;

        if (Multiplicity == RdfSchema.Multiplicity.OneToN
            || Multiplicity == RdfSchema.Multiplicity.StrictlyOne)
            return true;

        return false;
    }
}

public class CimRdfsIndividual(Uri baseUri)
    : CimMetaIndividualBase(baseUri, string.Empty, string.Empty),
        ICimRdfDescription, ICimMetaIndividual
{
    private readonly List<UMLStereotype> _stereotypes = [];

    public string Label
    {
        get => ShortName;
        set => ShortName = value;
    }

    public string Comment
    {
        get => Description;
        set => Description = value;
    }

    public CimRdfsClass? Datatype { get; set; }
    
    public CimRdfsPackage? BelongsToCategory 
    { get => Package as CimRdfsPackage; set => Package = value; }
    
    public ICollection<UMLStereotype> Stereotypes => _stereotypes;
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
    CIMDatatype
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
    StrictlyOne
}