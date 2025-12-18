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

namespace CimBios.Core.CimModel.Schema.AutoSchema;

/// <summary>
///     Schema auto class entity. Does not provide inheritance chain - only plain.
/// </summary>
public class CimAutoClass(Uri baseUri, string shortName, string description)
    : CimMetaClassBase(baseUri, shortName, description),
        ICimMetaClass, ICimMetaExtensible
{
    public override void AddProperty(ICimMetaProperty metaProperty)
    {
        if (HasProperty(metaProperty) == false) _Properties.Add(metaProperty);
    }

    public void SetIsEnum(bool isEnum)
    {
        IsEnum = isEnum;
    }

    public void SetIsCompound(bool isCompound)
    {
        IsCompound = isCompound;
    }

    public void SetIsAbstract(bool isAbstract)
    {
        IsAbstract = isAbstract;
    }
}

public class CimAutoProperty(Uri baseUri, string shortName, string description)
    : CimMetaPropertyBase(baseUri, shortName, description),
        ICimMetaProperty
{
    public void SetPropertyKind(CimMetaPropertyKind propertyKind)
    {
        PropertyKind = propertyKind;
    }

    public void SetPropertyDatatype(ICimMetaClass? propertyDatatype)
    {
        PropertyDatatype = propertyDatatype;
    }
}

public class CimAutoDatatype(Uri baseUri, string shortName, string description)
    : CimAutoClass(baseUri, shortName, description),
        ICimMetaDatatype
{
    public Type? SystemType { get; set; }
    public Type PrimitiveType => SystemType ?? typeof(string);
}

public class CimAutoIndividual(Uri baseUri, string shortName, string description)
    : CimMetaIndividualBase(baseUri, shortName, description),
        ICimMetaIndividual
{
}