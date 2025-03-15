using CimBios.Core.RdfIOLib;

namespace CimBios.Core.CimModel.CimDatatypeLib.OID;

public class GuidDescriptor : OIDDescriptorBase
{
    public const string DefaultNamespace = "urn:uuid:";

    public Guid Uuid { get; }

    public override Uri AbsoluteOID { get; }

    public override bool IsEmpty => Uuid == Guid.Empty;

    public GuidDescriptor (Uri absoluteOID)
    {
        if (RdfUtils.TryGetEscapedIdentifier(absoluteOID, out var oid)
            && Guid.TryParse(oid.Replace("#_", ""), out var uuid))
        {
            Uuid = uuid;
            AbsoluteOID = absoluteOID;
            return;
        }

        throw new ArgumentException($"Incorrect UUID uri {absoluteOID}!");
    }    

    public GuidDescriptor (Guid value, string ns)
    {        
        Uuid = value;

        AbsoluteOID = new Uri(ns + this);
    }  

    public GuidDescriptor (Guid value) : this (value, DefaultNamespace)
    {
    }

    public GuidDescriptor() : this (Guid.NewGuid())
    {
    }

    public GuidDescriptor (string value) : this (Guid.Parse(value))
    {
    }

    public override int GetHashCode()
    {
        return base.GetHashCode() ^ Uuid.GetHashCode();
    }

    public override int CompareTo (object? obj)
    {
        return Uuid.CompareTo(obj);
    }

    public override string ToString()
    {
        return Uuid.ToString().ToLower();
    }
}

public class GuidDescriptorFactory : IOIDDescriptorFactory
{
    public string Namespace { get; } = GuidDescriptor.DefaultNamespace;

    public GuidDescriptorFactory ()
    {
        
    }

    public GuidDescriptorFactory (string ns)
    {
        Namespace = ns;
    }

    public IOIDDescriptor Create()
    {
        return new GuidDescriptor();
    }

    public IOIDDescriptor Create(string value)
    {
        return new GuidDescriptor(value);
    }

    public IOIDDescriptor Create(Uri value)
    {
        return new GuidDescriptor(value);
    }
}