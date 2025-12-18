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

public abstract class RdfReaderBase
    : RdfNamespacesContainerBase
{
    /// <summary>
    ///     Parse string rdf/xml content.
    /// </summary>
    /// <param name="content">String rdf/xml content.</param>
    /// <param name="encoding"></param>
    public abstract void Parse(string content, Encoding? encoding = null);

    /// <summary>
    ///     Load rdf/xml content from TextReader.
    /// </summary>
    public abstract void Load(TextReader textReader);

    /// <summary>
    ///     Load rdf/xml content from XmlReader.
    /// </summary>
    public abstract void Load(XmlReader xmlReader);

    /// <summary>
    ///     Close reader.
    /// </summary>
    public abstract void Close();

    /// <summary>
    ///     Read RDF content of next element.
    /// </summary>
    /// <returns>RDF node of last read element.</returns>
    public abstract RdfNode? ReadNext();

    /// <summary>
    /// Read all elements in document.
    /// </summary>
    /// <returns>Enumerable of RDF nodes</returns>
    public abstract IEnumerable<RdfNode> ReadAll();
}