using CimBios.Core.CimModel.RdfSerializer;
using CimBios.Core.DataProvider;

namespace CimBios.Core.CimModel.Context;

/// <summary>
/// Factory provides abstract dataprovider and serializer for model context.
/// </summary>
public interface IModelContextDataFactory
{
    /// <summary>
    /// Data provider with source.
    /// </summary>
    public IDataProvider DataProvider { get; set; }

    /// <summary>
    /// Rdf data serializer with data provider.
    /// </summary>
    public RdfSerializerBase Serializer { get; set; }
}

public class RdfXmlFileModelContextFactory : IModelContextDataFactory
{
    public IDataProvider DataProvider { get; set; }

    public RdfSerializerBase Serializer { get; set; }

    public RdfXmlFileModelContextFactory(Uri source)
    {
        var provider = new RdfXmlFileDataProvider(source);
        DataProvider = provider;
        Serializer = new RdfXmlSerializer(provider);
    }
}
