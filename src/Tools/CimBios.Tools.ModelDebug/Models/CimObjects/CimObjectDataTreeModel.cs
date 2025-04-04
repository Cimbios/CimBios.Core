using System;
using System.Linq;
using CimBios.Core.CimModel.CimDatatypeLib;

namespace CimBios.Tools.ModelDebug.Models;

public class CimObjectDataTreeModel : TreeViewNodeModel
{
    public IModelObject ModelObject { get; set; }

    public string Uuid { get => ModelObject.OID.ToString(); }

    public string Name 
    {
        get
        {
            if (ModelObject.HasProperty("name"))
            {
                return ModelObject.GetAttribute<string>("name") ?? "noname";
            }

            return "noname";
        }
    }

    public CimObjectDataTreeModel(IModelObject modelObject) : base()
    {
        ModelObject = modelObject;

        Title = $"{Uuid}: {Name}";
    }
}
