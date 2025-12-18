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

using CimBios.Core.CimModel.DatatypeLib;
using CimBios.Core.CimModel.DatatypeLib.OID;
using CimBios.Core.CimModel.Schema;
using CimBios.Core.RdfIOLib;
using CimBios.Core.RdfIOLib.RdfXml;
using Serilog;

namespace CimBios.Core.CimModel.RdfSerializer;

/// <summary>
///     CIM Rdf/Xml serializer implementation. Based on RdfXmlIOLib.
/// </summary>
public class RdfXmlSerializer : RdfSerializerBase
{
    private readonly IOIDDescriptorFactory _oidDescriptorFactory;

    private readonly RdfReaderBase _rdfReader;
    private readonly RdfWriterBase _rdfWriter;

    public RdfXmlSerializer(ICimSchema schema, ICimDatatypeLib datatypeLib,
        IOIDDescriptorFactory? oidDescriptorFactory = null, ILogger? logger=null)
        : base(schema, datatypeLib, logger)
    {
        _rdfReader = new RdfXmlReader();
        _rdfWriter = new RdfXmlWriter();

        if (oidDescriptorFactory == null)
            _oidDescriptorFactory = new UuidDescriptorFactory();
        else
            _oidDescriptorFactory = oidDescriptorFactory;
    }

    protected override RdfReaderBase RdfReader => _rdfReader;
    protected override RdfWriterBase RdfWriter => _rdfWriter;

    protected override IOIDDescriptorFactory OidDescriptorFactory
        => _oidDescriptorFactory;
}

public class RdfXmlSerializerFactory : IRdfSerializerFactory
{
    public RdfSerializerSettings Settings { get; set; } = new();

    public IRdfSerializer Create(ICimSchema cimSchema,
        ICimDatatypeLib typeLib,
        IOIDDescriptorFactory? oidDescriptorFactory = null,
        ILogger? logger=null)
    {
        var serializer = new RdfXmlSerializer(cimSchema,
            typeLib, oidDescriptorFactory, logger)
        {
            Settings = Settings
        };

        return serializer;
    }
}