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

namespace CimBios.Core.CimModel.DatatypeLib.TypeLib;

/// <summary>
/// Base class of any CIM model document
/// </summary>
[CimClass(ClassUri)]
public class Model(IOIDDescriptor oid, ICimMetaClass metaClass)
    : ModelObject.ModelObject(oid, metaClass)
{
    public const string ClassUri
        = "http://iec.ch/TC57/61970-552/ModelDescription/1#Model";

    /// <summary>
    /// </summary>
    public DateTime? created
    {
        get => GetAttribute<DateTime?>(nameof(created));
        set => SetAttribute(nameof(created), value);
    }

    /// <summary>
    /// </summary>
    public string? description
    {
        get => GetAttribute<string?>(nameof(description));
        set => SetAttribute(nameof(description), value);
    }

    /// <summary>
    /// </summary>
    public Uri? modelingAuthoritySet
    {
        get => GetAttribute<Uri?>(nameof(modelingAuthoritySet));
        set => SetAttribute(nameof(modelingAuthoritySet), value);
    }

    /// <summary>
    /// </summary>
    public Uri? profile
    {
        get => GetAttribute<Uri?>(nameof(profile));
        set => SetAttribute(nameof(profile), value);
    }

    /// <summary>
    /// </summary>
    public DateTime? scenarioTime
    {
        get => GetAttribute<DateTime?>(nameof(scenarioTime));
        set => SetAttribute(nameof(scenarioTime), value);
    }

    /// <summary>
    /// </summary>
    public int? version
    {
        get => GetAttribute<int?>(nameof(version));
        set => SetAttribute(nameof(version), value);
    }

    /// <summary>
    /// </summary>
    public Model[] DependentOn => GetAssoc1ToM<Model>(nameof(DependentOn));

    /// <summary>
    /// </summary>
    public Model[] SupersededBy => GetAssoc1ToM<Model>(nameof(SupersededBy));

    public void AddToDependentOn(Model assocObject)
    {
        AddAssoc1ToM(
            nameof(DependentOn), assocObject);
    }

    public void RemoveFromDependentOn(Model assocObject)
    {
        RemoveAssoc1ToM(
            nameof(DependentOn), assocObject);
    }

    public void RemoveAllFromDependentOn()
    {
        RemoveAllAssocs1ToM(
            nameof(DependentOn));
    }

    public void AddToSupersededBy(Model assocObject)
    {
        AddAssoc1ToM(
            nameof(SupersededBy), assocObject);
    }

    public void RemoveFromSupersededBy(Model assocObject)
    {
        RemoveAssoc1ToM(
            nameof(SupersededBy), assocObject);
    }

    public void RemoveAllFromSupersededBy()
    {
        RemoveAllAssocs1ToM(
            nameof(SupersededBy));
    }
}