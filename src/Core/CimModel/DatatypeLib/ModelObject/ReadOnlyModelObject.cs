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
using CimBios.Core.CimModel.DatatypeLib.OID;
using CimBios.Core.CimModel.Schema;

namespace CimBios.Core.CimModel.DatatypeLib.ModelObject;

public class ReadOnlyModelObject : IReadOnlyModelObject
{
    private IReadOnlyModelObject ModelObject { get; }
    public IOIDDescriptor OID => ModelObject.OID;
    public ICimMetaClass MetaClass => ModelObject.MetaClass;

    public ReadOnlyModelObject (IReadOnlyModelObject modelObject)
    {
        ModelObject = modelObject;

        RouteEventsSetup();
    }

    private void RouteEventsSetup()
    {
        ModelObject.PropertyChanging += (_, args) 
            => PropertyChanging?.Invoke(this, args);
        
        ModelObject.PropertyChanged += (_, args) 
            => PropertyChanged?.Invoke(this, args);
    }

    public bool HasProperty(string propertyName)
    {
        return ModelObject.HasProperty(propertyName);
    }

    public void Shrink()
    {
        ModelObject.Shrink();
    }

    public object? GetAttribute(ICimMetaProperty metaProperty)
    {
        return ModelObject.GetAttribute(metaProperty);
    }

    public object? GetAttribute(string attributeName)
    {
        return ModelObject.GetAttribute(attributeName);
    }

    public T? GetAttribute<T>(ICimMetaProperty metaProperty)
    {
        return ModelObject.GetAttribute<T>(metaProperty);
    }

    public T? GetAttribute<T>(string attributeName)
    {
        return ModelObject.GetAttribute<T>(attributeName);
    }

    public T? GetAssoc1To1<T>(ICimMetaProperty metaProperty)
        where T : IModelObject
    {
        return ModelObject.GetAssoc1To1<T>(metaProperty);
    }

    public T? GetAssoc1To1<T>(string assocName) where T : IModelObject
    {
        return ModelObject.GetAssoc1To1<T>(assocName);
    }

    public IModelObject[] GetAssoc1ToM(ICimMetaProperty metaProperty)
    {
        return ModelObject.GetAssoc1ToM(metaProperty);
    }

    public IModelObject[] GetAssoc1ToM(string assocName)
    {
        return ModelObject.GetAssoc1ToM(assocName);
    }

    public T[] GetAssoc1ToM<T>(ICimMetaProperty metaProperty)
        where T : IModelObject
    {
        return ModelObject.GetAssoc1ToM<T>(metaProperty);
    }

    public T[] GetAssoc1ToM<T>(string assocName)
        where T : IModelObject
    {
        return ModelObject.GetAssoc1ToM<T>(assocName);
    }

    public IModelObject? GetAssoc1To1(ICimMetaProperty metaProperty)
    {
        return ModelObject.GetAssoc1To1(metaProperty);
    }

    public IModelObject? GetAssoc1To1(string assocName)
    {
        return ModelObject.GetAssoc1To1(assocName);
    }
    
    public event PropertyChangedEventHandler? PropertyChanged;
    public event CanCancelPropertyChangingEventHandler? PropertyChanging;
}

/// <summary>
///     Extension methods for IReadOnlyModelObject interface.
/// </summary>
public static class IReadOnlyModelObjectExtensions
{
    public static object? TryGetPropertyValue(
        this IReadOnlyModelObject modelObject,
        ICimMetaProperty metaProperty)
    {
        if (metaProperty.PropertyKind == CimMetaPropertyKind.Attribute) return modelObject.GetAttribute(metaProperty);

        if (metaProperty.PropertyKind == CimMetaPropertyKind.Assoc1To1)
            return modelObject.GetAssoc1To1<IModelObject>(metaProperty);

        if (metaProperty.PropertyKind == CimMetaPropertyKind.Assoc1ToM) return modelObject.GetAssoc1ToM(metaProperty);

        throw new NotSupportedException();
    }
}
