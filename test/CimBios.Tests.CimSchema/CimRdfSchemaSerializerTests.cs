using CimBios.Core.CimModel.Schema;
using CimBios.Core.CimModel.Schema.RdfSchema;

namespace CimBios.Tests.CimSchema;

public class CimRdfSchemaSerializerTests
{
    [Theory]
    [InlineData("assets/SplitXmlNodes.rdfs")]
    [InlineData("assets/XmlNodes.rdfs")]
    public void RdfsXmlSchemaBuilding(string path)
    {
        var schema = new CimRdfSchemaXmlFactory().CreateSchema();
        schema.Load(new StreamReader(path));

        var identifiedObjectClass = schema.TryGetResource<ICimMetaClass>(
            new Uri("http://iec.ch/TC57/CIM100#IdentifiedObject"));
        
        Assert.NotNull(identifiedObjectClass);

        Assert.Equal("IdentifiedObject", identifiedObjectClass.ShortName);
        
        Assert.True(identifiedObjectClass.IsAbstract);
        
        var identifiedObjectNameProperty = schema.TryGetResource<ICimMetaProperty>(
            new Uri("http://iec.ch/TC57/CIM100#IdentifiedObject.name"));
        
        Assert.NotNull(identifiedObjectNameProperty);
        
        Assert.Contains(identifiedObjectNameProperty, 
            identifiedObjectClass.SelfProperties);
        
        Assert.Equal(CimMetaPropertyKind.Attribute, 
            identifiedObjectNameProperty.PropertyKind);

        var dataType = identifiedObjectNameProperty.PropertyDatatype as ICimMetaDatatype;
        Assert.NotNull(dataType);

        Assert.Equal(typeof(string), dataType.PrimitiveType);
        
        var organisationClass = schema.TryGetResource<ICimMetaClass>(
            new Uri("http://iec.ch/TC57/CIM100#Organisation"));
        
        Assert.NotNull(organisationClass);
        
        Assert.Equal(identifiedObjectClass, organisationClass.ParentClass);
        
        var phaseCodeClass = schema.TryGetResource<ICimMetaClass>(
            new Uri("http://iec.ch/TC57/CIM100#PhaseCode"));
        
        Assert.NotNull(phaseCodeClass);
        
        var phaseABCLiteral = schema.TryGetResource<ICimMetaIndividual>(
            new Uri("http://iec.ch/TC57/CIM100#PhaseCode.ABC"));
        
        Assert.NotNull(phaseABCLiteral);

        Assert.Contains(phaseABCLiteral, phaseCodeClass.SelfIndividuals);
    }
}
