using CimBios.Core.CimModel.DatatypeLib.ModelObject;

namespace CimBios.Tools.ModelDebug.Models.CimObjects;

public class CimObjectDataTreeModel : TreeViewNodeModel
{
    public CimObjectDataTreeModel(IModelObject modelObject)
    {
        ModelObject = modelObject;

        Title = $"{Uuid}: {Name}";
    }

    public IModelObject ModelObject { get; set; }

    public string Uuid => ModelObject.OID.ToString();

    public string Name
    {
        get
        {
            if (ModelObject.HasProperty("name")) return ModelObject.GetAttribute<string>("name") ?? "noname";

            return "noname";
        }
    }
}