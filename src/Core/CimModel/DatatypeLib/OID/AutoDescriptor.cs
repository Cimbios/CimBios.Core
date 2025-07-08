namespace CimBios.Core.CimModel.CimDatatypeLib.OID;

public class AutoDescriptor : OIDDescriptorBase
{
    private const string BaseNamespace = "base:";

    public AutoDescriptor()
    {
        var oid = $"_auto-{DateTime.UtcNow.Ticks}";

        AbsoluteOID = new Uri(BaseNamespace + oid);
    }

    public override Uri AbsoluteOID { get; }

    public override int CompareTo(object? obj)
    {
        return AbsoluteOID.AbsolutePath.CompareTo(obj);
    }
}