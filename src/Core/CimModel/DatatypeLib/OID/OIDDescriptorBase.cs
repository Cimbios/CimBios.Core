namespace CimBios.Core.CimModel.CimDatatypeLib.OID;

/// <summary>
/// Base ToString and GetHashCode functionality for OID Descriptors.
/// </summary>
/// <typeparam name="T">Not null generic type</typeparam>
public abstract class OIDDescriptorBase : IOIDDescriptor
{
    public abstract Uri AbsoluteOID { get; }

    public virtual bool IsEmpty => AbsoluteOID.Fragment.Length == 0 
        && AbsoluteOID.LocalPath.Length == 0;

    public virtual int CompareTo(object? obj)
    {
        if (obj is not IOIDDescriptor oidDescriptor)
        {
            throw new InvalidCastException("Only IOIDDescriptor can be comparable!");
        }

        return AbsoluteOID.AbsoluteUri
            .CompareTo(oidDescriptor.AbsoluteOID.AbsoluteUri);
    }

    public bool Equals(IOIDDescriptor? other)
    {
        return AbsoluteOID.AbsoluteUri == other?.AbsoluteOID.AbsoluteUri;
    }

    public override int GetHashCode()
    {
        return AbsoluteOID.AbsoluteUri.GetHashCode();
    }

    public override string ToString()
    {
        var stringVal = AbsoluteOID.ToString();
        if (stringVal == null)
        {
            throw new NotSupportedException();
        }

        return stringVal;
    }

    public static implicit operator string (OIDDescriptorBase descriptor)
    {
        return descriptor.ToString();
    }

    public static implicit operator Uri (OIDDescriptorBase descriptor)
    {
        return descriptor.AbsoluteOID;
    }
}
