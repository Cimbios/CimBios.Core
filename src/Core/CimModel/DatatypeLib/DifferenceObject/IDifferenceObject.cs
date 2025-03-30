using CimBios.Core.CimModel.Schema;

namespace CimBios.Core.CimModel.CimDatatypeLib;

/// <summary>
/// Represents generic changes beetween two model objects or add/del of object.
/// </summary>
public interface IDifferenceObject : IModelObjectCore
{
    /// <summary>
    /// Collection of modified properties of changing object.
    /// </summary>
    public IReadOnlyCollection<ICimMetaProperty> ModifiedProperties { get; }

    /// <summary>
    /// Original object. Null if add or remove difference.
    /// </summary>
    public IReadOnlyModelObject? OriginalObject { get; }

    /// <summary>
    /// Modified object.
    /// </summary>
    public IReadOnlyModelObject ModifiedObject { get; }

    /// <summary>
    /// Set change state for attribute.
    /// </summary>
    /// <param name="metaProperty">CIM attribute meta property.</param>
    /// <param name="fromValue">Origin value.</param>
    /// <param name="toValue">Changed value.</param>
    public void ChangeAttribute (ICimMetaProperty metaProperty, 
        object? fromValue, object? toValue);

    /// <summary>
    /// Set change state for assoc 1 to 1.
    /// </summary>
    /// <param name="metaProperty">CIM assoc 1 to 1 meta property.</param>
    /// <param name="modelObject">Changing association object. Pushes to forward if not null.</param>
    public void ChangeAssoc1 (ICimMetaProperty metaProperty, 
        IModelObject? fromObject, IModelObject? toObject);

    /// <summary>
    /// Set change state for assoc 1 to M. Add statement to forward section.
    /// </summary>
    /// <param name="metaProperty">CIM assoc 1 to M meta property.</param>
    /// <param name="modelObject">Changing association object. Pushes to forward.</param>
    public void AddToAssocM (ICimMetaProperty metaProperty, 
        IModelObject modelObject);

    /// <summary>
    /// Set change state for assoc 1 to M. Add statement to reverse section.
    /// </summary>
    /// <param name="metaProperty">CIM assoc 1 to M meta property.</param>
    /// <param name="modelObject">Changing association object. Pushes to reverse.</param>
    public void RemoveFromAssocM (ICimMetaProperty metaProperty, 
        IModelObject modelObject);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="metaProperty"></param>
    /// <param name="fromValue"></param>
    /// <param name="toValue"></param>
    /// <returns></returns>
    public bool ChangePropertyValue (ICimMetaProperty metaProperty, 
        object? fromValue, object? toValue);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="metaProperty"></param>
    /// <param name="fromValue"></param>
    /// <param name="toValue"></param>
    /// <returns></returns>
    public bool TryGetPropertyValue (ICimMetaProperty metaProperty,
        out object? fromValue, out object? toValue);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="metaProperty"></param>
    public void RemovePropertyChange(ICimMetaProperty metaProperty);
}