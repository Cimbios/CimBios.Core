using CimBios.Core.CimModel.DatatypeLib.Factory;
using CimBios.Core.CimModel.DatatypeLib.OID;
using CimBios.Core.CimModel.DatatypeLib.TypeLib;

namespace CimBios.Tests.DatatypeLib;

public class CoreTypeLib
{
    [Fact]
    public void CreateCoreTypeLib()
    {
        var typeLib = new CoreDatatypeLibFactory().Create();
        Assert.NotNull(typeLib);

        var fullModel = typeLib.CreateInstance<FullModel>(new UuidDescriptor());
        Assert.NotNull(fullModel);
        
        fullModel.created = DateTime.Now;
        fullModel.version = 1;
        
        var diffModel = typeLib.CreateInstance<DifferenceModel>(new UuidDescriptor());
        Assert.NotNull(diffModel);
        
        diffModel.forwardDifferences.Clear();
        diffModel.reverseDifferences.Clear();
        
        var description = typeLib.CreateInstance<Description>(new UuidDescriptor());
        Assert.NotNull(description);
    }
}