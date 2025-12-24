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

namespace CimBios.Core.CimModel.DatatypeLib.OID;

/// <summary>
///     Base ToString and GetHashCode functionality for OID Descriptors.
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
            throw new InvalidCastException("Only IOIDDescriptor can be comparable!");

        return AbsoluteOID.AbsoluteUri
            .CompareTo(oidDescriptor.AbsoluteOID.AbsoluteUri);
    }

    public virtual bool Equals(IOIDDescriptor? other)
    {
        return AbsoluteOID.AbsoluteUri == other?.AbsoluteOID.AbsoluteUri;
    }

    public override string ToString()
    {
        var stringVal = AbsoluteOID.ToString();
        if (stringVal == null) throw new NotSupportedException();

        return stringVal;
    }

    public override int GetHashCode()
    {
        return AbsoluteOID.AbsoluteUri.GetHashCode();
    }

    public static implicit operator string(OIDDescriptorBase descriptor)
    {
        return descriptor.ToString();
    }

    public static implicit operator Uri(OIDDescriptorBase descriptor)
    {
        return descriptor.AbsoluteOID;
    }
}