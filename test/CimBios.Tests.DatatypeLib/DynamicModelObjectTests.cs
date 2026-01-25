using CimBios.Core.CimModel.DatatypeLib;
using CimBios.Core.CimModel.DatatypeLib.ModelObject;
using CimBios.Core.CimModel.DatatypeLib.OID;
using CimBios.Core.CimModel.Schema;
using CimBios.Tests.Infrastructure;

namespace CimBios.Tests.DatatypeLib;

public class DynamicModelObjectTests
{
    [Fact]
    public void GetAsDynamic()
    {
        var typelib = GetTypeLib();

        var substation = typelib.CreateInstance<Substation>(new UuidDescriptor()) 
            ?? throw new NullReferenceException();

        Assert.NotNull(substation.AsDynamic());
    }

    [Fact]
    public void DynamicAttribute()
    {
        var typelib = GetTypeLib();

        var substation = typelib.CreateInstance<Substation>(new UuidDescriptor()) 
            ?? throw new NullReferenceException();

        substation.name = "Dynamic substation";

        var dynamicObject = substation.AsDynamic();

        // typelib attribute
        Assert.Equal("Dynamic substation", dynamicObject.name);

        dynamicObject.description = "Description";

        // only schema attribute
        Assert.Equal("Description", dynamicObject.description);
    }

    [Fact]
    public void DynamicAssocs()
    {
        var typelib = GetTypeLib();

        var substation = typelib.CreateInstance<Substation>(new UuidDescriptor()) 
            ?? throw new NullReferenceException();

        var dynamicSubstation = substation.AsDynamic();

        var voltageLevel = typelib.CreateInstance<VoltageLevel>(new UuidDescriptor()) 
            ?? throw new NullReferenceException();

        var dynamicVoltageLevel = voltageLevel.AsDynamic();

        dynamicSubstation.AddToVoltageLevels(dynamicVoltageLevel);

        Assert.Contains(dynamicVoltageLevel, dynamicSubstation.VoltageLevels);

        // only schema assocs
        var nameClass = typelib.Schema
            .TryGetResource<ICimMetaClass>(new Uri("http://iec.ch/TC57/CIM100#Name"))
            ?? throw new NullReferenceException();

        var name = typelib.CreateInstance(
            new ModelObjectFactory(), 
            new UuidDescriptor(),
            nameClass) as ModelObject ?? throw new NullReferenceException();

        dynamicSubstation.AddToNames(name);

        dynamicSubstation.RemoveFromNames(name);

        dynamicSubstation.RemoveAllFromNames();

        name.SetAssoc1To1("IdentifiedObject", voltageLevel);

        Assert.Equal(voltageLevel, name.AsDynamic()?.IdentifiedObject);

        name.AsDynamic().IdentifiedObject = null;

        Assert.Null(name.AsDynamic().IdentifiedObject);
    }

    [Fact]
    public void TransitiveDynamicObjects()
    {
        var typelib = GetTypeLib();

        var substation = typelib.CreateInstance<Substation>(new UuidDescriptor()) 
            ?? throw new NullReferenceException();

        substation.name = "New substation";

        var dynamicSubstation = substation.AsDynamic();

        dynamicSubstation.description = "Substation description";

        var voltageLevel = typelib.CreateInstance<VoltageLevel>(new UuidDescriptor()) 
            ?? throw new NullReferenceException();

        voltageLevel.name = "New voltage level";

        var dynamicVoltageLevel = voltageLevel.AsDynamic();

        dynamicSubstation.AddToVoltageLevels(dynamicVoltageLevel);

        Assert.Equal("New substation", dynamicVoltageLevel.Substation.name);
        Assert.Equal("Substation description", dynamicVoltageLevel.Substation.description);

        var nameClass = typelib.Schema
            .TryGetResource<ICimMetaClass>(new Uri("http://iec.ch/TC57/CIM100#Name"))
            ?? throw new NullReferenceException();

        var name = typelib.CreateInstance(
            new ModelObjectFactory(), 
            new UuidDescriptor(),
            nameClass) as ModelObject ?? throw new NullReferenceException();

        dynamicSubstation.AddToNames(name);

        name.AsDynamic().name = "Extension name";

        Assert.Equal("Extension name", dynamicVoltageLevel.Substation.Names[0].name);
    }

    private static ICimDatatypeLib GetTypeLib()
    {
        var schema = ModelLoader.LoadTestCimRdfSchema();
        var typeLib = new CimDatatypeLib(schema);
        typeLib.LoadAssembly("CimBios.Tests.Infrastructure", reset: true);
        return typeLib;
    }
}
