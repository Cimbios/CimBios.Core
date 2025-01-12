using CimBios.Core.CimModel.CimDatatypeLib;
using CimBios.Core.CimModel.RdfSerializer;
using CimBios.Core.CimModel.Schema;
using CimBios.Core.DataProvider;

namespace CimBios.Core.CimModel.Document;

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
    public ICimDatatypeLib TypeLib { get; }
}

public interface IModelObjectsProviderFactory
{
    public IModelObjectsProvider Create(Uri source, 
        ICimSchema schema, ICimDatatypeLib? typeLib = null);
}

public class RdfXmlFileModelObjectsProvider : IModelObjectsProvider
{
    public IDataProvider DataProvider { get; }

    public RdfSerializerBase Serializer { get; }

    public ICimSchema CimSchema { get; }

    public ICimDatatypeLib TypeLib { get; }

    public RdfXmlFileModelObjectsProvider(Uri source, 
        ICimSchema schema, 
        ICimDatatypeLib? typeLib = null)
    {
        var provider = new FileStreamDataProvider(source);
        DataProvider = provider;
        CimSchema = schema;
        
        if (typeLib == null)
        {
            TypeLib = new CimDatatypeLib.CimDatatypeLib();
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
        ICimSchema schema, ICimDatatypeLib? typeLib = null)
    {
        return new RdfXmlFileModelObjectsProvider(source, schema, typeLib);
    }
}