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

public class EnumValueObject : object
{
    internal EnumValueObject(ICimMetaIndividual metaIndividual)
    {
        MetaEnumValue = metaIndividual;
    }

    internal EnumValueObject(ICimMetaIndividual metaIndividual, Type enumType)
        : this(metaIndividual)
    {
        if (enumType.IsEnum == false) throw new InvalidEnumArgumentException("Non enum type received!");

        EnumType = enumType;
    }

    /// <summary>
    ///     Core schema meta individual.
    /// </summary>
    public ICimMetaIndividual MetaEnumValue { get; }

    /// <summary>
    ///     Typed translation of meta individual. Null if does not exist in typelib.
    /// </summary>
    public virtual Type? EnumType { get; }

    public EnumValueObject<TEnum> Cast<TEnum>()
        where TEnum : struct, Enum
    {
        if (this is EnumValueObject<TEnum> self) return self;

        return new EnumValueObject<TEnum>(MetaEnumValue);
    }

    public virtual object AsEnum()
    {
        if (EnumType != null
            && Enum.TryParse(EnumType,
                MetaEnumValue.ShortName, out var enumValue))
            return enumValue;

        throw new InvalidCastException();
    }

    public virtual object AsEnum(Type enumType)
    {
        if (enumType.IsEnum && Enum.TryParse(enumType,
                MetaEnumValue.ShortName, out var enumValue))
            return enumValue;

        throw new InvalidCastException();
    }

    public override string ToString()
    {
        return MetaEnumValue.ShortName;
    }

    public static implicit operator Uri(
        EnumValueObject enumValueObject)
    {
        return enumValueObject.MetaEnumValue.BaseUri;
    }

    public static implicit operator Enum(
        EnumValueObject enumValueObject)
    {
        if (enumValueObject.EnumType == null) throw new InvalidCastException();

        return (Enum)enumValueObject.AsEnum();
    }

    public static bool operator ==(EnumValueObject? left,
        EnumValueObject? right)
    {
        if (left is null && right is null) return true;

        if (left is not null) return left.Equals(right);

        if (right is not null) return right.Equals(left);

        return false;
    }

    public static bool operator !=(EnumValueObject? left,
        EnumValueObject? right)
    {
        return !(left == right);
    }

    public override bool Equals(object? obj)
    {
        if (obj is not EnumValueObject rightEV) return base.Equals(obj);

        return MetaEnumValue.Equals(rightEV.MetaEnumValue);
    }

    public override int GetHashCode()
    {
        return MetaEnumValue.BaseUri.GetHashCode();
    }
}

/// <summary>
///     Typed enum wrapper for meta individual instances. Provides enum value in
///     IModelObject entities.
/// </summary>
/// <typeparam name="TEnum">Enum typed generic type.</typeparam>
public sealed class EnumValueObject<TEnum> : EnumValueObject
    where TEnum : struct, Enum
{
    internal EnumValueObject(ICimMetaIndividual metaIndividual)
        : base(metaIndividual)
    {
        if (Enum.TryParse<TEnum>(metaIndividual.ShortName,
                out var typedEnumValue))
            TypedEnumValue = typedEnumValue;
        else
            throw new InvalidEnumArgumentException(
                $"Meta enum value {metaIndividual.ShortName} does not accord to enum type {typeof(TEnum).Name}");
    }

    public override Type? EnumType => TypedEnumValue.GetType();

    public TEnum TypedEnumValue { get; }

    public override object AsEnum()
    {
        return TypedEnumValue;
    }

    public static implicit operator Uri(
        EnumValueObject<TEnum> enumValueObject)
    {
        return enumValueObject.MetaEnumValue.BaseUri;
    }

    public static implicit operator TEnum(
        EnumValueObject<TEnum> enumValueObject)
    {
        return enumValueObject.TypedEnumValue;
    }
}

/// <summary>
///     Extension for wrap enum value before set attribute.
/// </summary>
public static class ModelObjectSetEnumExtension
{
    public static void SetAttributeAsEnum(this IModelObject modelObject,
        ICimMetaProperty metaProperty, ICimMetaIndividual metaIndividual)
    {
        if (modelObject is DynamicModelObjectBase 
                { InternalTypeLib: not null } dynamicModelObject)
        {
            var wrappedTypedEnumValue = dynamicModelObject.InternalTypeLib
                .CreateEnumValueInstance(metaIndividual);

            modelObject.SetAttribute(metaProperty, wrappedTypedEnumValue);
            return;
        }

        var wrappedEnumValue = new EnumValueObject(metaIndividual);
        modelObject.SetAttribute(metaProperty, wrappedEnumValue);
    }

    public static void SetAttributeAsEnum(this IModelObject modelObject,
        ICimMetaProperty metaProperty, Enum enumValue)
    {
        if (metaProperty.PropertyDatatype == null
            || metaProperty.PropertyDatatype.IsEnum == false)
            throw new InvalidEnumArgumentException();

        var metaIndividual = metaProperty.PropertyDatatype.AllIndividuals
                                 .FirstOrDefault(i => i.ShortName == enumValue.ToString())
                             ?? throw new InvalidEnumArgumentException();

        var wrappedEnumValue = new EnumValueObject(metaIndividual,
            enumValue.GetType());

        modelObject.SetAttribute(metaProperty, wrappedEnumValue);
    }

    public static void SetAttributeAsEnum(this IModelObject modelObject,
        string attributeName, Enum enumValue)
    {
        if (modelObject.HasProperty(attributeName) == false) throw new InvalidEnumArgumentException();

        var metaProperty = modelObject.MetaClass
            .AllProperties.Where(p => p.ShortName == attributeName)
            .First();

        modelObject.SetAttributeAsEnum(metaProperty, enumValue);
    }
}