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

namespace CimBios.Core.RdfIOLib;

/// <summary>
///     Helper class for rdf entities.
/// </summary>
public static class RdfUtils
{
    public static T? ExtractPredicateValue<T>(RdfNode node,
        Uri predicate) where T : RdfTripleObjectContainerBase
    {
        var triples = node.Triples
            .Where(t => RdfUriEquals(t.Predicate, predicate));
        if (triples.Any() && triples.First().Object is T value) return value;

        return null;
    }

    /// <summary>
    ///     Equality respects URI fragments comparision.
    /// </summary>
    public static bool RdfUriEquals(Uri? lUri, Uri? rUri)
    {
        if (lUri == null && rUri == null) return true;

        if (lUri != null && rUri != null) return lUri.AbsoluteUri == rUri.AbsoluteUri;

        return false;
    }

    /// <summary>
    ///     Get short string form of URI.
    /// </summary>
    /// <param name="uri">Resource identifier.</param>
    /// <param name="identifier">Escaped identifier. Empty if conversion fails.</param>
    /// <returns>True if identifier</returns>
    public static bool TryGetEscapedIdentifier(Uri uri, out string identifier)
    {
        identifier = string.Empty;

        var allowedNamespaces = new HashSet<string>
        {
            "urn", "base"
        };

        if (allowedNamespaces.Contains(uri.Scheme))
        {
            identifier = uri.AbsoluteUri.Split(':').Last();
            return true;
        }

        if (uri.Fragment != string.Empty)
        {
            identifier = uri.Fragment
                .Replace("#", "");

            return true;
        }

        if (uri.LocalPath != string.Empty)
        {
            identifier = uri.LocalPath.Replace("/", "");
            return true;
        }

        return false;
    }
}

public class RdfUriComparer : EqualityComparer<Uri>
{
    public override bool Equals(Uri? lUri, Uri? rUri)
    {
        return RdfUtils.RdfUriEquals(lUri, rUri);
    }

    public override int GetHashCode(Uri uri)
    {
        return uri.AbsoluteUri.GetHashCode();
    }
}