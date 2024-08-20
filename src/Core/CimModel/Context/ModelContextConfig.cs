using CimBios.Core.CimModel.CimDatatypeLib;
using CimBios.Core.CimModel.RdfSerializer;
using CimBios.Core.CimModel.Schema;
using CimBios.Core.DataProvider;

namespace CimBios.Core.CimModel.Context;

/// <summary>
/// Factory provides abstract dataprovider and serializer for model context.
/// </summary>
public interface IModelContextConfig
{
    /// <summary>
    /// Data provider with source.
    /// </summary>
    public IDataProvider DataProvider { get; }

    /// <summary>
    /// Rdf data serializer with data provider.
    /// </summary>
    public RdfSerializerBase Serializer { get; }

    public ICimSchema? CimSchema { get; }

    public IDatatypeLib TypeLib { get; }
}

public class RdfXmlFileModelContextConfig : IModelContextConfig
{
    public IDataProvider DataProvider { get; }

    public RdfSerializerBase Serializer { get; }

    public ICimSchema? CimSchema { get; }

    public IDatatypeLib TypeLib { get; }

    public RdfXmlFileModelContextConfig(Uri source, 
        ICimSchema? schema = null, 
        IDatatypeLib? typeLib = null)
    {
        var provider = new RdfXmlFileDataProvider(source);
        DataProvider = provider;
        Serializer = new RdfXmlSerializer(provider);
        CimSchema = schema;
        
        if (typeLib == null)
        {
            TypeLib = new DatatypeLib();
        }
        else
        {
            TypeLib = typeLib;
        }
    }
}
