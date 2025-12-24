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

using CimBios.Core.CimModel.DataModel.Utils;
using CimBios.Core.CimModel.DatatypeLib.DifferenceObject;
using CimBios.Core.CimModel.DatatypeLib.TypeLib;

namespace CimBios.Core.CimModel.DataModel;

/// <summary>
///     CIM model differences managment wrapper.
/// </summary>
public interface ICimDifferenceModel
{
    /// <summary>
    ///     Model description.
    /// </summary>
    public Model? ModelDescription { get; }

    /// <summary>
    ///     Current context differences set.
    /// </summary>
    public IReadOnlyCollection<IDifferenceObject> Differences { get; }

    /// <summary>
    ///     Compare CIM data models and push to current differences set.
    /// </summary>
    /// <param name="originDataModel">Origin (left) CIM data model.</param>
    /// <param name="modifiedDataModel">Modified (right) CIM data model.</param>
    public void CompareDataModels(ICimDataModel originDataModel,
        ICimDataModel modifiedDataModel);

    /// <summary>
    ///     Subscribes on CIM data model objects changes. Raising changes are accumulating in cache.
    /// </summary>
    /// <param name="cimDataModel">CIM data model instance.</param>
    public void SubscribeToDataModelChanges(ICimDataModel cimDataModel);

    /// <summary>
    ///     Unsubscribe from CIM data model.
    /// </summary>
    public void UnsubscribeFromDataModelChanges();

    /// <summary>
    ///     Clear current differences set and internal difference model.
    /// </summary>
    public void ResetAll();
    
    /// <summary>
    ///     Event fires on data model object storage changed - add/remove objects.
    /// </summary>
    public event CimDifferenceModelStorageChangedEventHandler?
        DifferencesStorageChanged;
}