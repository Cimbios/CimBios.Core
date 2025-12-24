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

namespace CimBios.Core.RdfIOLib.RdfXml;

/// <summary>
///     Writer for rdf/xml formatted data.
///     Converts data from RDF-Triple format to XDocument.
/// </summary>
public class RdfXmlWriter : RdfWriterBase
{
    private bool _excludeBase;

    private XmlWriter? _xmlWriter;

    /// <summary>
    ///     Default constructor, needs namespaces from Schema to function properly
    /// </summary>
    public RdfXmlWriter()
    {
    }

    private XmlWriter XmlWriter
    {
        get
        {
            if (_xmlWriter == null)
                throw new InvalidOperationException(
                    "XmlWriter has not been initialized!");

            return _xmlWriter;
        }
    }

    public override void Open(TextWriter textWriter,
        bool excludeBase = true, Encoding? encoding = null)
    {
        encoding ??= Encoding.Default;

        var xmlWriter = XmlWriter.Create(textWriter,
            new XmlWriterSettings
            {
                Indent = true,
                CloseOutput = true,
                Encoding = encoding
            }
        );

        Open(xmlWriter, excludeBase);
    }

    public override void Open(XmlWriter xmlWriter,
        bool excludeBase = true)
    {
        _xmlWriter = xmlWriter;
        _excludeBase = excludeBase;

        if (_xmlWriter.WriteState is WriteState.Closed or WriteState.Error)
            throw new Exception("XmlWriter has not been initialized!");

        if (WriteRdfRootNode() == false) throw new Exception("Failed to write rdf:RDF root node!");
    }

    public override void Close()
    {
        if (XmlWriter.WriteState == WriteState.Closed
            || XmlWriter.WriteState == WriteState.Error)
            return;

        XmlWriter.WriteEndElement();
        XmlWriter.WriteEndDocument();
        XmlWriter.Close();
    }

    public override void Write(RdfNode rdfNode)
    {
        var nodeName = UriToName(rdfNode.TypeIdentifier);
        WriteElementHeader(
            nodeName.prefix,
            nodeName.name);

        var iri = NormalizeIris ? NormalizeIdentifier(rdfNode.Identifier) 
            : rdfNode.Identifier.AbsoluteUri;

        if (rdfNode.IsAuto == false)
            XmlWriter.WriteAttributeString(
                "rdf",
                RdfIRIMode == RdfIRIModeKind.About ? "about" : "ID",
                Rdf, iri);

        foreach (var triple in rdfNode.Triples)
        {
            var (prefix, name) = UriToName(triple.Predicate);
            WriteElementHeader(prefix, name);

            if (triple.Object is RdfTripleObjectUriContainer uriContainer)
                XmlWriter.WriteAttributeString(
                    "rdf", "resource", Rdf,
                    NormalizeIris ? NormalizeIdentifier(uriContainer.UriObject) 
                        : uriContainer.UriObject.AbsoluteUri);
            
            else if (triple.Object is RdfTripleObjectStatementsContainer statements)
                foreach (var statement in statements.RdfNodesObject)
                    Write(statement);
            
            else if (triple.Object is RdfTripleObjectLiteralContainer literal)
                XmlWriter.WriteString(literal.LiteralObject);

            XmlWriter.WriteEndElement();
        }

        XmlWriter.WriteEndElement();
    }

    public override void WriteAll(IEnumerable<RdfNode> rdfNodes)
    {
        foreach (var rdfNode in rdfNodes) Write(rdfNode);

        Close();
    }

    /// <summary>
    /// </summary>
    /// <param name="prefix"></param>
    /// <param name="name"></param>
    private void WriteElementHeader(string prefix, string name)
    {
        if (prefix == "base")
            XmlWriter.WriteStartElement(name);
        else
            XmlWriter.WriteStartElement(
                prefix,
                name,
                Namespaces[prefix].AbsoluteUri);
    }

    /// <summary>
    ///     Creates header-node with all on XML-RDF namespaces
    /// </summary>
    private bool WriteRdfRootNode()
    {
        if (CanWriteNext() == false) return false;

        XmlWriter.WriteStartDocument();
        XmlWriter.WriteStartElement("rdf", "RDF", Rdf);

        foreach (var ns in Namespaces)
        {
            if (_excludeBase && ns.Key == "base") continue;

            XmlWriter.WriteAttributeString(
                ns.Key == "base" ? "xml" : "xmlns",
                ns.Key,
                ns.Key == "base" ? Xml : Xmlns,
                ns.Value.AbsoluteUri);
        }

        return true;
    }

    /// <summary>
    ///     Get writing ability status.
    /// </summary>
    /// <returns>True if writing is available.</returns>
    private bool CanWriteNext()
    {
        return _xmlWriter != null
               && _xmlWriter.WriteState != WriteState.Closed
               && _xmlWriter.WriteState != WriteState.Error;
    }

    /// <summary>
    ///     Makes string identifier with escaped symbols.
    /// </summary>
    /// <param name="uri"></param>
    /// <returns></returns>
    private string NormalizeIdentifier(Uri uri)
    {
        if ((RdfUtils.TryGetEscapedIdentifier(uri, out var rid)
             && Namespaces.Values.Contains(uri)) || uri.Scheme == "base")
        {
            var prefix = Namespaces.FirstOrDefault(ns => ns.Value == uri).Key;

            if (prefix == "base" || uri.Scheme == "base") return rid;

            return $"{prefix}:{rid}";
        }

        return uri.AbsoluteUri;
    }
}
