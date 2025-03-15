using CimBios.Core.CimModel.CimDatatypeLib.OID;
using CimBios.Core.CimModel.Schema;

namespace CimBios.Core.CimModel.CimDatatypeLib;

/// <summary>
/// Base CIM model object interface.
/// </summary>
public interface IModelObjectCore
{
    /// <summary>
    /// Core typelib factory.
    /// </summary>
    public ICimDatatypeLib? TypeLib { get; }

    /// <summary>
    /// Neccesary object identifier.
    /// </summary>
    public IOIDDescriptor OID { get; }

    /// <summary>
    /// Schema meta class.
    /// </summary>
    public ICimMetaClass MetaClass { get; }

    /// <summary>
    /// Check is property exists method.
    /// </summary>
    /// <param name="propertyName">String property name.</param>
    /// <returns>True if exists like attribute or assoc.</returns>
    public bool HasProperty(string propertyName);
}
