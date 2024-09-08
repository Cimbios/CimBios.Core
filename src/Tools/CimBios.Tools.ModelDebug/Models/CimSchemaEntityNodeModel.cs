using System.Collections.ObjectModel;
using CimBios.Core.CimModel.Schema;

namespace CimBios.Tools.ModelDebug.Models;

public class CimSchemaEntityNodeModel : TreeViewNodeModel
{
    public ICimSchemaSerializable CimSchemaEntity { get; }

    public CimSchemaEntityNodeModel(ICimSchemaSerializable cimSchemaEntity)
        : base(cimSchemaEntity.ShortName)
    {
        CimSchemaEntity = cimSchemaEntity;
    }

     public CimSchemaEntityNodeModel(ICimSchemaSerializable cimSchemaEntity,
        ObservableCollection<TreeViewNodeModel> subNodes)
        : base(cimSchemaEntity.ShortName, subNodes)
    {
        CimSchemaEntity = cimSchemaEntity;
    }
}
