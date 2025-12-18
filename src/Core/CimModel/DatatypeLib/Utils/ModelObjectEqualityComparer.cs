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

using CimBios.Core.CimModel.DatatypeLib.ModelObject;

namespace CimBios.Core.CimModel.DatatypeLib.Utils;

/// <summary>
///     Equality comparer class for only model objects OID comparision.
/// </summary>
public class ModelObjectOIDEqualityComparer : IEqualityComparer<IModelObject>
{
    public bool Equals(IModelObject? left, IModelObject? right)
    {
        if (left == null && right == null) return true;

        if (left != null && right != null) return left.OID.Equals(right.OID);

        return false;
    }

    public int GetHashCode(IModelObject obj)
    {
        return obj.OID.GetHashCode();
    }
}