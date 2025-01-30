using CimBios.Core.CimModel.Schema;
using CimBios.Core.CimModel.Schema.RdfSchema;

namespace CimBios.Tests.CimSchemaTests;

public class CimSchemaTests
{
    [Fact]
    public void GetNamespacesTest()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public void GetClassesTest()
    {
        throw new NotImplementedException();
    }    

    [Fact]
    public void GetExtensionsTest()
    {
        throw new NotImplementedException();
    }    

    // ...

    [Fact]
    public void JoinUnintersectedClassTest()
    {
        var schema1 = LoadCimSchema("../../../assets/SchemaTestAssets-rdfs.xml");
        var schema2 = LoadCimSchema("../../../assets/RightSchemaTestAssets-rdfs.xml");
        schema1.Join(schema2);

        var classZ = schema1.TryGetResource<ICimMetaClass>(
            new("http://cim.bios/Profiles/TestAssets#ClassZ"));

        if (classZ == null)
        {
            Assert.Fail("No class Z in joined schema.");
        }

        Assert.Single(classZ.AllProperties);
    }    

    [Fact]
    public void JoinIntersectedGeneralizationClassTest()
    {
        var schema1 = LoadCimSchema("../../../assets/SchemaTestAssets-rdfs.xml");
        var schema2 = LoadCimSchema("../../../assets/RightSchemaTestAssets-rdfs.xml");
        schema1.Join(schema2);

        var classA = schema1.TryGetResource<ICimMetaClass>(
            new("http://cim.bios/Profiles/TestAssets#ClassA"));

        var classX = schema1.TryGetResource<ICimMetaClass>(
            new("http://cim.bios/Profiles/TestAssets#ClassX"));

        if (classX == null || classA == null)
        {
            Assert.Fail("No class A|X in joined schema.");
        }   

        var AProp1 = schema1.TryGetResource<ICimMetaProperty>(
            new("http://cim.bios/Profiles/TestAssets#ClassA.AProp1")); 

        if (AProp1 == null)
        {
            Assert.Fail("No prop AProp1 in joined schema.");
        }    

        if (classX.ParentClass != classA)
        {
            Assert.Fail("X is not inherited from A.");
        }

        Assert.Contains(AProp1, classX.AllProperties);
    }   

    [Fact]
    public void JoinReverseGeneralizationClassTest()
    {
        var schema1 = LoadCimSchema("../../../assets/SchemaTestAssets-rdfs.xml");
        var schema2 = LoadCimSchema("../../../assets/RightSchemaTestAssets-rdfs.xml");
        schema1.Join(schema2);

        var classU = schema1.TryGetResource<ICimMetaClass>(
            new("http://cim.bios/Profiles/TestAssets#ClassU"));

        var classY = schema1.TryGetResource<ICimMetaClass>(
            new("http://cim.bios/Profiles/TestAssets#ClassY"));

        if (classU == null || classY == null)
        {
            Assert.Fail("No class U|Y in joined schema.");
        }   

        Assert.Equal(classY, classU.ParentClass);
    } 

    [Fact]
    public void JoinExtensionClassTest()
    {
        var schema1 = LoadCimSchema("../../../assets/SchemaTestAssets-rdfs.xml");
        var schema2 = LoadCimSchema("../../../assets/RightSchemaTestAssets-rdfs.xml");
        schema1.Join(schema2);  

        var classB = schema1.TryGetResource<ICimMetaClass>(
            new("http://cim.bios/Profiles/TestAssets#ClassB"));

        var classExtB = schema1.TryGetResource<ICimMetaClass>(
            new("http://cim.bios/Profiles/TestAssets/Extensions#ClassB")); 

        if (classB == null || classExtB == null)
        {
            Assert.Fail("No class B|extB in joined schema.");
        }  

        Assert.True(classB.Extensions.Contains(classExtB) 
            && classB.SelfProperties.Any());
    } 

    [Fact]
    public void JoinEnumTest()
    {
        var schema1 = LoadCimSchema("../../../assets/SchemaTestAssets-rdfs.xml");
        var schema2 = LoadCimSchema("../../../assets/RightSchemaTestAssets-rdfs.xml");
        schema1.Join(schema2);  

        var enum0 = schema1.TryGetResource<ICimMetaClass>(
            new("http://cim.bios/Profiles/TestAssets#Enum0"));

        var enum1 = schema1.TryGetResource<ICimMetaClass>(
            new("http://cim.bios/Profiles/TestAssets#Enum1"));

        if (enum0 == null || enum1 == null)
        {
            Assert.Fail("No class Enum0|1 in joined schema.");
        }   

        Assert.True(enum0.SelfIndividuals.Count() == 2 
            && enum1.SelfIndividuals.Count() == 2);
    }

    private static ICimSchema LoadCimSchema(string path, 
        ICimSchemaFactory? factory = null)
    {
        factory ??= new CimRdfSchemaXmlFactory();
        var cimSchema = factory.CreateSchema();

        cimSchema.Load(new StreamReader(path));

        return cimSchema;
    }
}
