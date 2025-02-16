using CimBios.Core.CimModel.CimDatatypeLib;
using CimBios.Core.CimModel.Schema;

namespace CimBios.Core.CimModel.DatatypeLib.ModelObject;

/// <summary>
/// Read abillities interface of CIM model object.
/// </summary>
public interface IReadOnlyModelObject : IModelObjectCore
{
    /// <summary>
    /// Get attribute value by meta property instance. 
    /// Throws exception if property does not exists.
    /// </summary>
    /// <param name="metaProperty">Schema meta property instance.</param>
    /// <returns>Value.</returns>
    public object? GetAttribute(ICimMetaProperty metaProperty);

    /// <summary>
    /// Get attribute value by property name. 
    /// Throws exception if property does not exists.
    /// </summary>
    /// <param name="attributeName">Attribute name in format of '(Domain.)Attribute'.</param>
    /// <returns>Value.</returns>
    public object? GetAttribute(string attributeName);

    /// <summary>
    /// Get attribute typed T value. Throws exception if property does not exists.
    /// </summary>
    /// <param name="metaProperty">Schema meta property instance.</param>
    /// <returns>Typed value.</returns>
    public T? GetAttribute<T>(ICimMetaProperty metaProperty);

    /// <summary>
    /// Get attribute typed T value. Throws exception if property does not exists.
    /// </summary>
    /// <param name="attributeName">Attribute name in format of '(Domain.)Attribute'.</param>
    /// <returns>Typed value.</returns>
    public T? GetAttribute<T>(string attributeName);

    /// <summary>
    /// Get 1 to 1 assoc object. Throws exception if property does not exists.
    /// </summary>
    /// <param name="metaProperty">Schema meta property instance.</param>
    /// <returns>IModelObject instance.</returns>
    public T? GetAssoc1To1<T>(ICimMetaProperty metaProperty) where T: IModelObject;

    /// <summary>
    /// Get 1 to 1 assoc object. Throws exception if property does not exists.
    /// </summary>
    /// <param name="assocName">Assoc name in format of '(Domain.)Assoc'.</param>
    /// <returns>IModelObject instance.</returns>
    public T? GetAssoc1To1<T>(string assocName) where T: IModelObject;

    /// <summary>
    /// Get 1 to M assoc objects. Throws exception if property does not exists.
    /// </summary>
    /// <param name="metaProperty">Schema meta property instance.</param>
    /// <returns>IModelObject instances array.</returns>
    public IModelObject[] GetAssoc1ToM(ICimMetaProperty metaProperty);

    /// <summary>
    /// Get 1 to M assoc objects. Throws exception if property does not exists.
    /// </summary>
    /// <param name="assocName">Assoc name in format of 'Domain.Assoc'.</param>
    /// <returns>IModelObject instances array.</returns>
    public IModelObject[] GetAssoc1ToM(string assocName);

    /// <summary>
    /// Get 1 to M assoc objects. Throws exception if property does not exists.
    /// </summary>
    /// <param name="metaProperty">Schema meta property instance.</param>
    /// <returns>IModelObject instances array.</returns>
    public T[] GetAssoc1ToM<T>(ICimMetaProperty metaProperty) 
        where T: IModelObject;

    /// <summary>
    /// Get 1 to M assoc objects. Throws exception if property does not exists.
    /// </summary>
    /// <param name="assocName">Assoc name in format of 'Domain.Assoc'.</param>
    /// <returns>IModelObject instances array.</returns>
    public T[] GetAssoc1ToM<T>(string assocName)
        where T: IModelObject;
}
