using CimBios.Core.CimModel.CimDatatypeLib;
using CimBios.Core.CimModel.CimDatatypeLib.OID;
using CimBios.Core.CimModel.Schema;

namespace CimBios.Core.CimModel.RdfSerializer;

/// <summary>
/// Serializer factory.
/// </summary>
public interface IRdfSerializerFactory
{
    public RdfSerializerSettings Settings { get; set; }

    public RdfSerializerBase Create(ICimSchema cimSchema, 
        ICimDatatypeLib typeLib, IOIDDescriptorFactory? oidDescriptorFactory);
}
