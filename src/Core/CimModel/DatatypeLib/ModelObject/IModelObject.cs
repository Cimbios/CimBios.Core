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

using CimBios.Core.CimModel.DatatypeLib.OID;
using CimBios.Core.CimModel.Schema;

namespace CimBios.Core.CimModel.DatatypeLib.ModelObject;

/// <summary>
///     CIM object abstaction view.
///     Provides read and modification logic with data validation.
/// </summary>
public interface IModelObject : IReadOnlyModelObject
{
    /// <summary>
    ///     Set attribute typed T value.
    /// </summary>
    /// <param name="metaProperty">Schema meta property instance.</param>
    /// <param name="value">Typed value.</param>
    public void SetAttribute<T>(ICimMetaProperty metaProperty, T? value);

    /// <summary>
    ///     Set attribute typed T value.
    /// </summary>
    /// <param name="attributeName">Attribute name in format of '(Domain.)Attribute'.</param>
    /// <param name="value">Typed value.</param>
    public void SetAttribute<T>(string attributeName, T? value);

    /// <summary>
    ///     Create compound model object for meta property attribute.
    /// </summary>
    /// <param name="metaProperty">Schema compound meta property instance.</param>
    /// <param name="reset">Recreate compound if already exists.</param>
    public IModelObject InitializeCompoundAttribute(ICimMetaProperty metaProperty,
        bool reset = true);

    /// <summary>
    ///     Create compound model object for meta property attribute.
    /// </summary>
    /// <param name="attributeName">Attribute name in format of '(Domain.)Attribute'.</param>
    /// <param name="reset">Recreate compound if already exists.</param>
    public IModelObject InitializeCompoundAttribute(string attributeName,
        bool reset = true);

    /// <summary>
    ///     Set 1 to 1 assoc object or clear assoc if obj is null.
    /// </summary>
    /// <param name="metaProperty">Schema meta property instance.</param>
    /// <param name="obj">IModelObject instance.</param>
    public void SetAssoc1To1(ICimMetaProperty metaProperty, IModelObject? obj);

    /// <summary>
    ///     Set 1 to 1 assoc object.
    /// </summary>
    /// <param name="assocName">Assoc name in format of 'Domain.Assoc'.</param>
    /// <param name="obj">IModelObject instance.</param>
    public void SetAssoc1To1(string assocName, IModelObject? obj);

    /// <summary>
    ///     Add 1 to M assoc beetween objects.
    /// </summary>
    /// <param name="metaProperty">Schema meta property instance.</param>
    /// <param name="obj">IModelObject associated instance.</param>
    public void AddAssoc1ToM(ICimMetaProperty metaProperty, IModelObject obj);

    /// <summary>
    ///     Add 1 to M assoc beetween objects.
    /// </summary>
    /// <param name="assocName">Assoc name in format of '(Domain.)Assoc'.</param>
    /// <param name="obj">IModelObject associated instance.</param>
    public void AddAssoc1ToM(string assocName, IModelObject obj);

    /// <summary>
    ///     Remove 1 to M assoc beetween objects.
    /// </summary>
    /// <param name="metaProperty">Schema meta property instance.</param>
    /// <param name="obj">IModelObject associated instance.</param>
    public void RemoveAssoc1ToM(ICimMetaProperty metaProperty, IModelObject obj);

    /// <summary>
    ///     Remove 1 to M assoc beetween objects.
    /// </summary>
    /// <param name="assocName">Assoc name in format of '(Domain.)Assoc'.</param>
    /// <param name="obj">IModelObject associated instance.</param>
    public void RemoveAssoc1ToM(string assocName, IModelObject obj);

    /// <summary>
    ///     Remove all 1 to M assocs beetween objects.
    /// </summary>
    /// <param name="metaProperty">Schema meta property instance.</param>
    public void RemoveAllAssocs1ToM(ICimMetaProperty metaProperty);

    /// <summary>
    ///     Remove all 1 to M assocs beetween objects.
    /// </summary>
    /// <param name="assocName">Assoc name in format of '(Domain.)Assoc'.</param>
    public void RemoveAllAssocs1ToM(string assocName);

    /// <summary>
    ///     Get read only wrapper for model object.
    /// </summary>
    /// <returns>IModelObject instances array.</returns>
    public IReadOnlyModelObject AsReadOnly();
}

/// <summary>
///     Model object factory provides activation method.
/// </summary>
public interface IModelObjectFactory
{
    /// <summary>
    ///     Factory producing type info.
    /// </summary>
    public Type ProduceType { get; }

    /// <summary>
    ///     Create IModelObject instance.
    /// </summary>
    /// <param name="uuid">Object uuid.</param>
    /// <param name="metaClass">Schema meta class.</param>
    /// <param name="isAuto">Is creating object auto.</param>
    /// <returns>IModelObject instance.</returns>
    public IModelObject Create(IOIDDescriptor oid, ICimMetaClass metaClass);
}
