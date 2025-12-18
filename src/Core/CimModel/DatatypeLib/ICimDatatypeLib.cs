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

using System.Reflection;
using CimBios.Core.CimModel.DatatypeLib.ModelObject;
using CimBios.Core.CimModel.DatatypeLib.OID;
using CimBios.Core.CimModel.Schema;

namespace CimBios.Core.CimModel.DatatypeLib;

/// <summary>
///     Structure interface for datatype lib.
/// </summary>
public interface ICimDatatypeLib
{
    /// <summary>
    ///     Applied to type lib schema.
    /// </summary>
    public ICimSchema Schema { get; }

    /// <summary>
    ///     Dictionary Uri to Type of IModelObject concrete classes.
    /// </summary>
    public IReadOnlyDictionary<ICimMetaClass, Type> RegisteredTypes { get; }

    /// <summary>
    ///     Load assembly by file path.
    /// </summary>
    /// <param name="typesAssemblyPath">Path to assembly .dll file.</param>
    /// <param name="reset">Clear current assemblies collection.</param>
    public void LoadAssembly(string typesAssemblyPath, bool reset = true);

    /// <summary>
    ///     Load assembly by file path.
    /// </summary>
    /// <param name="typesAssembly">Assembly object.</param>
    /// <param name="reset">Clear current assemblies collection.</param>
    public void LoadAssembly(Assembly typesAssembly, bool reset = true);

    /// <summary>
    ///     Register new type in datatype library.
    /// </summary>
    /// <param name="type">Type for register.</param>
    public void RegisterType(Type type);

    /// <summary>
    ///     Create instance of schema meta class.
    /// </summary>
    /// <param name="modelObjectFactory">Model object factory.</param>
    /// <param name="oid">OID of creating instance.</param>
    /// <param name="metaClass">Cim schema meta class.</param>
    /// <param name="isAuto">Is auto class attribute.</param>
    /// <returns>IModelObject instance of meta type.</returns>
    public IModelObject? CreateInstance(IModelObjectFactory modelObjectFactory,
        IOIDDescriptor oid, ICimMetaClass metaClass);

    /// <summary>
    ///     Create instance of schema meta class.
    /// </summary>
    /// <typeparam name="T">IModelObject CIM lib type.</typeparam>
    /// <param name="oid">OID of creating instance.</param>
    /// <param name="metaClass">Cim schema meta class.</param>
    /// <param name="isAuto">Is auto class attribute.</param>
    /// <returns>IModelObject instance of meta type.</returns>
    public T? CreateInstance<T>(IOIDDescriptor oid)
        where T : class, IModelObject;

    /// <summary>
    ///     Create enum value instance of meta individual type.
    /// </summary>
    /// <typeparam name="TEnum">Typed enum generic value.</typeparam>
    /// <param name="metaIndividual">Meta individual instance.</param>
    /// <returns></returns>
    public EnumValueObject? CreateEnumValueInstance(
        ICimMetaIndividual metaIndividual);

    /// <summary>
    ///     Create enum value instance of meta individual type.
    /// </summary>
    /// <typeparam name="TEnum">Typed enum generic value.</typeparam>
    /// <param name="enumValue">Typed enum value instance.</param>
    /// <returns></returns>
    public EnumValueObject<TEnum>? CreateEnumValueInstance<TEnum>(
        TEnum enumValue) where TEnum : struct, Enum;

    /// <summary>
    ///     Create compound meta class instance.
    /// </summary>
    /// <param name="modelObjectFactory">Model object factory.</param>
    /// <param name="metaClass">Cim schema meta class.</param>
    /// <returns></returns>
    public IModelObject? CreateCompoundInstance(
        IModelObjectFactory modelObjectFactory, ICimMetaClass metaClass);

    /// <summary>
    ///     Create compound meta class instance of type T.
    /// </summary>
    /// <typeparam name="T">Type lib CIM type.</typeparam>
    /// <returns></returns>
    public T? CreateCompoundInstance<T>() where T : class, IModelObject;
}