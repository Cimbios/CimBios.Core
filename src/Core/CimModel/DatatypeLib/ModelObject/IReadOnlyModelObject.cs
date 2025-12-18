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
///     Read abillities interface of CIM model object.
/// </summary>
public interface IReadOnlyModelObject : INotifyPropertyChanged, IModelObjectCore
{
    /// <summary>
    ///     Get attribute value by meta property instance.
    ///     Throws exception if property does not exists.
    /// </summary>
    /// <param name="metaProperty">Schema meta property instance.</param>
    /// <returns>Value.</returns>
    public object? GetAttribute(ICimMetaProperty metaProperty);

    /// <summary>
    ///     Get attribute value by property name.
    ///     Throws exception if property does not exists.
    /// </summary>
    /// <param name="attributeName">Attribute name in format of '(Domain.)Attribute'.</param>
    /// <returns>Value.</returns>
    public object? GetAttribute(string attributeName);

    /// <summary>
    ///     Get attribute typed T value. Throws exception if property does not exists.
    /// </summary>
    /// <param name="metaProperty">Schema meta property instance.</param>
    /// <returns>Typed value.</returns>
    public T? GetAttribute<T>(ICimMetaProperty metaProperty);

    /// <summary>
    ///     Get attribute typed T value. Throws exception if property does not exists.
    /// </summary>
    /// <param name="attributeName">Attribute name in format of '(Domain.)Attribute'.</param>
    /// <returns>Typed value.</returns>
    public T? GetAttribute<T>(string attributeName);

    /// <summary>
    ///     Get 1 to 1 assoc object. Throws exception if property does not exists.
    /// </summary>
    /// <param name="metaProperty">Schema meta property instance.</param>
    /// <returns>IModelObject instance.</returns>
    public T? GetAssoc1To1<T>(ICimMetaProperty metaProperty) where T : IModelObject;

    /// <summary>
    ///     Get 1 to 1 assoc object. Throws exception if property does not exists.
    /// </summary>
    /// <param name="assocName">Assoc name in format of '(Domain.)Assoc'.</param>
    /// <returns>IModelObject instance.</returns>
    public T? GetAssoc1To1<T>(string assocName) where T : IModelObject;

    /// <summary>
    ///     Get 1 to 1 assoc object. Throws exception if property does not exists.
    /// </summary>
    /// <param name="metaProperty">Schema meta property instance.</param>
    /// <returns>IModelObject instance.</returns>
    public IModelObject? GetAssoc1To1(ICimMetaProperty metaProperty);

    /// <summary>
    ///     Get 1 to 1 assoc object. Throws exception if property does not exists.
    /// </summary>
    /// <param name="assocName">Assoc name in format of '(Domain.)Assoc'.</param>
    /// <returns>IModelObject instance.</returns>
    public IModelObject? GetAssoc1To1(string assocName);

    /// <summary>
    ///     Get 1 to M assoc objects. Throws exception if property does not exists.
    /// </summary>
    /// <param name="metaProperty">Schema meta property instance.</param>
    /// <returns>IModelObject instances array.</returns>
    public IModelObject[] GetAssoc1ToM(ICimMetaProperty metaProperty);

    /// <summary>
    ///     Get 1 to M assoc objects. Throws exception if property does not exists.
    /// </summary>
    /// <param name="assocName">Assoc name in format of 'Domain.Assoc'.</param>
    /// <returns>IModelObject instances array.</returns>
    public IModelObject[] GetAssoc1ToM(string assocName);

    /// <summary>
    ///     Get 1 to M assoc objects. Throws exception if property does not exists.
    /// </summary>
    /// <param name="metaProperty">Schema meta property instance.</param>
    /// <returns>IModelObject instances array.</returns>
    public T[] GetAssoc1ToM<T>(ICimMetaProperty metaProperty)
        where T : IModelObject;

    /// <summary>
    ///     Get 1 to M assoc objects. Throws exception if property does not exists.
    /// </summary>
    /// <param name="assocName">Assoc name in format of 'Domain.Assoc'.</param>
    /// <returns>IModelObject instances array.</returns>
    public T[] GetAssoc1ToM<T>(string assocName)
        where T : IModelObject;
    
    /// <summary>
    ///     Event fires before changing property value.
    /// </summary>
    public event CanCancelPropertyChangingEventHandler? PropertyChanging;
}
