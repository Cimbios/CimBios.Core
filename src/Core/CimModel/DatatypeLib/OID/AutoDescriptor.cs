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