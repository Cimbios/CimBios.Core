using CimBios.Core.CimModel.CimDatatypeLib;
using CimBios.Core.CimModel.CimDatatypeLib.EventUtils;
using CimBios.Core.CimModel.Schema;

namespace CimBios.Core.CimModel.CimDataModel;

public interface ICimDataModelChangeStatement
{
    public IModelObject ModelObject { get; }
}

/// <summary>
/// Abstract change statement class. Provides only model object related with change. 
/// </summary>
public abstract class CimDataModelChangeStatementBase (IModelObject modelObject)
    : ICimDataModelChangeStatement
{
    public IModelObject ModelObject { get; } = modelObject;
}

/// <summary>
/// Change statement class provides adding statement.
/// </summary>
public sealed class CimDataModelObjectAddedStatement (IModelObject modelObject)
    : CimDataModelChangeStatementBase (modelObject)
{
}

/// <summary>
/// Change statement class provides removing statement.
/// </summary>
public sealed class CimDataModelObjectRemovedStatement (IModelObject modelObject)
    : CimDataModelChangeStatementBase (modelObject)
{
}

/// <summary>
/// Change statement class provides update statement.
/// </summary>
public sealed class CimDataModelObjectUpdatedStatement
    : CimDataModelChangeStatementBase 
{
    public ICimMetaProperty MetaProperty { get; }
    public object? OldValue { get; }
    public object? NewValue { get; }

    public CimDataModelObjectUpdatedStatement(IModelObject modelObject,
        CimMetaPropertyChangedEventArgs eventArgs)
        : base(modelObject)
    {
        MetaProperty = eventArgs.MetaProperty;

        if (eventArgs is CimMetaAttributeChangedEventArgs attrEv)
        {
            OldValue = attrEv.OldValue;
            NewValue = attrEv.NewValue;
        }
        else if (eventArgs is CimMetaAssocChangedEventArgs assocEv)
        {
            OldValue = assocEv.OldModelObject;
            NewValue = assocEv.NewModelObject;
        }
        else
        {
            throw new NotSupportedException("Unknown CimMetaPropertyChangedEventArgs type!");
        }
    }
}

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
