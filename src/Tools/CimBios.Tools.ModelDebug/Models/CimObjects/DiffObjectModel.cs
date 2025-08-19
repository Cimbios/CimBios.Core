using CimBios.Core.CimModel.CimDatatypeLib;

namespace CimBios.Tools.ModelDebug.Models.CimObjects;

public class DiffObjectModel(IDifferenceObject differenceObject) 
    : TreeViewNodeModel
{
    public IDifferenceObject DifferenceObject { get; } = differenceObject;

    public DiffModelType Type
    {
        get
        {
            return DifferenceObject switch
            {
                UpdatingDifferenceObject => DiffModelType.Change,
                AdditionDifferenceObject => DiffModelType.Add,
                DeletionDifferenceObject => DiffModelType.Remove,
                _ => DiffModelType.Unknown
            };
        }
    }
}

public enum DiffModelType
{
    Unknown,
    Change,
    Add,
    Remove
}