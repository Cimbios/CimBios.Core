
namespace CimBios.Core.CimModel.CimDatatypeLib.OID;

public class AutoDescriptor : OIDDescriptorBase
{
    private const string BaseNamespace = "base:";

    public override Uri AbsoluteOID { get; }

    public AutoDescriptor()
    {
        var oid = $"_auto-{DateTime.UtcNow.Ticks}";

        AbsoluteOID = new Uri(BaseNamespace + oid);
    }

    public override int CompareTo(object? obj)
    {
        return AbsoluteOID.AbsolutePath.CompareTo(obj);
    }
}
