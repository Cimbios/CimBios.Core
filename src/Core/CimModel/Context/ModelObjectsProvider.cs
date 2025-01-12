using CimBios.Core.CimModel.CimDatatypeLib;
using CimBios.Core.CimModel.RdfSerializer;
using CimBios.Core.CimModel.Schema;
using CimBios.Core.DataProvider;

namespace CimBios.Core.CimModel.Context;

/// <summary>
/// Factory provides abstract dataprovider and serializer for model context.
/// </summary>
public interface IModelObjectsProvider
{
    /// <summary>
    /// Data provider with source.
    /// </summary>
    public IDataProvider DataProvider { get; }

    /// <summary>
    /// Rdf data serializer with data provider.
    /// </summary>
    public RdfSerializerBase Serializer { get; }

    /// <summary>
    /// CIM schema shapes.
    /// </summary>
    public ICimSchema CimSchema { get; }

    /// <summary>
    /// Library of CIM class types.
    /// </summary>
    public IDatatypeLib TypeLib { get; }
}

public interface IModelObjectsProviderFactory
{
    public IModelObjectsProvider Create(Uri source, 
        ICimSchema schema, IDatatypeLib? typeLib = null);
}

public class RdfXmlFileModelObjectsProvider : IModelObjectsProvider
{
    public IDataProvider DataProvider { get; }

    public RdfSerializerBase Serializer { get; }

    public ICimSchema CimSchema { get; }

    public IDatatypeLib TypeLib { get; }

    public RdfXmlFileModelObjectsProvider(Uri source, 
        ICimSchema schema, 
        IDatatypeLib? typeLib = null)
    {
        var provider = new FileStreamDataProvider(source);
        DataProvider = provider;
        CimSchema = schema;
        
        if (typeLib == null)
        {
            TypeLib = new DatatypeLib();
        }
        else
        {
            TypeLib = typeLib;
        }

        Serializer = new RdfXmlSerializer(provider, schema, TypeLib);
    }
}

public class RdfXmlFileModelObjectsProviderFactory 
    : IModelObjectsProviderFactory
{
    public IModelObjectsProvider Create(Uri source, 
        ICimSchema schema, IDatatypeLib? typeLib = null)
    {
        return new RdfXmlFileModelObjectsProvider(source, schema, typeLib);
    }
}