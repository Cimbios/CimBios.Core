using System.ComponentModel;
using CimBios.Core.CimModel.Schema;

namespace CimBios.Core.CimModel.CimDatatypeLib;

/// <summary>
/// Can cancel functionality for PropertyChangingEventArgs.
/// </summary>
public abstract class CanCancelPropertyChangingEventArgs 
    : PropertyChangingEventArgs
{
    public ICimMetaProperty MetaProperty { get; }

    protected CanCancelPropertyChangingEventArgs(
        ICimMetaProperty metaProperty)
        : base(metaProperty.ShortName)
    {
        MetaProperty = metaProperty;
    }

    /// <summary>
    /// Cancel property changing flag.
    /// </summary>
    public virtual bool Cancel { get; set; } = false;
}

/// <summary>
/// Conrete implementation of CanCancelPropertyChangingEventArgs for Attrbiute changed.
/// </summary>
public class CanCancelAttributeChangingEventArgs 
    : CanCancelPropertyChangingEventArgs
{
    public object? NewValue { get; }

    public CanCancelAttributeChangingEventArgs(
        ICimMetaProperty metaProperty, object? newValue)
        : base(metaProperty)
    {
        if (metaProperty.PropertyKind != CimMetaPropertyKind.Attribute)
        {
            throw new ArgumentException($"Property {metaProperty} is not attribute!");
        }

        NewValue = newValue;        
    }
}

/// <summary>
/// Conrete implementation of CanCancelPropertyChangingEventArgs for Attrbiute changed.
/// </summary>
public class CanCancelAssocChangingEventArgs 
    : CanCancelPropertyChangingEventArgs
{
    public IModelObject ModelObject { get; }
    public bool IsRemove { get; }

    public CanCancelAssocChangingEventArgs(
        ICimMetaProperty metaProperty, IModelObject modelObject, bool isRemove)
        : base(metaProperty)
    {
        if (metaProperty.PropertyKind != CimMetaPropertyKind.Assoc1To1
            && metaProperty.PropertyKind != CimMetaPropertyKind.Assoc1ToM)
        {
            throw new ArgumentException($"Property {metaProperty} is not association!");
        }

        ModelObject = modelObject; 
        IsRemove = isRemove;    
    }
}

/// <summary>
/// ICimMetaProperty based implementation of PropertyChangedEventArgs.
/// </summary>
public abstract class CimMetaPropertyChangedEventArgs : PropertyChangedEventArgs
{
    public ICimMetaProperty MetaProperty { get; }

    protected CimMetaPropertyChangedEventArgs(ICimMetaProperty metaProperty) 
        : base(metaProperty.ShortName)
    {
        MetaProperty = metaProperty;
    }
}

/// <summary>
/// Conrete implementation of CimMetaPropertyChangedEventArgs for Attrbiute changed.
/// </summary>
public class CimMetaAttributeChangedEventArgs : CimMetaPropertyChangedEventArgs
{
    public object? OldValue { get; }
    public object? NewValue { get; }

    public CimMetaAttributeChangedEventArgs(ICimMetaProperty metaProperty,
        object? oldValue, object? newValue) 
        : base(metaProperty)
    {
        if (metaProperty.PropertyKind != CimMetaPropertyKind.Attribute)
        {
            throw new ArgumentException($"Property {metaProperty} is not attribute!");
        }

        OldValue = oldValue;
        NewValue = newValue;
    }
}

/// <summary>
/// Conrete implementation of CimMetaPropertyChangedEventArgs for Assoc changed.
/// </summary>
public class CimMetaAssocChangedEventArgs : CimMetaPropertyChangedEventArgs
{
    public IModelObject? OldModelObject { get; }
    public IModelObject? NewModelObject { get; }

    public CimMetaAssocChangedEventArgs(ICimMetaProperty metaProperty,
        IModelObject? oldModelObject, IModelObject? newModelObject) 
        : base(metaProperty)
    {
        if (metaProperty.PropertyKind != CimMetaPropertyKind.Assoc1To1
            && metaProperty.PropertyKind != CimMetaPropertyKind.Assoc1ToM)
        {
            throw new ArgumentException($"Property {metaProperty} is not association!");
        }

        OldModelObject = oldModelObject;
        NewModelObject = newModelObject;
    }
}

public delegate void CanCancelPropertyChangingEventHandler(object? sender, 
    CanCancelPropertyChangingEventArgs e);
