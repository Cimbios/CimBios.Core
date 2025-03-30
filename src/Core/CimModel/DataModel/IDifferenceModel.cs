using CimBios.Core.CimModel.CimDataModel;
using CimBios.Core.CimModel.CimDatatypeLib;
using CimBios.Core.CimModel.CimDatatypeLib.Headers552;
using CimBios.Utils.ClassTraits;

namespace CimBios.Core.CimModel.CimDifferenceModel;

/// <summary>
/// CIM model differences managment wrapper.
/// </summary>
public interface ICimDifferenceModel : ICanLog
{
    /// <summary>
    /// Model description.
    /// </summary>
    public Model? ModelDescription { get; }

    /// <summary>
    /// Current context differences set.
    /// </summary>
    public IReadOnlyCollection<IDifferenceObject> Differences { get; }

    /// <summary>
    /// Compare CIM data models and push to current differences set.
    /// </summary>
    /// <param name="originDataModel">Origin (left) CIM data model.</param>
    /// <param name="modifiedDataModel">Modified (right) CIM data model.</param>
    public void CompareDataModels(ICimDataModel originDataModel, 
        ICimDataModel modifiedDataModel);

    /// <summary>
    /// Subscribes on CIM data model objects changes. Raising changes are accumulating in cache.
    /// </summary>
    /// <param name="cimDataModel">CIM data model instance.</param>
    public void SubscribeToDataModelChanges(ICimDataModel cimDataModel);

    /// <summary>
    /// Unsubscribe from CIM data model.
    /// </summary>
    public void UnsubscribeFromDataModelChanges();

    /// <summary>
    /// Clear current differences set and internal difference model.
    /// </summary>
    public void ResetAll();
}
