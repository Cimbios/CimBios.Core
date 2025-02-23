using CimBios.Core.CimModel.Schema;

namespace CimBios.Core.CimModel.CimDatatypeLib;

/// <summary>
/// 
/// </summary>
public interface IDifferenceObject 
{
    /// <summary>
    /// 
    /// </summary>
    public string OID { get; }

    /// <summary>
    /// 
    /// </summary>
    public IReadOnlyCollection<ICimMetaProperty> ModifiedProperties { get; }

    /// <summary>
    /// 
    /// </summary>
    public IReadOnlyModelObject? OriginalObject { get; }

    /// <summary>
    /// 
    /// </summary>
    public IReadOnlyModelObject ModifiedObject { get; }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="metaProperty"></param>
    /// <param name="fromValue"></param>
    /// <param name="toValue"></param>
    public void ChangeAttribute (ICimMetaProperty metaProperty, 
        object? fromValue, object? toValue);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="metaProperty"></param>
    /// <param name="modelObject"></param>
    public void ChangeAssoc1 (ICimMetaProperty metaProperty, 
        IModelObject? fromObject, IModelObject? toObject);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="metaProperty"></param>
    /// <param name="modelObject"></param>
    public void AddToAssocM (ICimMetaProperty metaProperty, 
        IModelObject modelObject);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="metaProperty"></param>
    /// <param name="modelObject"></param>
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
}