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

    private static ICimSchema LoadCimSchema(string path,
        ICimSchemaFactory? factory = null)
    {
        factory ??= new CimRdfSchemaXmlFactory();
        var cimSchema = factory.CreateSchema();

        cimSchema.Load(new StreamReader(path));

        return cimSchema;
    }
}