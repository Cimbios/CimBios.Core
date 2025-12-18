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

using System.Text.RegularExpressions;

namespace CimBios.Core.CimModel.Schema.AutoSchema;

/// <summary>
///     Recognize primitive datatype class.
/// </summary>
internal static class LiteralValueTypeRecognizer
{
    /// <summary>
    ///     Each next element should overset previous.
    /// </summary>
    private static readonly (string typeUri, string pattern)[] _PatternsMap =
    [
        (XmlDatatypesMapping.BooleanUri, @"^(?i)(true|false)$"),
        (XmlDatatypesMapping.IntegerUri, @"^[+-]?\b[0-9]+\b$"),
        (XmlDatatypesMapping.DoubleUri, @"^[-+]?[0-9]+([eE][-+]?[0-9]+)*$"),
        (XmlDatatypesMapping.DoubleUri, @"^[-+]?[0-9]+\.[0-9]*([eE][-+]?[0-9]+)*$"),
        (XmlDatatypesMapping.DateTimeUri, @"^[0-9]{4}-[0-9]{2}-[0-9]{2}T[0-9]{2}:[0-9]{2}:[0-9]{2}Z$")
    ];

    /// <summary>
    ///     Recognize primitive datatype by string literal value.
    /// </summary>
    /// <param name="literalValue">String literal value.</param>
    /// <returns>XSD type Uri.</returns>
    internal static Uri Recognize(string literalValue)
    {
        foreach (var (typeUri, pattern) in _PatternsMap)
            if (Regex.IsMatch(literalValue, pattern,
                    RegexOptions.Compiled))
                return new Uri(typeUri);

        return new Uri(XmlDatatypesMapping.StringUri);
    }

    /// <summary>
    ///     Get superset type order. bool < integer < double < ...
    /// </summary>
    /// <param name="typeUri">String type uri.</param>
    /// <returns>Order num.</returns>
    internal static int GetTypeSetOrder(string typeUri)
    {
        if (typeUri == XmlDatatypesMapping.StringUri) return int.MaxValue;

        for (var i = 0; i < _PatternsMap.Count(); ++i)
            if (_PatternsMap[i].typeUri == typeUri)
                return i;

        return int.MinValue;
    }
}