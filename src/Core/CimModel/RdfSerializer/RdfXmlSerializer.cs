using CimBios.Core.CimModel.CimDatatypeLib;
using CimBios.Core.CimModel.CimDatatypeLib.OID;
using CimBios.Core.CimModel.Schema;
using CimBios.Core.RdfIOLib;

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
        IOIDDescriptorFactory? oidDescriptorFactory = null)
        : base(schema, datatypeLib)
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

    public RdfSerializerBase Create(ICimSchema cimSchema,
        ICimDatatypeLib typeLib,
        IOIDDescriptorFactory? oidDescriptorFactory = null)
    {
        var serializer = new RdfXmlSerializer(cimSchema,
            typeLib, oidDescriptorFactory)
        {
            Settings = Settings
        };

        return serializer;
    }
}