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
///     Model object identifier descriptor interface.
/// </summary>
public interface IOIDDescriptor : IComparable, IEquatable<IOIDDescriptor>
{
    /// <summary>
    ///     Full absolute uri formatted descriptor.
    /// </summary>
    public Uri AbsoluteOID { get; }

    /// <summary>
    ///     Is empty value OID.
    /// </summary>
    public bool IsEmpty { get; }

    /// <summary>
    ///     Not null string representation.
    /// </summary>
    /// <returns>Text return.</returns>
    public string ToString();
}

/// <summary>
///     OID Descriptor creation factory interface.
/// </summary>
public interface IOIDDescriptorFactory
{
    public string Namespace { get; }

    public IOIDDescriptor Create();
    public IOIDDescriptor Create(string value);
    public IOIDDescriptor Create(Uri value);

    public IOIDDescriptor? TryCreate();
    public IOIDDescriptor? TryCreate(string value);
    public IOIDDescriptor? TryCreate(Uri value);
}