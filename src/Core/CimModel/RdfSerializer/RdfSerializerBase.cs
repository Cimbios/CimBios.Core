using CimBios.Core.CimModel.CimDatatypeLib;
using CimBios.Core.CimModel.Schema;
using CimBios.Core.DataProvider;

namespace CimBios.Core.CimModel.RdfSerializer;

/// <summary>
/// Base serializer class provides (de)serialization functions.
/// </summary>
public abstract class RdfSerializerBase
{
    protected const string IdentifierPrefix = "#_";

    /// <summary>
    /// Cim schema rules.
    /// </summary>
    public ICimSchema Schema
    { get => _schema; set => _schema = value; }

    /// <summary>
    /// CIM data types library for contrete typed instances creating.
    /// </summary>
    public IDatatypeLib TypeLib
    { get => _typeLib; set => _typeLib = value; }

    /// <summary>
    /// Data provider with source.
    /// </summary>
    public IDataProvider Provider
    { get => _provider; }

    protected RdfSerializerBase(IDataProvider provider,
        ICimSchema schema, IDatatypeLib datatypeLib) 
    {
        _provider = provider;
        _schema = schema;
        _typeLib = datatypeLib;
    }

    /// <summary>
    /// Deserialize data provider data to IModelObject instances.
    /// <param name="settings">Serializer settings.</param>
    /// <returns>Deserializer IModelObject collection.</returns>
    /// </summary>
    public abstract IEnumerable<IModelObject> Deserialize();

    /// <summary>
    /// Serialize IModelObject instances to data provider source.
    /// <param name="modelObjects">IModelObject collection for serialization.</param>
    /// <param name="settings">Serializer settings.</param>
    /// </summary>
    public abstract void Serialize(IEnumerable<IModelObject> modelObjects);

    private ICimSchema _schema;
    private IDatatypeLib _typeLib;
    private IDataProvider _provider;
}
