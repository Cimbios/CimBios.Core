using System.Collections.ObjectModel;
using CimBios.Core.CimModel.Schema;

namespace CimBios.Tools.ModelDebug.Models;

public class CimSchemaEntityNodeModel : TreeViewNodeModel
{
    public ICimSchemaSerializable CimSchemaEntity { get; }

    public CimSchemaEntityNodeModel(ICimSchemaSerializable cimSchemaEntity,
        string prefix)
        : base($"{prefix}:{cimSchemaEntity.ShortName}")
    {
        CimSchemaEntity = cimSchemaEntity;
    }

     public CimSchemaEntityNodeModel(ICimSchemaSerializable cimSchemaEntity,
        string prefix,
        ObservableCollection<TreeViewNodeModel> subNodes)
        : base($"{prefix}:{cimSchemaEntity.ShortName}", subNodes)
    {
        CimSchemaEntity = cimSchemaEntity;
    }
}
