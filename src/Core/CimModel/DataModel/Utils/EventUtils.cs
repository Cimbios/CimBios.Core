using CimBios.Core.CimModel.CimDatatypeLib;
using CimBios.Core.CimModel.CimDatatypeLib.EventUtils;

namespace CimBios.Core.CimModel.CimDataModel.Utils;

/// <summary>
/// 
/// </summary>
/// <param name="sender"></param>
/// <param name="modelObject"></param>
/// <param name="e"></param>
public delegate void CimDataModelObjectPropertyChangedEventHandler(
    ICimDataModel? sender, 
    IModelObject modelObject,
    CimMetaPropertyChangedEventArgs e);

/// <summary>
/// 
/// </summary>
/// <param name="sender"></param>
/// <param name="modelObject"></param>
/// <param name="e"></param>
public delegate void CimDataModelObjectStorageChangedEventHandler(
    ICimDataModel? sender, 
    IModelObject modelObject,
    CimDataModelObjectStorageChangedEventArgs e);

/// <summary>
/// 
/// </summary>
/// <param name="changeType"></param>
public class CimDataModelObjectStorageChangedEventArgs 
    (CimDataModelObjectStorageChangeType changeType) 
    : EventArgs
{
    public CimDataModelObjectStorageChangeType ChangeType { get; } = changeType;
}

/// <summary>
/// 
/// </summary>
public enum CimDataModelObjectStorageChangeType
{
    Add,
    Remove,
}
