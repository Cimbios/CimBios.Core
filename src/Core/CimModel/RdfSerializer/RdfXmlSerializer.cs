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

    public RdfXmlSerializer(IDataProvider provider,
        ICimSchema schema, ICimDatatypeLib datatypeLib)
        : base(provider, schema, datatypeLib)
    {
        _rdfReader = new RdfXmlReader(provider.Source);
        _rdfWriter = new RdfXmlWriter();
    }

    public RdfXmlSerializer(string path, ICimSchema schema, 
        ICimDatatypeLib datatypeLib)
        : base(new FileStreamDataProvider(new Uri(path)), schema, datatypeLib)
    {
        _rdfReader = new RdfXmlReader(new Uri(path));
        _rdfWriter = new RdfXmlWriter();
    }

    private RdfReaderBase _rdfReader;
    private RdfWriterBase _rdfWriter;
}
