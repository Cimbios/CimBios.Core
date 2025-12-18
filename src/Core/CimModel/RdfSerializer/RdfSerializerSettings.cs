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

namespace CimBios.Core.CimModel.RdfSerializer;

/// <summary>
///     Rdf serializer settings.
/// </summary>
/// <param name="unknownClassesAllowed">
///     Create auto class instances
///     of unknown classes.
/// </param>
/// <param name="unknownPropertiesAllowed">
///     Create and read
///     auto property instances.
/// </param>
/// <param name="includeUnresolvedReferences">
///     Include unresolved references
///     for objects while read/write.
/// </param>
///  <param name="includeBaseNamespace">
///     Include xml:base namespace definition.
/// </param>
/// <param name="normalizeIris">
///     Normalize uri form - change absolute uri with resolved prefix.
/// </param>
/// <param name="iriMode">rdf:_iri_ mode.</param>
public sealed class RdfSerializerSettings(
    bool unknownClassesAllowed = false,
    bool unknownPropertiesAllowed = false,
    bool includeUnresolvedReferences = true,
    bool includeBaseNamespace = false,
    bool normalizeIris = true,
    RdfIRIModeKind iriMode = RdfIRIModeKind.About)
{
    /// <summary>
    ///     Create auto class instances of unknown classes.
    /// </summary>
    public bool UnknownClassesAllowed { get; set; }
        = unknownClassesAllowed;

    /// <summary>
    ///     Create and read auto property instances.
    /// </summary>
    public bool UnknownPropertiesAllowed { get; set; }
        = unknownPropertiesAllowed;

    /// <summary>
    ///     Include unresolved references for objects while read/write.
    /// </summary>
    public bool IncludeUnresolvedReferences { get; set; }
        = includeUnresolvedReferences;

    /// <summary>
    ///     Include xml:base namespace definition.
    /// </summary>
    public bool IncludeBaseNamespace { get; set; }
        = includeBaseNamespace;
    
    /// <summary>
    ///     Normalize uri form - change absolute uri with resolved prefix.
    /// </summary>
    public bool NormalizeIris { get; set; }
        = normalizeIris;

    /// <summary>
    ///     rdf:_iri_ mode.
    /// </summary>
    public RdfIRIModeKind WritingIRIMode { get; set; } = iriMode;
}