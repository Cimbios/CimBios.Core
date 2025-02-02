using System.ComponentModel;
using CimBios.Core.CimModel.Schema;

namespace CimBios.Core.CimModel.CimDatatypeLib;

/// <summary>
/// Can cancel functionality for PropertyChangingEventArgs.
/// </summary>
public class CanCancelPropertyChangingEventArgs : PropertyChangingEventArgs
{
    public CanCancelPropertyChangingEventArgs(
        ICimMetaProperty metaProperty, bool cancel)
        : base(metaProperty?.BaseUri.AbsoluteUri)
    {
        Cancel = cancel;
    }

    /// <summary>
    /// Cancel property changing flag.
    /// </summary>
    public virtual bool Cancel { get; set; }
}

/// <summary>
/// ICimMetaProperty based implementation of PropertyChangedEventArgs.
/// </summary>
public class CimMetaPropertyChangedEventArgs : PropertyChangedEventArgs
{
    public CimMetaPropertyChangedEventArgs(ICimMetaProperty metaProperty) 
        : base(metaProperty.BaseUri.AbsoluteUri)
    {
    }
}

public delegate void CanCancelPropertyChangingEventHandler(object? sender, 
    CanCancelPropertyChangingEventArgs e);
