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

using System.Text;
using System.Xml;

namespace CimBios.Core.RdfIOLib;

public abstract class RdfWriterBase
    : RdfNamespacesContainerBase
{
    public RdfIRIModeKind RdfIRIMode { get; set; } = RdfIRIModeKind.About;

    /// <summary>
    ///     Normalize uri form - change absolute uri with resolved prefix.
    /// </summary>
    public bool NormalizeIris { get; set; } = false;

    /// <summary>
    ///     Open rdf/xml content from TextWriter.
    /// </summary>
    public abstract void Open(TextWriter textWriter,
        bool excludeBase = true, Encoding? encoding = null);

    /// <summary>
    ///     Open rdf/xml content from XmlWriter.
    /// </summary>
    public abstract void Open(XmlWriter xmlWriter,
        bool excludeBase = true);

    /// <summary>
    ///     End of rdf document writing.
    /// </summary>
    public abstract void Close();

    /// <summary>
    ///     Write RdfNode to XmlWriter stream
    /// </summary>
    /// <param name="rdfNode"></param>
    /// <param name="excludeBase"></param>
    /// <returns>Serialized model XDocument</returns>
    public abstract void Write(RdfNode rdfNode);

    /// <summary>
    ///     Writes RdfNodes list to XmlWriter stream
    /// </summary>
    /// <param name="rdfNodes"></param>
    /// <param name="excludeBase"></param>
    /// <returns>Serialized model XDocument</returns>
    public abstract void WriteAll(IEnumerable<RdfNode> rdfNodes);
}

public enum RdfIRIModeKind
{
    About,
    ID
}