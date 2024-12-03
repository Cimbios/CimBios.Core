using System.Text.RegularExpressions;

namespace CimBios.Core.CimModel.Schema.AutoSchema;

/// <summary>
/// Recognize primitive datatype class.
/// </summary>
internal static class LiteralValueTypeRecognizer
{
    /// <summary>
    /// Recognize primitive datatype by string literal value.
    /// </summary>
    /// <param name="literalValue">String literal value.</param>
    /// <returns>XSD type Uri.</returns>
    internal static Uri Recognize(string literalValue)
    {
        foreach (var (typeUri, pattern) in _PatternsMap)
        {
            if (Regex.IsMatch(literalValue, pattern, 
                RegexOptions.Compiled))
            {
                return new(typeUri);
            }
        }

        return new(XmlDatatypesMapping.StringUri);
    }

    /// <summary>
    /// Get superset type order. bool < integer < double < ...
    /// </summary>
    /// <param name="typeUri">String type uri.</param>
    /// <returns>Order num.</returns>
    internal static int GetTypeSetOrder(string typeUri)
    {
        if (typeUri == XmlDatatypesMapping.StringUri)
        {
            return int.MaxValue;
        }

        for (int i = 0; i < _PatternsMap.Count(); ++i)
        {
            if (_PatternsMap[i].typeUri == typeUri)
            {
                return i;
            }
        }

        return int.MinValue;
    }

    /// <summary>
    /// Each next element should overset previous.
    /// </summary>
    private static readonly (string typeUri, string pattern)[] _PatternsMap = 
    [
        (XmlDatatypesMapping.BooleanUri, @"^(?i)(true|false)$"),
        (XmlDatatypesMapping.IntegerUri, @"^[+-]?\b[0-9]+\b$"),
        (XmlDatatypesMapping.DoubleUri, @"^[-+]?[0-9]+([eE][-+]?[0-9]+)*$"),
        (XmlDatatypesMapping.DoubleUri, @"^[-+]?[0-9]+\.[0-9]*([eE][-+]?[0-9]+)*$"),
        (XmlDatatypesMapping.DateTimeUri, @"^[0-9]{4}-[0-9]{2}-[0-9]{2}T[0-9]{2}:[0-9]{2}:[0-9]{2}Z$"),
    ];
}
