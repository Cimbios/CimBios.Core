using CimBios.Core.CimModel.CimDataModel;
using CimBios.Core.CimModel.CimDatatypeLib;
using CimBios.Utils.ClassTraits;

namespace CimBios.Core.CimModel.CimDifferenceModel;

/// <summary>
/// 
/// </summary>
public interface ICimDifferenceModel : ICanLog
{
    public IReadOnlyCollection<IDifferenceObject> Differences { get; }

    public void ApplyToDataModel(ICimDataModel cimDataModel);
    public void ExtractFromDataModel(ICimDataModel cimDataModel);
    public void FitToDataModelSchema(ICimDataModel cimDataModel);

    public void ResetAll();

    // compare
    // load
    // save
    // apply

}
