using CimBios.Core.CimModel.Schema;

namespace CimBios.Core.CimModel.DatatypeLib.ModelObject;

/// <summary>
/// Base CIM model object interface.
/// </summary>
public interface IModelObjectCore
{
    /// <summary>
    /// Neccesary object identifier.
    /// </summary>
    public string OID { get; }

    /// <summary>
    /// Schema meta class.
    /// </summary>
    public ICimMetaClass MetaClass { get; }

    /// <summary>
    /// Unidentified object status
    /// </summary>
    public bool IsAuto { get; }

    /// <summary>
    /// Check is property exists method.
    /// </summary>
    /// <param name="propertyName">String property name.</param>
    /// <returns>True if exists like attribute or assoc.</returns>
    public bool HasProperty(string propertyName);
}
