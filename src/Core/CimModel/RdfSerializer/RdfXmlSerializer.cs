using CimBios.Core.CimModel.CimDatatypeLib;
using CimBios.Core.CimModel.Schema;
using CimBios.Core.DataProvider;
using CimBios.Core.RdfIOLib;

namespace CimBios.Core.CimModel.RdfSerializer;

/// <summary>
/// CIM Rdf/Xml serializer implementation. Based on RdfXmlIOLib.
/// </summary>
public class RdfXmlSerializer : RdfSerializerBase
{
    protected override RdfReaderBase _RdfReader => _rdfReader;
    protected override RdfWriterBase _RdfWriter => _rdfWriter;

    public RdfXmlSerializer(ICimSchema schema, ICimDatatypeLib datatypeLib)
        : base(schema, datatypeLib)
    {
        _rdfReader = new RdfXmlReader();
        _rdfWriter = new RdfXmlWriter();
    }

    private RdfReaderBase _rdfReader;
    private RdfWriterBase _rdfWriter;
}
