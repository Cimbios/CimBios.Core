using CimBios.Core.RdfIOLib;

namespace CimBios.Core.CimModel.CimDatatypeLib.OID;

public class TextDescriptor : OIDDescriptorBase
{
    public const string DefaultNamespace = "base:";

    public string TextOID { get; }
    public override Uri AbsoluteOID { get; }

    public override bool IsEmpty => TextOID.Length == 0;

    public TextDescriptor (Uri absoluteOID)
    {
        if (RdfUtils.TryGetEscapedIdentifier(absoluteOID, out var oid))
        {
            TextOID = oid;
            AbsoluteOID = absoluteOID;
            return;
        }

        throw new ArgumentException($"Incorrect UUID uri {absoluteOID}!");
    }

    public TextDescriptor (string value)
        : this (new Uri(DefaultNamespace + value))
    {
    }

    public override string ToString()
    {
        return TextOID;
    }

    public override int CompareTo(object? obj)
    {
        if (obj is not TextDescriptor textDescriptor)
        {
            return base.CompareTo(obj);
        }

        return TextOID.CompareTo(textDescriptor.TextOID);
    }
}

public class TextDescriptorFactory : IOIDDescriptorFactory
{
    public string Namespace => TextDescriptor.DefaultNamespace;

    public IOIDDescriptor Create()
    {
        throw new NotSupportedException("TextDescriptor cannot be empty string.");
    }

    public IOIDDescriptor Create(string value)
    {
        return new TextDescriptor(value);
    }

    public IOIDDescriptor Create(Uri value)
    {
        return new TextDescriptor(value);
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