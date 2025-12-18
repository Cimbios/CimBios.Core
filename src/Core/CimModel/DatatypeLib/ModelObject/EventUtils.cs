/*
*    CimBios.Core - Common Information Model (IEC61970) I/O Library
*    Copyright (C) 2025 Yuri A. Kovalenko a.k.a belizahrt <belizahrt@gmail.com>
*
*    This program is free software: you can redistribute it and/or modify
*    it under the terms of the GNU General Public License as published by
*    the Free Software Foundation, either version 3 of the License, or
*    (at your option) any later version.
*
*    This program is distributed in the hope that it will be useful,
*    but WITHOUT ANY WARRANTY; without even the implied warranty of
*    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
*    GNU General Public License for more details.
*
*    You should have received a copy of the GNU General Public License
*    along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/

using System.ComponentModel;
using CimBios.Core.CimModel.Schema;

namespace CimBios.Core.CimModel.DatatypeLib.ModelObject;

/// <summary>
///     Can cancel functionality for PropertyChangingEventArgs.
/// </summary>
public abstract class CanCancelPropertyChangingEventArgs
    : PropertyChangingEventArgs
{
    protected CanCancelPropertyChangingEventArgs(
        ICimMetaProperty metaProperty)
        : base(metaProperty.ShortName)
    {
        MetaProperty = metaProperty;
    }

    public ICimMetaProperty MetaProperty { get; }

    /// <summary>
    ///     Cancel property changing flag.
    /// </summary>
    public virtual bool Cancel { get; set; } = false;
}

/// <summary>
///     Conrete implementation of CanCancelPropertyChangingEventArgs for Attrbiute changed.
/// </summary>
public class CanCancelAttributeChangingEventArgs
    : CanCancelPropertyChangingEventArgs
{
    public CanCancelAttributeChangingEventArgs(
        ICimMetaProperty metaProperty, object? newValue)
        : base(metaProperty)
    {
        if (metaProperty.PropertyKind != CimMetaPropertyKind.Attribute)
            throw new ArgumentException($"Property {metaProperty} is not attribute!");

        NewValue = newValue;
    }

    public object? NewValue { get; }
}

/// <summary>
///     Conrete implementation of CanCancelPropertyChangingEventArgs for Attrbiute changed.
/// </summary>
public class CanCancelAssocChangingEventArgs
    : CanCancelPropertyChangingEventArgs
{
    public CanCancelAssocChangingEventArgs(
        ICimMetaProperty metaProperty, IModelObject? modelObject, bool isRemove)
        : base(metaProperty)
    {
        if (metaProperty.PropertyKind != CimMetaPropertyKind.Assoc1To1
            && metaProperty.PropertyKind != CimMetaPropertyKind.Assoc1ToM)
            throw new ArgumentException($"Property {metaProperty} is not association!");

        ModelObject = modelObject;
        IsRemove = isRemove;
    }

    public IModelObject? ModelObject { get; }
    public bool IsRemove { get; }
}

/// <summary>
///     ICimMetaProperty based implementation of PropertyChangedEventArgs.
/// </summary>
public abstract class CimMetaPropertyChangedEventArgs : PropertyChangedEventArgs
{
    protected CimMetaPropertyChangedEventArgs(ICimMetaProperty metaProperty)
        : base(metaProperty.ShortName)
    {
        MetaProperty = metaProperty;
    }

    public ICimMetaProperty MetaProperty { get; }
}

/// <summary>
///     Conrete implementation of CimMetaPropertyChangedEventArgs for Attrbiute changed.
/// </summary>
public class CimMetaAttributeChangedEventArgs : CimMetaPropertyChangedEventArgs
{
    public CimMetaAttributeChangedEventArgs(ICimMetaProperty metaProperty,
        object? oldValue, object? newValue)
        : base(metaProperty)
    {
        if (metaProperty.PropertyKind != CimMetaPropertyKind.Attribute)
            throw new ArgumentException($"Property {metaProperty} is not attribute!");

        OldValue = oldValue;
        NewValue = newValue;
    }

    public object? OldValue { get; }
    public object? NewValue { get; }
}

/// <summary>
///     Conrete implementation of CimMetaPropertyChangedEventArgs for Assoc changed.
/// </summary>
public class CimMetaAssocChangedEventArgs : CimMetaPropertyChangedEventArgs
{
    public CimMetaAssocChangedEventArgs(ICimMetaProperty metaProperty,
        IModelObject? oldModelObject, IModelObject? newModelObject)
        : base(metaProperty)
    {
        if (metaProperty.PropertyKind != CimMetaPropertyKind.Assoc1To1
            && metaProperty.PropertyKind != CimMetaPropertyKind.Assoc1ToM)
            throw new ArgumentException($"Property {metaProperty} is not association!");

        OldModelObject = oldModelObject;
        NewModelObject = newModelObject;
    }

    public IModelObject? OldModelObject { get; }
    public IModelObject? NewModelObject { get; }
}

public delegate void CanCancelPropertyChangingEventHandler(object? sender,
    CanCancelPropertyChangingEventArgs e);