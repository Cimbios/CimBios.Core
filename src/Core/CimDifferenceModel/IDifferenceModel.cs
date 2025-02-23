using System.Collections.ObjectModel;
using System.Text;
using CimBios.Core.CimModel.CimDataModel;
using CimBios.Utils.ClassTraits;

namespace CimBios.Core.CimDifferenceModel;

/// <summary>
/// 
/// </summary>
public interface ICimDifferenceModel : ICanLog
{
    public IReadOnlyCollection<IDifferenceObject> Differences { get; }

    public void Load(StreamReader streamReader);
    public void Load(string path);
    public void Parse(string content, Encoding? encoding = null);

    public void Save(StreamWriter streamWriter);
    public void Save(string path);

    public void ExtractFromDataModel(ICimDataModel cimDataModel);
    public void InvalidateDataWithModel(ICimDataModel cimDataModel);

    public void ResetAll();

    // forward
    // reference
    // compare
    // load
    // save
    // apply
    // subscribe
    // invalidate
}
