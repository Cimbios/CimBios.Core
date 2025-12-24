/*
*    CimBios.Core - Common Information Model (IEC61970) I/O Library
*    Copyright (C) 2025 Yuri A. Kovalenko a.k.a belizahrt <belizahrt@gmail.com>
*
*    This program is free software: you can redistribute it and/or modify
*    it under the terms of the GNU General Public License as published by
*    the Free Software Foundation, either version 3 of the License, or
*    (at your option) any later version.
*
*    This program is distributed in the hope that it will be useful,
*    but WITHOUT ANY WARRANTY; without even the implied warranty of
*    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
*    GNU General Public License for more details.
*
*    You should have received a copy of the GNU General Public License
*    along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/

using CimBios.Core.RdfIOLib;

namespace CimBios.Core.CimModel.DatatypeLib.OID;

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
            AbsoluteOID = new(DefaultNamespace + UuidPrefix 
                + uuid.ToString().ToLower());
    
            return;
        }

        throw new ArgumentException($"Incorrect UUID uri {absoluteOID}!");
    }    

    public UuidDescriptor (Guid value, string ns = DefaultNamespace)
    {        
        Uuid = value;

        AbsoluteOID = new(ns + UuidPrefix + ToString().ToLower());
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

    public override bool Equals(IOIDDescriptor? other)
    {
        if (other is not UuidDescriptor uuidDescriptor)
        {
            return base.Equals(other);
        }

        return Uuid.Equals(uuidDescriptor.Uuid);
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
