using CimBios.Core.RdfIOLib;

namespace CimBios.Core.CimModel.CimDatatypeLib.OID;

public class UuidDescriptor : OIDDescriptorBase
{
    public const string DefaultNamespace = "urn:uuid:";

    public Guid Uuid { get; }

    public override Uri AbsoluteOID { get; }

    public override bool IsEmpty => Uuid == Guid.Empty;

    public UuidDescriptor (Uri absoluteOID)
    {
        if (RdfUtils.TryGetEscapedIdentifier(absoluteOID, out var oid)
            && Guid.TryParse(oid.Replace(UuidPrefix, ""), out var uuid))
        {
            Uuid = uuid;
            AbsoluteOID = new Uri(DefaultNamespace + UuidPrefix 
                + uuid.ToString().ToLower());
    
            return;
        }

        throw new ArgumentException($"Incorrect UUID uri {absoluteOID}!");
    }    

    public UuidDescriptor (Guid value, string ns)
    {        
        Uuid = value;

        AbsoluteOID = new Uri(ns + this);
    }  

    public UuidDescriptor (Guid value) : this (value, DefaultNamespace)
    {
    }

    public UuidDescriptor() : this (Guid.NewGuid())
    {
    }

    public UuidDescriptor (string value) : this (Guid.Parse(value))
    {
    }

    public override int GetHashCode()
    {
        return base.GetHashCode() ^ Uuid.GetHashCode();
    }

    public override int CompareTo (object? obj)
    {
        if (obj is not UuidDescriptor uuidDescriptor)
        {
            return base.CompareTo(obj);
        }

        return Uuid.CompareTo(uuidDescriptor.Uuid);
    }

    public override string ToString()
    {
        return Uuid.ToString().ToLower();
    }

    private const string UuidPrefix = "#_";
}

public class UuidDescriptorFactory : IOIDDescriptorFactory
{
    public string Namespace { get; } = UuidDescriptor.DefaultNamespace;

    public UuidDescriptorFactory ()
    {
        
    }

    public UuidDescriptorFactory (string ns)
    {
        Namespace = ns;
    }

    public IOIDDescriptor Create()
    {
        return new UuidDescriptor();
    }

    public IOIDDescriptor Create(string value)
    {
        return new UuidDescriptor(value);
    }

    public IOIDDescriptor Create(Uri value)
    {
        return new UuidDescriptor(value);
    }

    public IOIDDescriptor? TryCreate()
    {
        try
        {
            return Create();
        }
        catch
        {
            return null;
        }
    }

    public IOIDDescriptor? TryCreate(string value)
    {
        try
        {
            return Create(value);
        }
        catch
        {
            return null;
        }
    }

    public IOIDDescriptor? TryCreate(Uri value)
    {
        try
        {
            return Create(value);
        }
        catch
        {
            return null;
        }
    }
}