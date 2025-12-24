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

public class TextDescriptor : OIDDescriptorBase
{
    public const string DefaultNamespace = "base:";

    public TextDescriptor(Uri absoluteOID)
    {
        if (RdfUtils.TryGetEscapedIdentifier(absoluteOID, out var oid))
        {
            TextOID = oid;
            AbsoluteOID = absoluteOID;
            return;
        }

        throw new ArgumentException($"Incorrect UUID uri {absoluteOID}!");
    }

    public TextDescriptor(string value)
        : this(new Uri(DefaultNamespace + value))
    {
        if (value == string.Empty)
            throw new NotSupportedException(
                "TextDescriptor cannot be empty string.");
    }

    public string TextOID { get; }
    public override Uri AbsoluteOID { get; }

    public override bool IsEmpty => TextOID.Length == 0;

    public override string ToString()
    {
        return TextOID;
    }

    public override int CompareTo(object? obj)
    {
        if (obj is not TextDescriptor textDescriptor) return base.CompareTo(obj);

        return TextOID.CompareTo(textDescriptor.TextOID);
    }
}

public class TextDescriptorFactory : IOIDDescriptorFactory
{
    public string Namespace => TextDescriptor.DefaultNamespace;

    public IOIDDescriptor Create()
    {
        return new TextDescriptor(new AutoDescriptor().AbsoluteOID);
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